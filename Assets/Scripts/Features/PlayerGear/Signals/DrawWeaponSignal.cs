public class DrawWeaponSignal : ISignal
{
    public bool Draw;
    public PlayerView Target;

    public DrawWeaponSignal(bool draw, PlayerView target)
    {
        Draw = draw;
        Target = target;
    }

}
