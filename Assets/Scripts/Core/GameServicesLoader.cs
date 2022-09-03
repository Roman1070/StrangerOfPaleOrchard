using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class GameServicesLoader : MonoBehaviour
{
    #region DEPENDENCIES
    [Inject]
    protected readonly SignalBus _signalBus;
    [Inject]
    protected readonly UpdateProvider _updateProvider;
    [Inject]
    protected readonly PlayerView _playerView;
    [Inject]
    protected readonly PlayerMovementConfig _movementConfig;
    [Inject]
    protected readonly PlayerCombatConfig _combatConfig;
    [Inject]
    protected readonly GameCanvas _gameCanvas;
    [Inject]
    protected readonly ItemsMap _itemsMap;
    [Inject]
    protected readonly PlayerLevelsConfig _levelsConfig;
    [Inject]
    protected readonly CameraMovementConfig _cameraConfig;
    [Inject]
    protected readonly InputConfig _inputConfig;
    [Inject]
    protected readonly EquipedWeaponOffsetConfig _weaponOffsetConfig;
    [Inject]
    protected readonly RenderSpace _renderSpace;
    [Inject]
    protected readonly CoroutineExecutor _coroutineExecutor;
    [Inject]
    protected readonly MainCameraAnchor _mainCameraAnchor;
    #endregion
    private List<LoadableService> _services;

    private void Start()
    {
        InitServices();
    }

    private void InitServices()
    {
        _services = new List<LoadableService>()
        {
            new GameUiService(_signalBus, _gameCanvas, _movementConfig,_playerView.OtherPlayersContainer,_updateProvider,_mainCameraAnchor.Camera,_levelsConfig),
            //new DevConsoleService(_signalBus, _gameCanvas),
            new InventoryService(_signalBus, _itemsMap),
            new PlayerCombatService(_signalBus,_updateProvider,_mainCameraAnchor.Camera,_playerView,_combatConfig,_coroutineExecutor),
            new InventoryUiService(_signalBus, _gameCanvas),
            new PlayerGearService(_signalBus, _playerView, _weaponOffsetConfig),
            new PlayerModelUpdateService(_signalBus, _renderSpace, _playerView, _weaponOffsetConfig,_playerView.OtherPlayersContainer),
            new PlayerDataService(_signalBus, _levelsConfig,PlayerPrefs.GetString("ID")),
            new PlayerStatesService(_signalBus),
            new VFXService(_signalBus,_playerView),
            new ItemCollectService(_signalBus,_updateProvider,_playerView),
            new PlayerMovementService(_signalBus,_playerView,_updateProvider,_mainCameraAnchor.Camera)
        };

        _playerView.ThrowDependencies(_signalBus, (_services.First(s => s is PlayerDataService) as PlayerDataService).DynamicData, (_services.First(s => s is PlayerDataService) as PlayerDataService).Id);

        foreach (var service in _services)
            service.OnServicesLoaded(_services.ToArray());

    }
}
