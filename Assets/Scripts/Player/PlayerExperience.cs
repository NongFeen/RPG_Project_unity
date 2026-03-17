using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExperience : MonoBehaviour
{
    [Header("Experience")]

    [SerializeField]int currentLevel, totalExperience;
    int previousLevelsExperience, nextLevelsExperience;

    public int CurrentLevel => currentLevel;
    public int TotalExperience => totalExperience;
    [SerializeField] public Player player;

    private const int CLASS_CHANGE_LEVEL = 15; 
    private const int MAX_LEVEL = 30; 
    private int Max_Level_Experience;

    void Awake()
    {
        Max_Level_Experience = (int)GameDatabase.Instance.GetExperienceData().experienceCurve.Evaluate(MAX_LEVEL);
    }

    public void AddExperience(int amount)
    {
        print($"Gain Exp {amount} to Current {totalExperience}");
        totalExperience += amount;
        print($"Finale Exp {totalExperience}");
        CheckForLevelUp();
        // UpdateInterface();
    }
    
    void CheckForLevelUp()
    {
        AnimationCurve curve = GameDatabase.Instance.GetExperienceData().experienceCurve;
        if (curve == null)
        {
            previousLevelsExperience = 0;
            nextLevelsExperience = 0;
            return;
        }
        previousLevelsExperience = (int)curve.Evaluate(currentLevel);
        nextLevelsExperience = (int)curve.Evaluate(currentLevel + 1);
        print($"Level {currentLevel} Exp {totalExperience}/{nextLevelsExperience} ");
        print($"Level Up? {totalExperience>=nextLevelsExperience}");
        if(totalExperience >= nextLevelsExperience)
        {
            print($"Exceed Exp should be {totalExperience-nextLevelsExperience}");
            UpdateLevel();
        }
    }

    void UpdateLevel()
    {
        if(currentLevel < 30)
        {
            //normal level up
            currentLevel++;
            if(player != null)
            {
                player.level = currentLevel;
                player.experience = totalExperience;
                player.upgradePoints += 1;
            }
            player.TryGetComponent<PlayerStats>(out var stats);
            stats.ChangeLevelServerRpc(currentLevel);
        }
        else
        {
            //at level 30. and level up again
            player.upgradePoints += 1;
            totalExperience = player.experience = Max_Level_Experience;
        }
        UpdateInterface();
    }
    public bool CanChangeClass()
    {
        return CurrentLevel == CLASS_CHANGE_LEVEL;
    }
    void UpdateInterface()
    {
        if (CanChangeClass())
        {
            // Class change UI display
        }
        else
        {
            // new stats upgrade alert!
        }
    }
    public void SetData(int level, int exp)
    {
        currentLevel = level;
        totalExperience = exp;
    }

}
