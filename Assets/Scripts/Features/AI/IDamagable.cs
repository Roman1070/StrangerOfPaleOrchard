using UnityEngine;

public interface IDamagable
{
    public Transform Transform { get; }
    public void TakeDamage(int damage);
    public bool IsAlive { get; }
}

public class ChangePlayersHealthSignal : ISignal
{
    public float Delta;

    public ChangePlayersHealthSignal(float delta)
    {
        Delta = delta;
    }
}