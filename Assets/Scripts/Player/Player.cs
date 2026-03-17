using System;
using NUnit.Framework;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public Rigidbody2D rb;
    [SerializeField] private InputReader inputReader;
    [SerializeField] public PlayerExperience playerExperience;
    [SerializeField] public PlayerStats playerStats;

    public string characterName; 
    public int experience;
    public int level;
    public int upgradePoints;
    [SerializeField]public BonusStats bonusStats;
    public ClassType characterClass;
    void Start()
    {
        if(!IsOwner) return;
        //sub to move event
        inputReader.Init();
        // InventoryManager.Instance.OnEquipmentChanged += ChangeEquipItem;
        Load(GameManager.Instance.getCurrentSaveProfileData().playerSaveData);
    }

    public void AddExperience(int experienceGained)
    {
        playerExperience.AddExperience(experienceGained);
    }
    public bool SpendUpgradePoint(StatType stat)
    {
        if (upgradePoints <= 0)
            return false;

        upgradePoints--;

        switch (stat)
        {
            case StatType.Health:
                bonusStats.bonusHealth += 10;
                break;

            case StatType.Defense:
                bonusStats.bonusDefense += 3;
                break;

            case StatType.CritChance:
                bonusStats.bonusCritChance += 0.01f;
                break;

            case StatType.CritDamage:
                bonusStats.bonusCritDamage += 0.02f;
                break;
        }
        return true;
    }
    public void ChangeClass(ClassType newClass)
    {
        if (level != 15)
            return;

        if (characterClass == newClass)
            return;
        characterClass = newClass;
        
        playerStats.ChangeClassServerRpc(newClass);
    }


    #region save&load
    public void Save(ref PlayerSaveData data)
    {
        if(playerExperience != null)
        {
            level = playerExperience.CurrentLevel;
            experience = playerExperience.TotalExperience;
        }
        data.characterName = characterName;
        data.level = level;
        data.experience = experience;
        data.characterClass = characterClass;
        data.upgradePoints = upgradePoints;
        data.bonusStats = bonusStats;
        // Debug.Log($"Saved Player: {characterName} Lv.{level} EXP:{experience}");
    }
    public void Load(PlayerSaveData data)
    {
        characterName = data.characterName.ToString();
        level = data.level;
        experience = data.experience;
        characterClass = data.characterClass;
        if(playerExperience != null)
        {
            playerExperience.SetData(level, experience);
        }
        upgradePoints = data.upgradePoints;
        bonusStats = data.bonusStats;   
        Debug.Log($"Loaded Player: {characterName} Lv.{level} EXP:{experience} BonusStats:{bonusStats}");
    }
    #endregion
}