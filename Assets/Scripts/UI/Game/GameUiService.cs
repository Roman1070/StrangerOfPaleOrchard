using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class GameUiService : LoadableService
{
    private GameCanvas _gameCanvas;
    private PlayerMovementConfig _movementConfig;
    private PlayerStatesService _statesService;
    private InventoryService _inventory;
    private OtherPlayersContainer _otherPlayers;
    private UpdateProvider _updateProvider;
    private Camera _camera;
    private PlayerLevelsConfig _levelsConfig;
    private NPCContainer _npcContainer;
    private List<GameUiControllerBase> _controllers;

    public GameUiService(SignalBus signalBus, GameCanvas canvas, PlayerMovementConfig config, OtherPlayersContainer otherPlayers,
        UpdateProvider updateProvider, Camera camera, PlayerLevelsConfig levelsConfig, NPCContainer npcContainer) : base(signalBus)
    {
        _gameCanvas = canvas;
        _movementConfig = config;
        _otherPlayers = otherPlayers;
        _updateProvider = updateProvider;
        _camera = camera;
        _levelsConfig = levelsConfig;
        _npcContainer = npcContainer;
    }

    public override void OnServicesLoaded(params LoadableService[] services)
    {
        _statesService = services.First(s => s.GetType() == typeof(PlayerStatesService)) as PlayerStatesService;
        _inventory = services.First(s => s.GetType() == typeof(InventoryService)) as InventoryService;
        InitControllers();
    }

    public void InitControllers()
    {
        _controllers = new List<GameUiControllerBase>()
        {
            new CollectedItemWidgetsController(_signalBus,_gameCanvas, _inventory),
            new InteractButtonController(_signalBus, _gameCanvas,_statesService,_camera),
            new PlayerExperienceUiController(_signalBus,_gameCanvas),
            new UiPanelsController(_signalBus, _gameCanvas),
            new OtherPlayersExperienceDispayController(_signalBus,_gameCanvas,_otherPlayers,_updateProvider,_camera,_levelsConfig),
            new EnemyHealthUIController(_signalBus,_gameCanvas,_npcContainer)
        };
    }
}
