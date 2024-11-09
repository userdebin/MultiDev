using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    // ListPlayer 
    public List<PlayerSettings> players = new List<PlayerSettings>();

    //Sync variable
    public NetworkVariable<int> lobbyStatus = new NetworkVariable<int>(0);
    public GameObject winUI;
    public TextMeshProUGUI winnerText;

    // 0 = Standby
    // 1 = Started
    // 2 = Ended
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

    // Method to handle player death

    // if players count is 1, then the game is started

    private void Update()
    {
        if (!IsServer) return;
        if (players.Count == 2)
        {
            lobbyStatus.Value = 1;
        }
        PlayerDeathClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    public void PlayerDeathClientRpc()
    {
        if (lobbyStatus.Value != 1)
        {
            return;
        }

        // Check player health with for loop
        for (int i = 0; i < players.Count; i++)
        {
            // how do i check if null ? 
            if (players[i].currentHealth <= 0 || players[i] == null)
            {
                players.RemoveAt(i);
            }
        }

        if (players.Count != 1)
        {
            return;
        }

        lobbyStatus.Value = 2;

        // Handle client-side updates
        if (players.Count == 1)
        {
            winUI.SetActive(true);
            int player = players[0].playerIndex + 1;
            winnerText.text = "P" + player + " Win";
        }
    }
}