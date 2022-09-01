using UnityEngine;

public class DrawWeaponRemoteSignal : ISignal
{
    public bool Draw;
    public PlayerView Player;

    public DrawWeaponRemoteSignal(bool draw, PlayerView player)
    {
        Draw = draw;
        Player = player;
    }
}
