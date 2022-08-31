﻿using System;
using System.Collections.Generic;

public class PlayerExperienceUiController : GameUiControllerBase
{
    private Queue<UpdatePlayerUiWidgetSignal> _Queue;
    private PlayerWidgetView _widget;
    private LevelUpAnimationView _levelUpAnim;

    public PlayerExperienceUiController(SignalBus signalBus, GameCanvas gameCanvas) : base(signalBus, gameCanvas)
    {
        _widget = gameCanvas.GetView<GameUiPanel>().GetView<PlayerWidgetView>();
        _levelUpAnim = gameCanvas.GetView<GameUiPanel>().GetView<LevelUpAnimationView>();

        signalBus.Subscribe<QueueUpdatePlayerWidgetSignals>(OnExperienceChanged, this);
        signalBus.Subscribe<UpdatePlayerUiWidgetSignal>(UpdateUi, this);
    }

    private void UpdateUi(UpdatePlayerUiWidgetSignal signal)
    {
        _widget.ExperienceBar.SetValues(signal.LevelChanged ? 0 : _widget.ExperienceBar.Image.fillAmount, signal.NormalizedExp);
        _widget.Level.text = signal.Level.ToString();

        if (signal.LevelChanged && !_levelUpAnim.IsPlaying)
            _levelUpAnim.Animate();

        _widget.ExperienceBar.Play(0, () =>
        {
            if (_Queue != null && _Queue.Count > 0)
            {
                _signalBus.FireSignal(_Queue.Dequeue());
            }
        });
    }

    private void OnExperienceChanged(QueueUpdatePlayerWidgetSignals signal)
    {
        _Queue = signal.Queue;
        _signalBus.FireSignal(_Queue.Dequeue());
    }
}
