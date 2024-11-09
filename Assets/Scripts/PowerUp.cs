using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PowerUp : NetworkBehaviour
{
    public enum PowerUpType
    {
        SpeedBoost,
        NormalBullet,
        HealthRestore,
        PowerBullet,
        VelocityBullet
    }

    public PowerUpType powerUpType;
    public NetworkObject networkObject;
    public NetworkVariable<float> powerDuration = new NetworkVariable<float>(10f);

    private void Start()
    {
        if (IsServer)
        {
            StartCoroutine(AutoDespawnAfterDuration());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.CompareTag("Player"))
        {
            PlayerSettings player = other.GetComponent<PlayerSettings>();
            if (player != null)
            {
                ApplyPowerUp(player);
                DespawnObjectServerRpc();
            }
        }
    }

    private void ApplyPowerUp(PlayerSettings player)
    {
        switch (powerUpType)
        {
            case PowerUpType.SpeedBoost:
                Debug.Log("Speed Boost");
                player.IncreaseSpeedServerRpc();
                break;
            case PowerUpType.NormalBullet:
                player.IncreaseBulletServerRpc(5);
                Debug.Log("Normal Bullet");
                break;
            case PowerUpType.HealthRestore:
                player.RestoreHealthServerRpc(1);
                Debug.Log("Health Restore");
                break;
            case PowerUpType.PowerBullet:
                player.IncreasePowerBulletServerRpc(1);
                Debug.Log("Power Bullet");
                break;
            case PowerUpType.VelocityBullet:
                player.IncreaseVelocityBulletServerRpc(1);
                Debug.Log("Velocity Bullet");
                break;
        }
    }

    [ServerRpc]
    private void DespawnObjectServerRpc()
    {
        networkObject.DontDestroyWithOwner = true;
        networkObject.Despawn();
    }


    private IEnumerator AutoDespawnAfterDuration()
    {
        yield return new WaitForSeconds(powerDuration.Value);
        DespawnObjectServerRpc();
    }
}