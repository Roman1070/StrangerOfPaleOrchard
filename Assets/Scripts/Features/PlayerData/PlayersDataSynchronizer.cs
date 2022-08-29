using Photon.Pun;
using System.Linq;
using UnityEngine;

public class PlayersDataSynchronizer : PhotonView
{
    [SerializeField]
    private UserDataPack _localData;

    private SignalBus _signalBus;
    private DatabaseAccessService _dbService;

    private void Start()
    {
        _dbService = FindObjectOfType<DatabaseAccessService>();
        _signalBus = FindObjectOfType<SignalBus>();

        _localData.Id = Random.Range(0,99999).ToString();
        _localData = new UserDataPack() { Experience = 0, Id = _localData.Id, Level = 1, Nickname = "во дворах вдрабадан" };
        _dbService.SaveNewData(_localData);

        Subscribe();
    }

    private void Subscribe()
    {
        _signalBus.Subscribe<OnExperienceChangedSignal>(OnExpChanged, this);
    }

    private void OnExpChanged(OnExperienceChangedSignal obj)
    {
        _localData.Experience += obj.Value;
        SendData();
    }

    private void SendData()
    {
        _dbService.UpdateExperience(_localData);
        Invoke("UpdateLocalData", 3);
    }
    private async void UpdateLocalData()
    {
        var task = _dbService.GetDataFromDB();
        var data = await task;
        _localData.Id = data.First(d => d.Id == _localData.Id).Id;
        _localData.Nickname = data.First(d => d.Id == _localData.Id.ToString()).Nickname;
        _localData.Level = data.First(d => d.Id == _localData.Id.ToString()).Level;
        _localData.Experience = data.First(d => d.Id == _localData.Id.ToString()).Experience;

        RPC("OnLocalDataUpdated", RpcTarget.All, _localData.Id, _localData.Level, _localData.Experience);
    }

    [PunRPC]
    public void OnLocalDataUpdated(string id, int level, int exp)
    {
        _localData.Id = id;
        _localData.Level = level;
        _localData.Experience = exp;
    }
}
