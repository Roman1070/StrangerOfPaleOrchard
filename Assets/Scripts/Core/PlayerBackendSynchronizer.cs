using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBackendSynchronizer : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private InputField _inputField;

    public string Id { get; private set; }

    private static PlayerBackendSynchronizer _instance;
    public static PlayerBackendSynchronizer Instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType <PlayerBackendSynchronizer>();
            return _instance;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        _inputField.onValueChanged.AddListener(OnIdInput);
    }

    private void OnIdInput(string text)
    {
        Id = text;
    }
}