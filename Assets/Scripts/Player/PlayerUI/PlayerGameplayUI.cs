using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Runtime.Serialization;
public class PlayerGameplayUI : MonoBehaviour,IPlayerStatUI
{
    [SerializeField] private Player player;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerEquipedItem playerEquipedItem;
    [SerializeField] private PlayerHealthUI playerHealthUI;
    [SerializeField] private PlayerWeaponUI playerWeaponUI;
    [SerializeField] private PlayerSkillUI playerSkillUI;
    [SerializeField] private PlayerBuffUI playerBuffUI;

    public void SetPlayerData(GameObject player)
    {
        this.player = player.GetComponent<Player>();
        this.playerStats = player.GetComponent<PlayerStats>();
        this.playerEquipedItem = player.GetComponent<PlayerEquipedItem>();

        playerHealthUI.SetPlayerData(player);
        playerWeaponUI.SetPlayerData(player);
        playerBuffUI.SetPlayerData(player);
    }
    public void SetPlayerSkill(GameObject player)
    {
        playerSkillUI.SetPlayerData(player);
    }
}
