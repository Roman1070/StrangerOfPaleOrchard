using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerLevelsConfig", menuName = "Configs/PlayerLevelsConfig")]
public class PlayerLevelsConfig : ScriptableObject
{
    public int[] ExpOnLevel;

    public float GetCurrentLevelNormalizedExp(int exp)
    {
        float currentLevelExp = exp;
        int i;
        for(i = 0; i < ExpOnLevel.Length; i++)
        {
            if (currentLevelExp >= ExpOnLevel[i]) currentLevelExp -= ExpOnLevel[i];
            else break;
        }
        int currentLevelIndex = i-1;
        currentLevelExp += ExpOnLevel[currentLevelIndex];
        return (currentLevelExp - ExpOnLevel[currentLevelIndex]) / (ExpOnLevel[currentLevelIndex + 1] - ExpOnLevel[currentLevelIndex]);
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
