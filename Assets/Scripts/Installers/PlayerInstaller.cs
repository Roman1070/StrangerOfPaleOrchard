using Photon.Pun;
using UnityEngine;
using Zenject;

public class PlayerInstaller : MonoInstaller
{
    [SerializeField]
    private GameObject _player;

    public override void InstallBindings()
    {
        var player = PhotonNetwork.Instantiate("Player",Vector3.zero,Quaternion.identity);
        Container.Bind<PlayerView>().FromInstance(player.GetComponent<PlayerView>()).AsSingle().NonLazy();
        Container.QueueForInject(player);
    }
}
