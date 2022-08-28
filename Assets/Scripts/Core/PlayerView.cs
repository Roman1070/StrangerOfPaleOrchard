using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class PlayerView : MonoBehaviour, IDamagable
{
    public CharacterController Controller;
    public Transform Model;
    public Transform GroundChecker;
    public Transform SpineAnchor;
    public Transform HandAnchor;
    public Transform WeaponsHolder;
    public CharacterControllerMoveAnimation MoveAnim;
    public PhotonView Photon;

    private SignalBus _signalBus;
    private PlayerDynamicData _dynamicData;
    public Transform Transform => transform;

    public bool IsAlive => _dynamicData.Health > 0;

    public void ThrowDependencies(SignalBus signalBus, PlayerDynamicData dynamicData)
    {
        if (!Photon.IsMine) return;
        _signalBus = signalBus;
        _dynamicData = dynamicData;
    }

    public void TakeDamage(int damage)
    {
        if (!Photon.IsMine) return;
        _signalBus.FireSignal(new ChangePlayersHealthSignal(-damage));
    }

    private void Start()
    {
        if (!Photon.IsMine) return;
        transform.position = new Vector3(25.3f, 5, 68.3f);
        GetComponent<NavMeshAgent>().enabled = true;
    }
}