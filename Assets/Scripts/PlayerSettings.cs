using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Collections;

public class PlayerSettings : NetworkBehaviour, INetworkSerializable
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

    private PlayerUIManager playerUIManager;

    private void Awake()
    {
        GameManager.Instance.players.Add(this);
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        currentHealth = maxHealth;
        playerUIManager = GetComponent<PlayerUIManager>();
    }

    public override void OnNetworkSpawn()
    {
        networkPlayerName.Value = "Player: " + (OwnerClientId + 1);
        playerName.text = networkPlayerName.Value.ToString();
        meshRenderer.material.color = colors[(int)OwnerClientId];
        playerIndex = (int)OwnerClientId;

        if (IsOwner && playerUIManager != null)
        {
            playerUIManager.UpdateHealthUI();
            playerUIManager.UpdateAmmoUI();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (!IsServer) return;
        currentHealth -= damage;

        if (IsOwner && playerUIManager != null)
        {
            playerUIManager.UpdateHealthUI();
        }

        if (currentHealth <= 0)
        {
            networkObject.Despawn();
        }
    }

    private IEnumerator ResetSpeed()
    {
        yield return new WaitForSeconds(3f);
        playerMovement.movementSpeed = 7f;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref maxHealth);
        serializer.SerializeValue(ref currentHealth);
    }

    // Ubah parameter ke tipe `int`
    [ClientRpc]
    public void ApplyPowerUpClientRpc(int powerUpTypeValue)
    {
        // Konversi kembali ke `PowerUpType`
        PowerUp.PowerUpType powerUpType = (PowerUp.PowerUpType)powerUpTypeValue;
        string powerUpName = "";

        switch (powerUpType)
        {
            case PowerUp.PowerUpType.SpeedBoost:
                powerUpName = "Speed Boost";
                playerMovement.movementSpeed *= 1.5f;
                StartCoroutine(ResetSpeed());
                break;
            case PowerUp.PowerUpType.NormalBullet:
                powerUpName = "Normal Bullet";
                gun.currentAmmo += 5;
                break;
            case PowerUp.PowerUpType.HealthRestore:
                powerUpName = "Health Restore";
                currentHealth += 1;
                if (currentHealth > maxHealth) currentHealth = maxHealth;
                break;
            case PowerUp.PowerUpType.PowerBullet:
                powerUpName = "Power Bullet";
                gun.powerAmmo += 1;
                break;
            case PowerUp.PowerUpType.VelocityBullet:
                powerUpName = "Velocity Bullet";
                gun.velocityAmmo += 1;
                break;
        }

        if (IsOwner && playerUIManager != null)
        {
            playerUIManager.DisplayPowerUpMessage(powerUpName);
            playerUIManager.UpdateHealthUI();
            playerUIManager.UpdateAmmoUI();
        }
    }
}
