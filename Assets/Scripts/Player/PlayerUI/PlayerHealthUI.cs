using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Unity.Collections;

public class PlayerHealthUI : MonoBehaviour,IPlayerStatUI
{
    [Serializable]
    public struct ClassIconSprite
    {
        public ClassType classType;
        public Sprite sprite;
    }
    [SerializeField] TextMeshProUGUI characterNameText;
    [SerializeField] Image playerIcon;
    [SerializeField] TextMeshProUGUI hpNumberText;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider HPbar;
    [SerializeField] private Sprite defaultIconSprite;
    [SerializeField] private List<ClassIconSprite> classIconSprites = new List<ClassIconSprite>();
    public void SetPlayerData(GameObject player)
    {
        playerStats = player.GetComponent<PlayerStats>();
        UpdateHp(0, playerStats.currentHP.Value);
        playerStats.currentHP.OnValueChanged += UpdateHp;
        playerStats.activeStats.OnValueChanged += UpdateMaxHp;
        playerStats.playerClass.OnValueChanged += UpdateClassIcon;
        playerStats.playerName.OnValueChanged += UpdatePlayerName;
        ApplyClassIcon(playerStats.playerClass.Value);
        // player name is already update before UI, so it need manually set
        characterNameText.text = playerStats.playerName.Value.ToString();
        // print("Name : "+characterNameText.text + "  " + playerStats.playerName.Value.ToString());
    }
    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.currentHP.OnValueChanged -= UpdateHp;
            playerStats.activeStats.OnValueChanged -= UpdateMaxHp;
            playerStats.playerClass.OnValueChanged -= UpdateClassIcon;
            playerStats.playerName.OnValueChanged -= UpdatePlayerName;
        }
    }
    private void UpdateHp(float oldValue, float newValue)
    {
        hpNumberText.text = $"{newValue:F0} / {playerStats.activeStats.Value.health:F0}";
        HPbar.value = newValue / playerStats.activeStats.Value.health;
    }
    private void UpdateMaxHp(Stats oldValue, Stats newValue)
    {
        hpNumberText.text = $"{playerStats.currentHP.Value:F0} / {newValue.health:F0}";
    }

    private void UpdateClassIcon(ClassType oldClass, ClassType newClass)
    {
        ApplyClassIcon(newClass);
    }
    private void UpdatePlayerName(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        print("PLayerName "+ playerStats.playerName.Value);
        characterNameText.text = newValue.ToString();
    }
    private void ApplyClassIcon(ClassType classType)
    {
        if (playerIcon == null) return;

        Sprite target = null;
        for (int i = 0; i < classIconSprites.Count; i++)
        {
            if (classIconSprites[i].classType == classType)
            {
                target = classIconSprites[i].sprite;
                break;
            }
        }

        if (target == null)
        {
            target = defaultIconSprite;
        }

        if (target != null)
        {
            playerIcon.sprite = target;
        }
    }
}
