using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringConst
{
    public const string CURRENCY = "Currency";
    public const string MELEE_WEAPON = "Melee weapon";
    public const string RANGED_WEAPON = "Ranged weapon";
    public const string RESOURCES = "Resources";
    public const string COLLECT = "Collect";
    public const string PRAY = "Pray";

    public const string ExitCombat = "Exit combat";
    public readonly Dictionary<AttackType, string> LayerNameByAttackType = new Dictionary<AttackType, string>()
    {
        {AttackType.Disarmed, "CombatLayerDisarmed" },
        {AttackType.OneHanded, "CombatLayerOneHanded" },
        {AttackType.TwoHanded, "CombatLayerTwoHanded" },
    };
}
