using System;
using System.Linq;
using UnityEngine;
public class InteractButtonController : GameUiControllerBase
{
    private InteractButton _button;
    private Camera _camera;
    private PlayerStatesService _states;

    public InteractButtonController(SignalBus signalBus, GameCanvas gameCanvas, PlayerStatesService states) : base(signalBus, gameCanvas)
    {
        _button = gameCanvas.GetView<GameUiPanel>().GetView<InteractButton>();
        _states = states;
    }
}
