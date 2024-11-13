using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button readyButton;
    [SerializeField] private Button mainmenuButton;
    [SerializeField] private TextMeshProUGUI hostIpText;

    private void Awake()
    {
        readyButton.onClick.AddListener(() =>
        {
            LobManager.Instance.SetPlayerReady();
        });
        
        mainmenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        });
        
        if (MultiDevManager.instance != null)
        {
            string localIPAddress = GetHostIP.GetLocalIPAddress();
            hostIpText.text = $"Host IP: {localIPAddress}";
        }
    }
}