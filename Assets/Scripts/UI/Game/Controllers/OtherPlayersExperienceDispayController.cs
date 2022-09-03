using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        _otherPlayers.OnPlayerJoined += OnNewPlayerJoined;
        _otherPlayers.OnPlayerRemoved += OnPlayerLeft;
    }

    private void OnPlayerLeft(PlayerView obj)
    {
        foreach (var key in _anchoredWidgets.Keys.ToList())
            if (_anchoredWidgets[key] == obj)
            {
                GameObject.Destroy(_anchoredWidgets[key]);
                _anchoredWidgets.Remove(key);
            }
    }

    private void OnNewPlayerJoined(PlayerView player)
    {
        var newWidget = GameObject.Instantiate(Resources.Load<OtherPlayerWidgetView>(prefabPath),_gameCanvas.transform);
        player.StartCoroutine(OnPlayerJoinedCoroutine(player, newWidget));
    }

    private IEnumerator OnPlayerJoinedCoroutine(PlayerView player, OtherPlayerWidgetView widget)
    {
        yield return new WaitUntil(() => player.Data != null);
        widget.Init(_camera, _updateProvider, player, player.Data.Nickname, _levelsConfig.GetLevelByExp(player.Data.Experience), _levelsConfig.GetCurrentLevelNormalizedExp(player.Data.Experience));
        _anchoredWidgets.Add(widget, player);
    }

    private void Update()
    {
        foreach (var widget in _anchoredWidgets.Keys.ToList())
        {
            widget.UpdateHealth(_anchoredWidgets[widget].DynamicData.Health);
            widget.UpdateLevelProgress(_levelsConfig.GetCurrentLevelNormalizedExp(_anchoredWidgets[widget].Data.Experience));
            widget.UpdateLevel(_levelsConfig.GetLevelByExp(_anchoredWidgets[widget].Data.Experience));
        }
    }
}
