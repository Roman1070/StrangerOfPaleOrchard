using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainCameraAnchor : MonoBehaviour
{
    [Inject]
    private PlayerView _player;

    public Camera Camera;

    private void Update()
    {
        transform.position = _player.transform.position;
    }
}
