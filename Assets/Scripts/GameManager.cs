using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

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
        SpawnPlayerServerRpc();
    }
}

[ServerRpc(RequireOwnership = false)]
private void SpawnPlayerServerRpc()
{
    if (!IsServer) return;

    int clientIndex = 0;



        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Vector3 playerPosition = spawnPosition + new Vector3(clientIndex * playerSpawnDistance, 5.5f, 0);
            Transform playerTransform = Instantiate(playerPerf, playerPosition, Quaternion.identity);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);


            playerTransform.name = $"Player {clientIndex}";
             clientIndex++;
        }
    
}

   private void Update()
{
    if (!IsServer) return;

    if (players.Count >= 1)
    {
        lobbyStatus.Value = 1;
    }
    PlayerDeathCheck();
}

    private void PlayerDeathCheck()
    {
        if (lobbyStatus.Value != 1) return;

        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (players[i] == null || players[i].currentHealth <= 0)
            {
                players.RemoveAt(i);
            }
        }

        if (players.Count == 1)
        {
            lobbyStatus.Value = 2;
            ShowWinUIClientRpc(players[0].playerIndex);
        }
    }

    [ClientRpc]
    private void ShowWinUIClientRpc(int winnerIndex)
    {
        winUI.SetActive(true);
        winnerText.text = "P" + (winnerIndex + 1) + " Win";
    }
}
