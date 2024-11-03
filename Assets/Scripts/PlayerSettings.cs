using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Collections;


public class PlayerSettings : NetworkBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer; 
    [SerializeField] private TextMeshProUGUI playerName;
    private NetworkVariable<FixedString128Bytes> networkPlayerName = new NetworkVariable<FixedString128Bytes>(
        "Player: 0", NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    public List<Color> colors = new List<Color>();

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();  
    }

    public override void OnNetworkSpawn()
    {
        networkPlayerName.Value = "Player: " + (OwnerClientId + 1);
        playerName.text = networkPlayerName.Value.ToString();
        meshRenderer.material.color = colors[(int)OwnerClientId];
    }
}
