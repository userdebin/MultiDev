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
        HealthRestore
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
        Debug.Log("Triggered");
        if (IsServer && other.CompareTag("Player"))
        {
            Debug.Log("Player Triggered");
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
                break;
            case PowerUpType.NormalBullet:
                Debug.Log("Normal Bullet");
                break;
            case PowerUpType.HealthRestore:
                player.RestoreHealthServerRpc(10);
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