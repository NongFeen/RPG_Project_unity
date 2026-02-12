using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponUI : MonoBehaviour,IPlayerStatUI
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerEquipedItem playerEquiped;
    [SerializeField] private List<TextMeshProUGUI> weaponAmmoText;
    [SerializeField] private List<Image> weaponAmmoImg;


    void OnDestroy()
    {
    }
    public void SetPlayerData(PlayerStats stats, PlayerEquipedItem equip)
    {
        this.playerStats = stats;
        playerEquiped = equip;
    }
    void Update()
    {
        if (playerEquiped == null) return;
        for (int i = 0; i < playerEquiped.equipSlots.Count; i++)
        {
            if (playerEquiped.equipSlots[i] == null)
            {
                weaponAmmoText[i].text = $"0 / 0";
                weaponAmmoImg[i].sprite = null;
                weaponAmmoImg[i].color = new Color(255, 255, 255, 0);
                
            }
            else
            {
                weaponAmmoText[i].text = $"{playerEquiped.equipSlots[i].currentAmmo} / {playerEquiped.equipSlots[i].maxAmmo}";
                weaponAmmoImg[i].color = new Color(255, 255, 255, 255);
                weaponAmmoImg[i].sprite = playerEquiped.equipSlots[i].weaponInstance.weaponData.image;
            }
        }
    }
}
