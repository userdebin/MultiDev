using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    [SerializeField] private Transform playerPerf;
    [SerializeField] private float playerSpawnDistance = 3f;
    private Vector3 spawnPosition = new Vector3(0, 0, 0);

    public List<PlayerSettings> players = new List<PlayerSettings>();

    public NetworkVariable<int> lobbyStatus = new NetworkVariable<int>(0);

    public GameObject winUI;
    public TextMeshProUGUI winnerText;

    private bool isClientStarted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            UILogManager.Instance.DisplayLog("GameManager initialized on server.");
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        UILogManager.Instance.DisplayLog($"Client connected with ID: {clientId}");

        if (!PlayerExists(clientId))
        {
            UILogManager.Instance.DisplayLog($"Spawning player for client {clientId}.");
            SpawnPlayerServerRpc(clientId);
        }
        else
        {
            UILogManager.Instance.DisplayLog($"Player for client {clientId} already exists.");
        }
    }

    private bool PlayerExists(ulong clientId)
    {
        foreach (var player in players)
        {
            if (player != null && player.OwnerClientId == clientId)
            {
                return true;
            }
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong clientId)
    {
        if (!IsServer) return;

        if (PlayerExists(clientId))
        {
            UILogManager.Instance.DisplayLog($"Skipping spawn: player for client {clientId} already exists.");
            return;
        }

        Vector3 playerPosition = spawnPosition + new Vector3(players.Count * playerSpawnDistance, 5.5f, 0);
        Transform playerTransform = Instantiate(playerPerf, playerPosition, Quaternion.identity);
        playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        var playerSettings = playerTransform.GetComponent<PlayerSettings>();
        if (playerSettings != null)
        {
            players.Add(playerSettings);
            UILogManager.Instance.DisplayLog($"Player {clientId} spawned and added to players list.");
        }

        playerTransform.name = $"Player {players.Count}";
    }

    private void Update()
    {
        if (!IsServer) return;

        if (players.Count >= 2)
        {
            lobbyStatus.Value = 1;
            UILogManager.Instance.DisplayLog("Game started with sufficient players.");
            PlayerDeathCheck();
        }
    }

    private void PlayerDeathCheck()
    {
        if (lobbyStatus.Value != 1) return;

        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (players[i] == null || players[i].currentHealth <= 0)
            {
                UILogManager.Instance.DisplayLog($"Removing player {players[i]?.playerIndex} due to death or disconnection.");
                players.RemoveAt(i);
            }
        }

        if (players.Count == 1 && lobbyStatus.Value == 1)
        {
            lobbyStatus.Value = 2; // Mark game as finished
            ShowWinUIClientRpc(players[0].playerIndex);
            UILogManager.Instance.DisplayLog($"Player {players[0].playerIndex} wins.");
        }
    }

    [ClientRpc]
    private void ShowWinUIClientRpc(int winnerIndex)
    {
        if (winUI != null && winnerText != null)
        {
            winUI.SetActive(true);
            winnerText.text = "P" + (winnerIndex + 1) + " Wins";

            UILogManager.Instance.DisplayLog($"Displaying win UI for Player {winnerIndex + 1}.");
            Invoke(nameof(ReturnToMainMenu), 10f);
        }
        else
        {
            UILogManager.Instance.DisplayLog("Error: Win UI or winner text is not assigned.");
        }
    }

    private void ReturnToMainMenu()
    {
        UILogManager.Instance.DisplayLog("Returning to Main Menu after win announcement.");

        if (IsServer)
        {
            NetworkManager.Singleton.Shutdown();
        }
        SceneManager.LoadScene("MainMenu");
    }
}
