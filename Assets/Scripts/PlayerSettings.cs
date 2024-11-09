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

    // Add health variable
    // [SerializeField] private NetworkVariable<int> maxHealth = new NetworkVariable<int>(3);

    // [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;

    // public NetworkVariable<int> currentHealth = new NetworkVariable<int>(3);
    public int currentHealth;

    [SerializeField] private NetworkObject networkObject;
    [SerializeField] private Gun gun;
    [SerializeField] private PlayerMovement playerMovement;

    private void Awake()
    {
        //Register to gamemanager
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
    }

    // Method to handle taking damage
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (!IsServer) return;
        currentHealth -= damage;
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

    //apply power up to client side
    // 
    [ClientRpc]
    public void ApplyPowerUpClientRpc(PowerUp.PowerUpType powerUpType)
    {
        switch (powerUpType)
        {
            case PowerUp.PowerUpType.SpeedBoost:
                Debug.Log("Speed Boost");
                playerMovement.movementSpeed *= 1.5f;
                StartCoroutine(ResetSpeed());
                break;
            case PowerUp.PowerUpType.NormalBullet:
                gun.currentAmmo += 5;
                Debug.Log("Normal Bullet");
                break;
            case PowerUp.PowerUpType.HealthRestore:
                currentHealth += 1;
                if (currentHealth > maxHealth)
                {
                    currentHealth = maxHealth;
                }

                Debug.Log("Health Restore");
                break;
            case PowerUp.PowerUpType.PowerBullet:
                gun.powerAmmo += 1;
                Debug.Log("Power Bullet");
                break;
            case PowerUp.PowerUpType.VelocityBullet:
                gun.velocityAmmo += 1;
                Debug.Log("Velocity Bullet");
                break;
        }
    }
}