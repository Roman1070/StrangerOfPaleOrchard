using System.Collections.Generic;

public class PlayerDataService : LoadableService
{
    private List<PlayerDataControllerBase> _controllers;
    private PlayerLevelsConfig _levelsConfig;
    private PlayerView _player;
    public readonly string Id;
    public PlayerDynamicData DynamicData { get; private set; }

    public PlayerDataService(SignalBus signalBus, PlayerLevelsConfig levelConfig, string id, PlayerView player) : base(signalBus)
    {
        _levelsConfig = levelConfig;
        _player = player;
        Id = id;
        DynamicData = new PlayerDynamicData() { Health = 100 };
        InitControllers();
    }

    public override void OnServicesLoaded(params LoadableService[] services)
    {

    }

    private void InitControllers()
    {
        _controllers = new List<PlayerDataControllerBase>()
        {
            new PlayerExperienceController(_signalBus,_levelsConfig,_player)
        };
    }
}

public class PlayerDynamicData
{
    public float Health;
}
