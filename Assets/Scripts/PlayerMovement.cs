using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] public float movementSpeed = 7f;
    [SerializeField] private float rotationSpeed = 500f;
    [SerializeField] private float positionRange = 5f;

    private void Start()
    {
        // Any initialization code if needed
    }

    public override void OnNetworkSpawn()
    {
        UpdatePositionServerRpc();

        // Set the player ID to the owner client ID
    }

    private void Update()
    {
        // Ensure only the owning client controls this player
        if (!IsOwner) return;
        // Cache input values
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Create movement direction vector
        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);
        movementDirection.Normalize();

        // Move the player in the desired direction
        transform.Translate(movementDirection * movementSpeed * Time.deltaTime, Space.World);

        // Rotate the player to face the movement direction
        if (movementDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePositionServerRpc()
    {
        transform.position = new Vector3(Random.Range(positionRange, -positionRange), 0.75f,
            Random.Range(positionRange, -positionRange));
        transform.rotation = Quaternion.Euler(0, 180, 0); // Initial rotation facing a specific direction
    }
}