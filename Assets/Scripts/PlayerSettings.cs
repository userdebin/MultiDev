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
        "Player: 0", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public List<Color> colors = new List<Color>();

    // Add health variable
    [SerializeField] private NetworkVariable<int> maxHealth = new NetworkVariable<int>(100);

    // [SerializeField] private int currentHealth;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100);

    [SerializeField] private NetworkObject networkObject;

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        currentHealth = maxHealth;
    }

    public override void OnNetworkSpawn()
    {
        networkPlayerName.Value = "Player: " + (OwnerClientId + 1);
        playerName.text = networkPlayerName.Value.ToString();
        meshRenderer.material.color = colors[(int)OwnerClientId];
    }

    // Method to handle taking damage
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (!IsServer) return;
        currentHealth.Value -= damage;
        if (currentHealth.Value <= 0)
        {
            networkObject.Despawn();
        }
    }
    
    // Method to handle health restoration
    [ServerRpc(RequireOwnership = false)]
    public void RestoreHealthServerRpc(int health)
    {
        if (!IsServer) return;
        currentHealth.Value += health;
        if (currentHealth.Value > maxHealth.Value)
        {
            currentHealth.Value = maxHealth.Value;
        }
    }
}