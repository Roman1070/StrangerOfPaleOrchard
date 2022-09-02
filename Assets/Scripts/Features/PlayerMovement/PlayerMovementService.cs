using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerMovementService : LoadableService
{
    private PlayerView _player;
    private UpdateProvider _updateProvider;
    private Camera _camera;
    private PlayerStatesService _statesService;

    private List<PlayerMovementControllerBase> _controllers;

    public PlayerMovementService(SignalBus signalBus, PlayerView player, UpdateProvider updateProvider, Camera camera) : base(signalBus)
    {
        _player = player;
        _updateProvider = updateProvider;
        _camera = camera;
    }

    public override void OnServicesLoaded(params LoadableService[] services)
    {
        _statesService = (services.ToList().FirstOrDefault(s => s is PlayerStatesService)) as PlayerStatesService;

        _controllers = new List<PlayerMovementControllerBase>()
        {
            new PlayerMovementController(_signalBus,_player,_updateProvider,_camera,_statesService)
        };
    }
}
