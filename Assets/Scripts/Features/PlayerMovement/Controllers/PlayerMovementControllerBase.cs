using UnityEngine;
using UnityEngine.AI;

public class PlayerMovementControllerBase
{
    protected SignalBus _signalBus;
    protected PlayerView _player;
    protected UpdateProvider _updateProvider;
    protected PlayerStatesService _statesService;
    public PlayerMovementControllerBase(SignalBus signalBus, PlayerView player, UpdateProvider updateProvider, PlayerStatesService statesService)
    {
        _signalBus = signalBus;
        _player = player;
        _updateProvider = updateProvider;
        _statesService = statesService;
    }

}

