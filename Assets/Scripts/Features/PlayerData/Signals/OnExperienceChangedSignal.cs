using UnityEngine;

public class OnExperienceChangedSignal :ISignal
{
    public int Value;
    public PlayerView Player;

    public OnExperienceChangedSignal(int value, PlayerView player)
    {
        Value = value;
        Player = player;
    }
}
