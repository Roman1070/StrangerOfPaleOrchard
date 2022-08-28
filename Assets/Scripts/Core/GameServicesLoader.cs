using DG.Tweening;
using System.Collections.Generic;
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
    protected readonly RenderSpace _playerModel;
    #endregion
    private List<LoadableService> _services;

    [SerializeField]
    private NavMeshSurface _surface;

    private void Start()
    {
        InitServices();
        _surface.transform.position += Vector3.up * 2;
        DOVirtual.DelayedCall(0.1f, () => _surface.transform.position -= Vector3.up * 2);
    }

    private void InitServices()
    {
        _services = new List<LoadableService>()
        {
            new GameUiService(_signalBus, _gameCanvas, _movementConfig),
            //new DevConsoleService(_signalBus, _gameCanvas),
            new InventoryService(_signalBus, _itemsMap),
            new InventoryUiService(_signalBus, _gameCanvas),
            new PlayerGearService(_signalBus, _playerView, _weaponOffsetConfig),
            new PlayerModelUpdateService(_signalBus, _playerModel, _playerView, _weaponOffsetConfig),
            new PlayerDataService(_signalBus, _levelsConfig),
            new PlayerStatesService(_signalBus),
            new VFXService(_signalBus,_playerView)
        };

        foreach (var service in _services)
            service.OnServicesLoaded(_services.ToArray());
    }
}
