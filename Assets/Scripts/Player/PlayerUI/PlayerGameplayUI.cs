using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Runtime.Serialization;
public class PlayerGameplayUI : MonoBehaviour,IPlayerStatUI
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerEquipedItem playerEquipedItem;
    [SerializeField] private PlayerHealthUI playerHealthUI;
    [SerializeField] private PlayerWeaponUI playerWeaponUI;
    [SerializeField] private PlayerSkillUI playerSkillUI;

    public void SetPlayerData(GameObject player)
    {
        // playerStats = stats; 
        playerStats = player.GetComponent<PlayerStats>();
        // playerEquipedItem = equip;
        playerEquipedItem = player.GetComponent<PlayerEquipedItem>();;
        playerHealthUI.SetPlayerData(player);
        playerWeaponUI.SetPlayerData(player);
        // playerSkillUI.SetPlayerData(player);
    }
    public void SetPlayerSkill(GameObject player)
    {
        playerSkillUI.SetPlayerData(player);
    }
}
