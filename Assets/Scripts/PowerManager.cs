using Unity.Netcode;
using UnityEngine;

public class PowerUpManager : NetworkBehaviour
{
    public GameObject powerUpPrefab;  // Reference to the power-up prefab
    public Vector3 spawnAreaMin;      // Minimum corner of the spawn area
    public Vector3 spawnAreaMax;      // Maximum corner of the spawn area
    public float spawnInterval = 10f; // Time interval between spawns

    private float spawnTimer;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Start the power-up spawn loop on the server
            spawnTimer = spawnInterval;
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnPowerUpServerRpc();
                spawnTimer = spawnInterval;
            }
        }
    }

    [ServerRpc]
    private void SpawnPowerUpServerRpc()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject powerUp = Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);
        powerUp.GetComponent<NetworkObject>().Spawn();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        float z = Random.Range(spawnAreaMin.z, spawnAreaMax.z);
        return new Vector3(x, y, z);
    }
}