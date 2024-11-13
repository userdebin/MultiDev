using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiDevManager : NetworkBehaviour 

{
    public static MultiDevManager instance; 
    private const int MAX_PLAYER_GAME = 4;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChange;
    private NetworkList<PlayerData> playerDataNetworkList;

    public string hostIpAddress { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlaterDataNetworkList_OnListChanged;
    }

    private void PlaterDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {  
        Debug.Log("Changed");
        OnPlayerDataNetworkListChange?.Invoke(this, EventArgs.Empty);
    }

    public void OnHostStart()
    {
        string localIPAddress = GetHostIP.GetLocalIPAddress();
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = localIPAddress;
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            response.Approved = false;
            response.Reason = "Game Sudah Dimulai";
            return;
        }
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_GAME)
        {
            response.Approved = false;
            response.Reason = "Player Sudah Maksimal";
            return;
        }
        response.Approved = true;
    }

    private void Singleton_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
        });
    }

    public void OnClientStart()
    {
        Debug.Log("Client trying to join the game.");
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
        Debug.Log("OnTryingToJoinGame invoked."); // Tambahkan log ini
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public void SetIPAddress(string ipAddress)
    {
        var unityTransport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        if (unityTransport != null)
        {
            unityTransport.ConnectionData.Address = ipAddress;
            Debug.Log("IP Address set to: " + ipAddress);
        }
        else
        {
            Debug.LogWarning("UnityTransport component not found on NetworkManager.");
        }
    }
}
