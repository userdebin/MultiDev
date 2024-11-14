using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ClientConnectTest : MonoBehaviour
{
    [SerializeField] private Button connectAsClientButton;

    private void Start()
    {
        // Pastikan tombol terhubung ke fungsi
        connectAsClientButton.onClick.AddListener(ConnectAsClient);

        // Membuat tombol terlihat jika bukan host
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            connectAsClientButton.gameObject.SetActive(false);
        }
        else
        {
            connectAsClientButton.gameObject.SetActive(true);
        }
    }

    private void ConnectAsClient()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client mencoba terhubung ke server...");
        }
        else
        {
            Debug.Log("NetworkManager tidak tersedia atau sudah terhubung sebagai server/client.");
        }
    }
}

