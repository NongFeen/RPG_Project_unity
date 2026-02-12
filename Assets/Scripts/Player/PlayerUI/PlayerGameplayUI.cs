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

    public void SetPlayerData(PlayerStats stats, PlayerEquipedItem equip)
    {
        playerStats = stats;
        playerEquipedItem = equip;
        playerHealthUI.SetPlayerData(stats, equip);
        playerWeaponUI.SetPlayerData(stats, equip);
    }
}
