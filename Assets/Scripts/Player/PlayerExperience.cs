using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExperience : MonoBehaviour
{
    [Header("Experience")]
    [SerializeField] public AnimationCurve experienceCurve;

    [SerializeField]int currentLevel, totalExperience;
    int previousLevelsExperience, nextLevelsExperience;

    public int CurrentLevel => currentLevel;
    public int TotalExperience => totalExperience;
    [SerializeField] public Player player;

    [Header("Interface")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI experienceText;
    [SerializeField] Image experienceFill;

     void Start()
    {
        UpdateLevel();
    }

    public void AddExperience(int amount)
    {
        totalExperience += amount;
        CheckForLevelUp();
        // UpdateInterface();
    }

    void CheckForLevelUp()
    {
        if(totalExperience >= nextLevelsExperience)
        {
            currentLevel++;
            UpdateLevel();

            if(player != null)
            {
                player.level = currentLevel;
                player.experience = totalExperience;
                player.upgradePoints += 1;
            }
        }
    }

    void UpdateLevel()
    {
        previousLevelsExperience = (int)experienceCurve.Evaluate(currentLevel);
        nextLevelsExperience = (int)experienceCurve.Evaluate(currentLevel + 1);
        // UpdateInterface();
    }

    void UpdateInterface()
    {
        int start = totalExperience - previousLevelsExperience;
        int end = nextLevelsExperience - previousLevelsExperience; 

        levelText.text = currentLevel.ToString();
        experienceText.text = start + " exp / " + end + " exp";
        experienceFill.fillAmount = (float)start / (float)end;
    }
    public void SetData(int level, int exp)
    {
        currentLevel = level;
        totalExperience = exp;
        
        UpdateLevel();
    }
}
