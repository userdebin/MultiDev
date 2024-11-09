using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PowerUpManager : NetworkBehaviour
{
    //powerup prefabs
    // public List<GameObject> powerUpPrefabs = new List<GameObject>();

    public GameObject[] rarePowerUpPrefabs; // Array for rare PowerUp prefabs
    public GameObject[] mediumPowerUpPrefabs; // Array for medium PowerUp prefab
    public GameObject[] easyPowerUpPrefabs; // Array for easy PowerUp prefabs

    public Vector3 spawnAreaMin; // Minimum corner of the spawn area
    public Vector3 spawnAreaMax; // Maximum corner of the spawn area
    public float spawnInterval = 1f; // Time interval between spawns
    public bool isPowerUpSpawned; // Flag to check if a power-up is spawned
    
    // Define spawn chances for each rarity type in percentage (0-100)
    [Range(0, 100)]
    public float rareSpawnChance = 40f;    // Chance for Rare power-ups to spawn (40%)
    [Range(0, 100)]
    public float mediumSpawnChance = 30f;  // Chance for Medium power-ups to spawn (30%)
    [Range(0, 100)]
    public float easySpawnChance = 30f;    // Chance for Easy power-ups to spawn (30%)

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

    [ServerRpc (RequireOwnership = false)]
    private void SpawnPowerUpServerRpc()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject powerUpPrefab = GetPowerUpBasedOnChance();
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

    private GameObject GetPowerUpBasedOnChance()
    {
        // Generate a random number between 0 and 100
        float randomChance = Random.Range(0f, 100f);

        // Check the chance and select the corresponding power-up
        if (randomChance < rareSpawnChance) // Rare PowerUp
        {
            return rarePowerUpPrefabs[Random.Range(0, rarePowerUpPrefabs.Length)];
        }
        else if (randomChance < rareSpawnChance + mediumSpawnChance) // Medium PowerUp
        {
            return mediumPowerUpPrefabs[0]; // Only one medium prefab
        }
        else if (randomChance < rareSpawnChance + mediumSpawnChance + easySpawnChance) // Easy PowerUp
        {
            return easyPowerUpPrefabs[Random.Range(0, easyPowerUpPrefabs.Length)];
        }
        else
        {
            // If no power-up is selected (this shouldn't happen if the chances sum to 100)
            return null;
        }
    }
}