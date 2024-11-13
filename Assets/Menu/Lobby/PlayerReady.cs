using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerReady : MonoBehaviour
{
   [SerializeField] private int playerIndex;
   [SerializeField] private GameObject readyGameObject;

    private void Start()
    {
        // Periksa apakah MultiDevManager instance ada
        if (MultiDevManager.instance == null) return;

        // Menambahkan event listener untuk perubahan daftar pemain
        MultiDevManager.instance.OnPlayerDataNetworkListChange += MultiDevManager_OnPlayerDataNetworkListChange;
        LobManager.Instance.OnReadyChanged += Ready_OnReadyChanged;
        
        // Perbarui status pemain
        UpdatePlayer();
    }

    private void Ready_OnReadyChanged(object sender, EventArgs e)
    {
        // Update player ketika ada perubahan readiness
        UpdatePlayer();
    }

    private void MultiDevManager_OnPlayerDataNetworkListChange(object sender, EventArgs e)
    {
        // Update player saat daftar pemain berubah
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        // Cek apakah player dengan index tertentu sudah terkoneksi
        if (MultiDevManager.instance != null && MultiDevManager.instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();

            // Ambil data pemain berdasarkan index
            PlayerData playerData = MultiDevManager.instance.GetPlayerDataFromPlayerIndex(playerIndex);

            // Periksa apakah pemain siap
            readyGameObject.SetActive(LobManager.Instance.IsPlayerReaady(playerData.clientId));
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
