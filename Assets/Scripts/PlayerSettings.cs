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

    public NetworkVariable<FixedString128Bytes> networkPlayerName = new NetworkVariable<FixedString128Bytes>(
        "Player: 0", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public int playerIndex;
    public List<Color> colors = new List<Color>();

    [SerializeField] private int maxHealth;
    public int currentHealth;

    [SerializeField] private NetworkObject networkObject;
    [SerializeField] private Gun gun;
    [SerializeField] private PlayerMovement playerMovement;

    private void Awake()
    {
        GameManager.Instance.players.Add(this);
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        currentHealth = maxHealth;
    }

    public override void OnNetworkSpawn()
    {
        networkPlayerName.Value = "Player: " + (OwnerClientId + 1);
        playerName.text = networkPlayerName.Value.ToString();
        meshRenderer.material.color = colors[(int)OwnerClientId];
        playerIndex = (int)OwnerClientId;

        // Inisialisasi UI health di awal
        if (IsOwner)
        {
            GetComponent<PlayerUIManager>().SetHealth(currentHealth);
        }
    }

    // Method untuk menerima damage
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (!IsServer) return;
        currentHealth -= damage;

        // Update UI health untuk pemain lokal
        if (IsOwner)
        {
            GetComponent<PlayerUIManager>().SetHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            networkObject.Despawn();
        }
    }

    // Method untuk mengembalikan health
    public void RestoreHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // Update UI health untuk pemain lokal
        if (IsOwner)
        {
            GetComponent<PlayerUIManager>().SetHealth(currentHealth);
        }
    }
}
