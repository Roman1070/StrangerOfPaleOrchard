using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AttackType
{
    OneHanded,
    TwoHanded,
    Disarmed
}

[CreateAssetMenu(fileName = "PlayerCombatConfig", menuName = "Configs/PlayerCombatConfig")]
public class PlayerCombatConfig : ScriptableObject
{
    public PlayerAttackData[] Attacks;
    public float AttackRange;

    public PlayerAttackData GetAttackById(string id) => Attacks.First(a => a.Id == id);

    public PlayerAttackData GetRandomAttack(string expceptId, AttackType targetAttackType)
    {
        var attacks = Attacks.Where(a => a.Id != expceptId && a.TargetAttackType==targetAttackType).ToArray();
        return attacks[UnityEngine.Random.Range(0, attacks.Length)];
    }

    public static readonly Dictionary<AttackType, string> LayersMappings = new Dictionary<AttackType, string>()
    {
        {AttackType.Disarmed,"CombatLayerDisarmed" },
        {AttackType.OneHanded,"CombatLayerOneHanded" },
        {AttackType.TwoHanded,"CombatLayerTwoHanded" },
    };
}

[Serializable]
public class PlayerAttackData
{
    public string Id;
    public AttackType TargetAttackType;
    public float Duration;
    public float DamageMultiplier;
}