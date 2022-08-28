using System;
using System.Linq;
using UnityEngine;

public class EnemyNPCConfig : ScriptableObject
{
    public float PatrolingDistance;
    public float PatrolSpeed;
    public float DetectionDistance;
    public float ChaseSpeed;
    public float ChaseDistance;
    public float AttackRange;
    public float MaxHealth;
    public NPCAttackData[] Attacks;

    public NPCAttackData GetRandomAttack(string exceptId=null)
    {
        return Attacks.Where(a => a.Id!= exceptId).ToArray().Random();
    }
}

[Serializable]
public class NPCAttackData
{
    public string Id;
    public float Duration;
}