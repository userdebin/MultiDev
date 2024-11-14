using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public const string KEY_GAME_MODE = "GameMode";
    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_CHARACTER = "Character";

    public event EventHandler OnLeftLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<LobbyEventArgs> OnLobbyGameModeChanged;

    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    public enum GameMode
    {
        KillForAll,
        ComingSoon
    }

    public enum PlayerCharacter
    {
        Marine,
        Ninja,
        Zombie
    }

    private float heartbeatTimer;
    private float lobbyPollTimer;
    private Lobby joinedLobby;
    private string playerName;

    [SerializeField] private Button startButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (startButton != null)
        {
            startButton.gameObject.SetActive(false);
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    private void OnStartButtonClicked()
    {
        if (IsLobbyHost())
        {
            UILogManager.Instance.DisplayLog("Starting game as host...");
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("PlayingField", LoadSceneMode.Single);
        }
    }

    public async void Authenticate(string playerName)
    {
        this.playerName = playerName;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            UILogManager.Instance.DisplayLog("Signed in! Player ID: " + AuthenticationService.Instance.PlayerId);
            RefreshLobbyList();
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = 15f;
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyPolling()
    {
        if (joinedLobby != null)
        {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                lobbyPollTimer = 1.1f;
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                if (!IsPlayerInLobby())
                {
                    UILogManager.Instance.DisplayLog("Kicked from Lobby!");
                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                    joinedLobby = null;
                }
                else
                {
                    UpdateStartButtonVisibility();
                    CheckAutoJoinAsClient(); // Cek dan otomatis join sebagai client jika host sudah memulai
                }
            }
        }
    }

    private void UpdateStartButtonVisibility()
    {
        if (startButton != null)
        {
            startButton.gameObject.SetActive(IsLobbyHost());
        }
    }

    private void CheckAutoJoinAsClient()
    {
        if (!IsLobbyHost() && NetworkManager.Singleton != null && !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            UILogManager.Instance.DisplayLog("Joining game as client...");
            NetworkManager.Singleton.StartClient();
        }
    }

    public Lobby GetJoinedLobby() => joinedLobby;

    public bool IsLobbyHost() => joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;

    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private Player GetPlayer()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject>
        {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
            { KEY_PLAYER_CHARACTER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerCharacter.Marine.ToString()) }
        });
    }

    public void ChangeGameMode(GameMode newGameMode)
    {
        if (IsLobbyHost())
        {
            UpdateLobbyGameMode(newGameMode);
        }
        else
        {
            UILogManager.Instance.DisplayLog("Only the host can change the game mode.");
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, GameMode gameMode)
    {
        Player player = GetPlayer();
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Player = player,
            IsPrivate = isPrivate,
            Data = new Dictionary<string, DataObject>
            {
                { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) }
            }
        };

        joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        UILogManager.Instance.DisplayLog("Created Lobby: " + joinedLobby.Name);

        UpdateStartButtonVisibility();
    }

    public async void RefreshLobbyList()
    {
        try
        {
            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            UILogManager.Instance.DisplayLog("Error refreshing lobby list: " + e.Message);
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        Player player = GetPlayer();
        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions { Player = player });
        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

        UpdateStartButtonVisibility();
        CheckAutoJoinAsClient();
    }

    public async void UpdatePlayerName(string newName)
    {
        if (joinedLobby != null)
        {
            try
            {
                UpdatePlayerOptions options = new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, newName) }
                    }
                };

                joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, options);
                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            }
            catch (LobbyServiceException e)
            {
                UILogManager.Instance.DisplayLog("Error updating player name: " + e.Message);
            }
        }
    }

    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                UILogManager.Instance.DisplayLog("Error kicking player: " + e.Message);
            }
        }
    }

    public async void UpdateLobbyGameMode(GameMode gameMode)
    {
        if (joinedLobby != null)
        {
            try
            {
                joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) }
                    }
                });

                OnLobbyGameModeChanged?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            }
            catch (LobbyServiceException e)
            {
                UILogManager.Instance.DisplayLog("Error updating game mode: " + e.Message);
            }
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
            OnLeftLobby?.Invoke(this, EventArgs.Empty);
            UILogManager.Instance.DisplayLog("Left the lobby");

            if (startButton != null)
            {
                startButton.gameObject.SetActive(false);
            }
        }
    }

    public async void UpdatePlayerCharacter(PlayerCharacter playerCharacter)
    {
        if (joinedLobby != null)
        {
            try
            {
                UpdatePlayerOptions options = new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { KEY_PLAYER_CHARACTER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerCharacter.ToString()) }
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;
                joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                UILogManager.Instance.DisplayLog("Player character updated to: " + playerCharacter);
            }
            catch (LobbyServiceException e)
            {
                UILogManager.Instance.DisplayLog("Error updating player character: " + e.Message);
            }
        }
    }
}
