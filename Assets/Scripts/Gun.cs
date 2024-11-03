using UnityEngine;
using Unity.Netcode;

public class Gun : NetworkBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public int maxAmmo = 10;
    public float cooldownTime = 0.5f;

    private int currentAmmo;
    private float lastFiredTime;

    private void Start()
    {
        // Initialize ammo and last fired time
        currentAmmo = maxAmmo;
        lastFiredTime = -cooldownTime;
    }

    void Update()
    {
        // Check if the player is the owner to ensure only they can trigger firing
        if (IsOwner && Input.GetKeyDown(KeyCode.Space) && currentAmmo > 0 && Time.time >= lastFiredTime + cooldownTime)
        {
            // Request the server to spawn the bullet
            FireServerRpc();
        }
    }

    // ServerRpc to handle bullet spawning on the server
    [ServerRpc]
    private void FireServerRpc(ServerRpcParams rpcParams = default)
    {
        // Spawn bullet and set direction
        var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Attach the NetworkObject component for networked spawning
        bullet.GetComponent<NetworkObject>().Spawn();

        // Set bullet's velocity based on player's forward direction
        Vector3 bulletDirection = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        bullet.GetComponent<Rigidbody>().velocity = bulletDirection * bulletSpeed;

        // Update cooldown and ammo for the firing player
        lastFiredTime = Time.time;
        currentAmmo--;
    }

    // Optional: method to reload ammo
    public void Reload()
    {
        currentAmmo = maxAmmo;
        Debug.Log("Reloaded! Ammo refilled.");
    }
}
