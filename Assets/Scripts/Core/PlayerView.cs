using Photon.Pun;
using System.Linq;
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
    public OtherPlayersContainer OtherPlayersContainer;
    public Transform Transform => transform;
    public UserDataPack Data { get; private set; }
    public bool IsAlive => _dynamicData.Health > 0;

    public int DamagableId => 0;


    private SignalBus _signalBus;
    private PlayerDynamicData _dynamicData;


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

    [PunRPC]
    private void UpdateExpRemote(int value)
    {
        Data.Experience = value;
    }

    [PunRPC]
    private void SendRemoteCombatTrigger(string id)
    {
        var animator = Model.GetComponent<Animator>();
        animator.SetLayerWeight(4, id == StringConst.ExitCombat ? 0 : 1);
        animator.SetTrigger(id);
    }
    [PunRPC]
    private void SendRemoteMovementSpeed(int speed)
    {
        var animator = Model.GetComponent<Animator>();
        animator.SetFloat("Speed", speed);
    }

    private void Start()
    {
        UserDataPack localData = new UserDataPack() { Id = Photon.ViewID.ToString(), Nickname = $"player {Photon.ViewID}", Experience = 0, Level = 1 };
        Data = localData;

        if (!Photon.IsMine)
        {
            FindObjectsOfType<PlayerView>().First(p => p.Photon.IsMine).OtherPlayersContainer.Insert(this);
            Invoke("GetData", 3);
            return;
        }

        DatabaseAccessService.Instance.Init(localData);

        transform.position = new Vector3(25.3f, 5, 68.3f);
        GetComponent<NavMeshAgent>().enabled = true;
    }

    private async void GetData()
    {
        var datas = DatabaseAccessService.Instance.GetDataFromDB();
        var result = await datas;
        Data = result.First(d => d.Id == Photon.ViewID.ToString());
    }

}
