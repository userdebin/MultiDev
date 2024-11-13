using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionResponseUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI messageTExt;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }
    private void Start()
    {
        MultiDevManager.instance.OnFailedToJoinGame += MultiDevManager_OnFailedToJoinGame;
        Hide();
    }

    private void MultiDevManager_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Show();
        messageTExt.text = string.IsNullOrEmpty(NetworkManager.Singleton.DisconnectReason)
                      ? "Failed to join game for unknown reason"
                      : NetworkManager.Singleton.DisconnectReason;

    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        MultiDevManager.instance.OnFailedToJoinGame -= MultiDevManager_OnFailedToJoinGame;
    }

}
