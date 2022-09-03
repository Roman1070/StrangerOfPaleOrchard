using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LobbyPhotonBehaviour : MonoBehaviourPunCallbacks
{
    private bool _isConnected;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CreateRoom()
    {
        if (!_isConnected) return;

        if (PhotonNetwork.CountOfRooms == 0)
        {
            PhotonNetwork.CreateRoom("Room", new Photon.Realtime.RoomOptions { MaxPlayers = 60 });
        }
        else
        {
            PhotonNetwork.JoinRoom("Room");
        }
    }


    public override void OnConnectedToMaster()
    {
        _isConnected = true;
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }
}
