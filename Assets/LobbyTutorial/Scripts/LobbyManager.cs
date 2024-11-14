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

    [SerializeField] private Button startGameButton; // Referensi tombol Start Game

    private float heartbeatTimer;
    private float lobbyPollTimer;
    private Lobby joinedLobby;
    private string playerName;

    private void Awake()
    {
        Instance = this;
        UILogManager.Instance.DisplayLog("Lobby Manager Initialized");
        startGameButton.gameObject.SetActive(false); // Tombol hanya aktif untuk host
        startGameButton.onClick.AddListener(StartGame); // Set fungsi saat tombol diklik
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    public async void Authenticate(string playerName)
    {
        this.playerName = playerName;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            UILogManager.Instance.DisplayLog("Signed in successfully");
            RefreshLobbyList();
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        UILogManager.Instance.DisplayLog("Authenticating...");
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
                UILogManager.Instance.DisplayLog("Heartbeat sent to maintain lobby connection");
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
                    UILogManager.Instance.DisplayLog("Kicked from lobby!");
                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                    joinedLobby = null;
                }
                else
                {
                    UILogManager.Instance.DisplayLog("Lobby updated");
                    CheckIfHostAndDisplayStartButton(); // Periksa jika host dan tampilkan tombol start
                }
            }
        }
    }

    private void CheckIfHostAndDisplayStartButton()
    {
        // Tampilkan tombol Start jika pemain adalah host dan ada minimal 2 pemain dalam lobby
        if (IsLobbyHost() && joinedLobby.Players.Count >= 2)
        {
            startGameButton.gameObject.SetActive(true);
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
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
            UILogManager.Instance.DisplayLog("Game mode changed to " + newGameMode);
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
        UILogManager.Instance.DisplayLog("Lobby created: " + joinedLobby.Name);
        Debug.Log("Created Lobby " + joinedLobby.Name);
        CheckIfHostAndDisplayStartButton(); // Pastikan tombol start ditampilkan jika pembuat lobby adalah host
    }

    public async void RefreshLobbyList()
    {
        try
        {
            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
            UILogManager.Instance.DisplayLog("Lobby list refreshed");
        }
        catch (LobbyServiceException e)
        {
            UILogManager.Instance.DisplayLog("Error refreshing lobby list");
            Debug.Log(e);
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        Player player = GetPlayer();
        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions { Player = player });
        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        UILogManager.Instance.DisplayLog("Joined lobby: " + joinedLobby.Name);

        CheckIfHostAndDisplayStartButton(); // Pastikan host menampilkan tombol start
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
                UILogManager.Instance.DisplayLog("Player name updated to " + newName);
            }
            catch (LobbyServiceException e)
            {
                UILogManager.Instance.DisplayLog("Failed to update player name");
                Debug.Log(e);
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
                UILogManager.Instance.DisplayLog("Player kicked from lobby");
            }
            catch (LobbyServiceException e)
            {
                UILogManager.Instance.DisplayLog("Failed to kick player");
                Debug.Log(e);
            }
        }
    }

    public async void UpdateLobbyGameMode(GameMode gameMode)
    {
        if (joinedLobby != null)
        {
            try
            {
                joinedLobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) }
                    }
                });

                OnLobbyGameModeChanged?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                UILogManager.Instance.DisplayLog("Lobby game mode updated to " + gameMode);
            }
            catch (LobbyServiceException e)
            {
                UILogManager.Instance.DisplayLog("Failed to update game mode");
                Debug.Log(e);
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
            UILogManager.Instance.DisplayLog("Left lobby");
        }
    }

    public void StartGame()
    {
        if (IsLobbyHost() && NetworkManager.Singleton.IsServer && joinedLobby.Players.Count >= 2)
        {
            UILogManager.Instance.DisplayLog("Starting game...");
            Debug.Log("Starting game with host permissions...");

            NetworkManager.Singleton.SceneManager.LoadScene("PlayingField", LoadSceneMode.Single);
        }
        else
        {
            UILogManager.Instance.DisplayLog("Cannot start game: Need at least 2 players and must be host.");
            Debug.Log("Failed to start game: Not enough players or not host.");
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
                UILogManager.Instance.DisplayLog("Player character updated to " + playerCharacter);
            }
            catch (LobbyServiceException e)
            {
                UILogManager.Instance.DisplayLog("Failed to update character");
                Debug.Log(e);
            }
        }
    }
}
