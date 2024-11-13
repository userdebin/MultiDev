using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : NetworkBehaviour
{
    [SerializeField] private Button createGame;
    [SerializeField] private Button joinGame;

    [SerializeField] private TMP_InputField ipInputField;


    private void Awake()
    {
        createGame.onClick.AddListener(() =>
        {
            MultiDevManager.instance.OnHostStart();
        });

        joinGame.onClick.AddListener(() =>
        {
            joinGame.onClick.AddListener(() =>
        {
            string ipAddress = ipInputField.text;
            Debug.Log("IP Address entered: " + ipAddress);  

          
            MultiDevManager.instance.SetIPAddress(ipAddress);
            MultiDevManager.instance.OnClientStart();
        });
        });
    }

}
