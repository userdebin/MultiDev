// Bullet.cs

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float life = 3f;
    public int bulletDamage = 10;
    public NetworkObject networkObject;
    public Gun parent;

    private void Awake()
    {
        // Destroy(gameObject, life);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            if (parent == null)
            {
                return;
            }

            parent.DespawnBulletsServerRpc();
        }
        else if (other.gameObject.CompareTag("Player"))
        {
            if (parent == null)
            {
                return;
            }

            parent.DespawnBulletsServerRpc();
            other.gameObject.GetComponent<PlayerSettings>().TakeDamageServerRpc(bulletDamage);
        }
    }

    private void DespawnObject()
    {
        networkObject.DontDestroyWithOwner = true;
        networkObject.Despawn();
    }
}