using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExperienceController : PlayerDataControllerBase, IOnEventCallback
{
    public int Experience { get; private set; }
    public int Level { get; private set; }
    private int CurrentLevelExpirience => Experience - CurrentLevelRequiredExp;
    private int CurrentLevelRequiredExp => _levelsConfig.ExpOnLevel[Level - 1];
    private int NextLevelRequiredExp => _levelsConfig.ExpOnLevel[Level];

    private float NormalizedExp => Mathf.Clamp((float)CurrentLevelExpirience / (NextLevelRequiredExp - CurrentLevelRequiredExp),0,1);

    private PlayerLevelsConfig _levelsConfig;
    private PlayerView _player;

    public PlayerExperienceController(SignalBus signalBus, PlayerLevelsConfig levelConfig, PlayerView player) : base(signalBus)
    {
        _levelsConfig = levelConfig;
        _player = player;
        if (Level == 0)
        {
            Experience = 0;
            Level = 1;
        }

        signalBus.Subscribe<OnExperienceChangedSignal>(OnExpChanged, this);
        signalBus.Subscribe<OnDBDataLoadedSignal>(OnDataLoaded, this);
        DOVirtual.DelayedCall(0.1f, () => { signalBus.FireSignal(new UpdatePlayerUiWidgetSignal(NormalizedExp, Level, false)); });
    }

    private void OnDataLoaded(OnDBDataLoadedSignal signal)
    {
        if (!signal.Player.Photon.IsMine) return;
        Experience = signal.Data.Experience;

        UpdateLevel();
    }

    private void OnExpChanged(OnExperienceChangedSignal signal)
    {
        if (signal.Player.Photon.ViewID != _player.Photon.ViewID) return;
        Experience += signal.Value;

        UpdateLevel();
    }

    private void UpdateLevel()
    {
        var queue = new Queue<UpdatePlayerUiWidgetSignal>();

        queue.Enqueue(new UpdatePlayerUiWidgetSignal(NormalizedExp, Level,false));

        for (int i = Level; i < _levelsConfig.ExpOnLevel.Length;i++)
        {
            if (Experience >= _levelsConfig.ExpOnLevel[i])
            {
                Level = i + 1;
                _signalBus.FireSignal(new OnPlayerLevelIncreasedSignal());
                queue.Enqueue(new UpdatePlayerUiWidgetSignal(NormalizedExp, Level,true));

            }
        }
        _signalBus.FireSignal(new QueueUpdatePlayerWidgetSignals(queue));
    }

    public void OnEvent(EventData photonEvent)
    {

    }
}

