using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using Zenject;

public class PlayerInstaller : MonoInstaller
{
    [SerializeField]
    private GameObject _player;

    public override void InstallBindings()
    {
        var player = PhotonNetwork.Instantiate(_player.name, Vector3.zero, Quaternion.identity);
        if (player.GetComponent<PhotonView>().IsMine)
        {
            Container.Bind<PlayerView>().FromInstance(player.GetComponent<PlayerView>()).AsSingle().NonLazy();
            Container.QueueForInject(player);
        }
        

    }
}
