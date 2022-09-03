using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class OnDBDataLoadedSignal : ISignal
{
    public UserDataPack Data;
    public PlayerView Player;

    public OnDBDataLoadedSignal(UserDataPack data, PlayerView player)
    {
        Data = data;
        Player = player;
    }
}

public class PlayerView : MonoBehaviour, IDamagable, IPunObservable
{
    public Transform Model;
    public Transform GroundChecker;
    public Transform SpineAnchor;
    public Transform HandAnchor;
    public Transform WeaponsHolder;
    public PhotonView Photon;
    public OtherPlayersContainer OtherPlayersContainer;
    public Transform Transform => transform;
    public UserDataPack Data { get; private set; }
    public bool IsAlive => DynamicData.Health > 0;

    public int DamagableId => 0;

    public string Id;
    public SignalBus SignalBus { get; private set; }
    public PlayerDynamicData DynamicData { get; private set; }

    private Animator _animator;
    private Animator Animator
    {
        get
        {
            if (_animator == null)
                _animator = Model.GetComponent<Animator>();
            return _animator;
        }
    }
    public void ThrowDependencies(SignalBus signalBus, PlayerDynamicData dynamicData, string id)
    {
        if (!Photon.IsMine) return;

        SignalBus = signalBus;
        DynamicData = dynamicData;
    }

    public void TakeDamage(int damage)
    {
        if (!Photon.IsMine) return;
        SignalBus.FireSignal(new ChangePlayersHealthSignal(-damage));
    }

    [PunRPC]
    private void OnEquipementChangedRemote(int itemId, int itemLevel)
    {
        StartCoroutine(OnEquipementChangedRemoteCoroutine(itemId, itemLevel));
    }
    private IEnumerator OnEquipementChangedRemoteCoroutine(int itemId, int itemLevel)
    {
        yield return new WaitUntil(() => SignalBus != null);
        SignalBus.FireSignal(new OnEquippedItemChangedSignal(itemId, itemLevel, 0, this));
    }

    [PunRPC]
    private void DrawWeaponRemote(bool draw)
    {
        SignalBus.FireSignal(new DrawWeaponRemoteSignal(draw, this));
    }
    [PunRPC]
    private void UpdateExpRemote(int value)
    {
        Data.Experience = value;
    }
    [PunRPC]
    private void SetDrawingWeaponRemoteTrigger(bool draw)
    {
        Animator.SetTrigger(draw ? "DrawWeapon" : "RemoveWeapon");
    }
    [PunRPC]
    private void SendRemoteCombatTrigger(string id)
    {
        Animator.SetLayerWeight(4, id == StringConst.ExitCombat ? 0 : 1);
        Animator.SetTrigger(id);
    }
    [PunRPC]
    private void SendRemoteMovementSpeed(int speed)
    {
        Animator.SetFloat("Speed", speed);
    }

    private void Start()
    {
        if (!Photon.IsMine)
        {
            var minePlayer = FindObjectsOfType<PlayerView>().First(p => p.Photon.IsMine);
            minePlayer.OtherPlayersContainer.Insert(this);
            SignalBus = minePlayer.SignalBus;
            DynamicData = new PlayerDynamicData() { Health = 100 };
            return;
        }
        Id = PlayerBackendSynchronizer.Instance.Id;
        Data = new UserDataPack() { Id = Id, Nickname = $"player {Id}", Experience = 0, Level = 1 };
        DatabaseAccessService.Instance.SetLocalData(this,Data);
        GetData();
        transform.position = new Vector3(25.3f, 4, 68.3f);
        GetComponent<NavMeshAgent>().enabled = true;
    }


    private void OnDisable()
    {
        var minePlayer = FindObjectsOfType<PlayerView>().First(p => p.Photon.IsMine);
        minePlayer.OtherPlayersContainer.Remove(this);
    }

    private async void GetData()
    {
        var datas = DatabaseAccessService.Instance.GetDataFromDB();
        var result = await datas;
        foreach (var data in result)
        {
            if (data.Id == Data.Id)
            {
                Data = data;
                DatabaseAccessService.Instance.SetLocalData(this, Data);
                SignalBus.FireSignal(new OnDBDataLoadedSignal(Data,this));
                return;
            }
        }
        DatabaseAccessService.Instance.SaveNewData(Data);
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(int.Parse(Id));
        }
        else
        {
            Id = ((int)stream.ReceiveNext()).ToString();
            if (Data == null && !Photon.IsMine)  
            {
                Data = new UserDataPack() { Id = Id, Nickname = $"player {Id}", Experience = 0, Level = 1 };
                DatabaseAccessService.Instance.SetLocalData(this, Data);
                GetData();
            }
        }
    }
}
