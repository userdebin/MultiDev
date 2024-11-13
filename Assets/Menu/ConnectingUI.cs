using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    private void Start()
    {
        MultiDevManager.instance.OnTryingToJoinGame += MultiDevManager_OnTryingToJoinGame;
        MultiDevManager.instance.OnFailedToJoinGame += MultiDevManager_OnFailedToJoinGame;
        Hide();
    }

    private void MultiDevManager_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Hide();
    }

    private void MultiDevManager_OnTryingToJoinGame(object sender, EventArgs e)
    {
        Show();
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
        if (MultiDevManager.instance != null)
        {
            MultiDevManager.instance.OnTryingToJoinGame -= MultiDevManager_OnTryingToJoinGame;
            MultiDevManager.instance.OnFailedToJoinGame -= MultiDevManager_OnFailedToJoinGame;
        }
    }

}
