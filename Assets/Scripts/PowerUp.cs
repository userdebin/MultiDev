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
                // ApplyPowerUpServerRpc(player.NetworkObjectId);
                player.ApplyPowerUpClientRpc(powerUpType);
                DespawnObjectServerRpc();
            }
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