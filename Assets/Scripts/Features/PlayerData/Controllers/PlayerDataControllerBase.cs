﻿public class PlayerDataControllerBase
{
    protected SignalBus _signalBus;

    public PlayerDataControllerBase(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }
}
