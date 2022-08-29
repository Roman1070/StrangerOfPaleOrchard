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
    public NPCAttackData[] Attacks;
    public NPCLevelConfig[] LevelData;

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

[Serializable]
public class NPCLevelConfig
{
    public int Level;
    public int MaxHealth;
    public int Damage;
    public int ExpOnKill;
    public EnumerableItem[] RewardsOnKill;
}