using System.Collections.Generic;
using UnityEngine;

public class OtherPlayersExperienceDispayController : GameUiControllerBase
{
    private OtherPlayersContainer _otherPlayers;
    private Dictionary<OtherPlayerWidgetView, PlayerView> _anchoredWidgets = new Dictionary<OtherPlayerWidgetView, PlayerView>();
    private UpdateProvider _updateProvider;
    private Camera _camera;
    private PlayerLevelsConfig _levelsConfig;
    private readonly string prefabPath = "Prefabs/UI/OtherPlayerWidget";

    public OtherPlayersExperienceDispayController(SignalBus signalBus, GameCanvas gameCanvas, OtherPlayersContainer otherPlayers, UpdateProvider updateProvider, Camera camera, PlayerLevelsConfig levelsConfig) : base(signalBus, gameCanvas)
    {
        _otherPlayers = otherPlayers;
        _camera = camera;
        _updateProvider = updateProvider;
        _levelsConfig = levelsConfig;
        _updateProvider.Updates.Add(Update);
        _otherPlayers.OnPlayerInserted += OnNewPlayerJoined;
    }

    private void OnNewPlayerJoined(PlayerView player)
    {
        var newWidget = GameObject.Instantiate(Resources.Load<OtherPlayerWidgetView>(prefabPath),_gameCanvas.transform);
        newWidget.Init(_camera,_updateProvider,player,player.Data.Nickname,player.Data.Level,player.Data.Experience/200f);
        _anchoredWidgets.Add(newWidget,player);
    }

    private void Update()
    {
        foreach (var kvp in _anchoredWidgets)
        {
            kvp.Key.UpdateHealth(kvp.Value.DynamicData.Health);
            kvp.Key.UpdateLevelProgress(_levelsConfig.GetCurrentLevelNormalizedExp(kvp.Value.Data.Experience));
            kvp.Key.UpdateLevel(_levelsConfig.GetLevelByExp(kvp.Value.Data.Experience));
        }
    }
}
