using System;
using System.Linq;
using UnityEngine;
public class InteractButtonController : GameUiControllerBase
{
    private InteractButton _button;
    private Camera _camera;
    private PlayerStatesService _states;
    private InteractableObject _closestObject;

    private bool InteractingAvailable => new bool[]
    {
        _states.States[PlayerState.Interacting]
    }.Sum().Inverse();

    public InteractButtonController(SignalBus signalBus, GameCanvas gameCanvas, PlayerStatesService states, Camera camera) : base(signalBus, gameCanvas)
    {
        _button = gameCanvas.GetView<GameUiPanel>().GetView<InteractButton>();
        _states = states;
        _camera = camera;
        _button.Button.onClick.RemoveAllListeners();
        _button.Button.onClick.AddListener(() =>
        {
            if (InteractingAvailable)
                signalBus.FireSignal(new OnInteractingItemStartedSignal(_closestObject));
        });
        signalBus.Subscribe<UpdateInteractableItemSignal>(UpdateNearbyItem, this);
        signalBus.Subscribe<OnInteractingItemStartedSignal>(OnInteractingStarted, this);
    }

    private void OnInteractingStarted(OnInteractingItemStartedSignal signal)
    {
        _button.SetActive(false);
    }

    private void UpdateNearbyItem(UpdateInteractableItemSignal signal)
    {
        _closestObject = signal.Object;
        if (_closestObject == null || _states.States[PlayerState.Interacting]) _button.SetActive(false);
        else
        {
            _button.SetActive(true);
            _button.SetData(_closestObject.Action);
            _button.transform.position = _camera.WorldToScreenPoint(_closestObject.transform.position);
        }
    }


}
