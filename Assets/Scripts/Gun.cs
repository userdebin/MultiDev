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
    private PlayerUIManager playerUIManager;

    private void Start()
    {
        // Initialize ammo and last fired time
        currentAmmo = maxAmmo;
        lastFiredTime = -cooldownTime;

        // Inisialisasi UI untuk ammo di pemain lokal
        if (IsOwner)
        {
            playerUIManager = GetComponent<PlayerUIManager>();
            if (playerUIManager != null)
            {
                playerUIManager.SetAmmo(currentAmmo); // Inisialisasi tampilan ammo saat mulai
            }
        }
    }

    void Update()
    {
        // Hanya pemain lokal yang bisa menembak
        if (IsOwner && Input.GetKeyDown(KeyCode.Space) && currentAmmo > 0 && Time.time >= lastFiredTime + cooldownTime)
        {
            FireServerRpc();
        }
    }

    // ServerRpc untuk menembak peluru pada server
    [ServerRpc(RequireOwnership = false)]
    private void FireServerRpc(ServerRpcParams rpcParams = default)
    {
        if (currentAmmo <= 0) return;

        // Spawn peluru dan atur arahnya
        var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Atur jenis peluru berdasarkan jenis amunisi yang tersedia
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
            currentAmmo--; // Kurangi amunisi biasa jika tidak ada power atau velocity ammo
        }

        spawnedBullets.Add(bullet);
        bullet.GetComponent<Bullet>().parent = this;
        bullet.GetComponent<NetworkObject>().Spawn();

        // Set kecepatan peluru berdasarkan arah pemain
        Vector3 bulletDirection = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        bullet.GetComponent<Rigidbody>().velocity = bulletDirection * bulletSpeed;

        // Update UI ammo di pemain lokal
        if (IsOwner && playerUIManager != null)
        {
            playerUIManager.SetAmmo(currentAmmo);
        }

        lastFiredTime = Time.time;
    }

    // Method untuk reload amunisi
    public void Reload()
    {
        currentAmmo = maxAmmo;
        Debug.Log("Reloaded! Ammo refilled.");

        // Update UI ammo di pemain lokal
        if (IsOwner && playerUIManager != null)
        {
            playerUIManager.SetAmmo(currentAmmo);
        }
    }
}
