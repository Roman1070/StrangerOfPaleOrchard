using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBackendSynchronizer : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private InputField _inputField;

    private string _id;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        _inputField.onValueChanged.AddListener(OnIdInput);
    }

    private void OnIdInput(string text)
    {
        _id = text;
        PlayerPrefs.SetString("ID", _id);
    }
}