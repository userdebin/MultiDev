using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Gun : NetworkBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public int maxAmmo = 10;
    public float cooldownTime = 0.5f;

    public int currentAmmo;
    public int velocityAmmo = 0;
    public int powerAmmo = 0;
    private float lastFiredTime;

    [SerializeField] private List<GameObject> spawnedBullets = new List<GameObject>();

    private void Start()
    {
        // Initialize ammo and last fired time
        currentAmmo = maxAmmo;
        lastFiredTime = -cooldownTime;
    }

    void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.Space) && currentAmmo > 0 && Time.time >= lastFiredTime + cooldownTime)
        {
            FireServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void FireServerRpc(ServerRpcParams rpcParams = default)
    {
        if (currentAmmo <= 0) return;

        var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        if (velocityAmmo > 0)
        {
            bulletSpeed *= 2f;
            velocityAmmo--;
        }
        else if (powerAmmo > 0)
        {
            bullet.GetComponent<Bullet>().SetPowerBullet();
            powerAmmo--;
        }
        else
        {
            bullet.GetComponent<Bullet>().SetNormalBullet();
            bulletSpeed = 10f;
            currentAmmo--;
        }

        spawnedBullets.Add(bullet);
        bullet.GetComponent<Bullet>().parent = this;
        bullet.GetComponent<NetworkObject>().Spawn();
        bullet.GetComponent<Rigidbody>().velocity = new Vector3(transform.forward.x, 0, transform.forward.z).normalized * bulletSpeed;
        lastFiredTime = Time.time;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnBulletsServerRpc()
    {
        if (spawnedBullets.Count > 0)
        {
            GameObject toDestroy = spawnedBullets[0];
            toDestroy.GetComponent<NetworkObject>().Despawn();
            spawnedBullets.RemoveAt(0);
        }
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
        Debug.Log("Reloaded! Ammo refilled.");
    }
}
