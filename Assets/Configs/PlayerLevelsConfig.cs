using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerLevelsConfig", menuName = "Configs/PlayerLevelsConfig")]
public class PlayerLevelsConfig : ScriptableObject
{
    public int[] ExpOnLevel;

    public float GetCurrentLevelNormalizedExp(int exp)
    {
        int currentLevel = GetLevelByExp(exp);
        int expRequiredByCurrentLevel = ExpOnLevel[currentLevel - 1];
        int nextLevelExp = ExpOnLevel[currentLevel];

        return (float)(exp - expRequiredByCurrentLevel) / (nextLevelExp - expRequiredByCurrentLevel);
    }

    public int GetLevelByExp(int exp)
    {
        for (int i = 0; i < ExpOnLevel.Length; i++)
        {
            if (exp < ExpOnLevel[i]) return i;
        }
        return -1;
    }
}
