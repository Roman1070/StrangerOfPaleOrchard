using System.Collections.Generic;

public class PlayerDataService : LoadableService
{
    private List<PlayerDataControllerBase> _controllers;
    private PlayerLevelsConfig _levelsConfig;

    public PlayerDynamicData DynamicData { get; private set; }
    public PlayerStaticData StaticData { get; private set; }

    public PlayerDataService(SignalBus signalBus, PlayerLevelsConfig levelConfig) : base(signalBus)
    {
        _levelsConfig = levelConfig;
        DynamicData = new PlayerDynamicData() { Health = 100 };
        StaticData = new PlayerStaticData() { Experience = 0, Level = 1 };
        InitControllers();
    }

    public override void OnServicesLoaded(params LoadableService[] services)
    {

    }

    private void InitControllers()
    {
        _controllers = new List<PlayerDataControllerBase>()
        {
            new PlayerExpirienceController(_signalBus,_levelsConfig)
        };
    }
}

public class PlayerDynamicData
{
    public float Health;
}

public class PlayerStaticData
{
    public float Experience;
    public int Level;
}
