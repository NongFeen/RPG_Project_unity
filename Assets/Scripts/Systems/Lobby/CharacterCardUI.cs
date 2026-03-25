using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SaveSystem;

public class CharacterCardUI : MonoBehaviour
{
    [Serializable]
    public struct ClassIconSprite
    {
        public ClassType classType;
        public Sprite sprite;
    }
    [SerializeField] private List<ClassIconSprite> classIconSprites = new List<ClassIconSprite>();
    [SerializeField]Image charIcon;
    [SerializeField]GameObject charNameText;
    [SerializeField]GameObject charLevelText;
    [SerializeField]GameObject charClassText;
    SaveProfileData saveProfileData;
    public void SetUp(int index,SaveProfileData SaveProfileData)
    {
        saveProfileData = SaveProfileData;
        // print(SaveProfileData.playerSaveData.characterName.ToString());
        charNameText.GetComponent<TextMeshProUGUI>().text = SaveProfileData.playerSaveData.characterName.ToString();
        charLevelText.GetComponent<TextMeshProUGUI>().text = SaveProfileData.playerSaveData.level.ToString();
        charClassText.GetComponent<TextMeshProUGUI>().text = SaveProfileData.playerSaveData.characterClass.ToString();
        charIcon.sprite = GetCharacterClassIcon(SaveProfileData.playerSaveData.characterClass);
    }
    public void SetUp(int index,LobbyPlayerData lobbyPlayerData)
    {
        // print(lobbyPlayerData.playerSaveData.characterName.ToString());
        charNameText.GetComponent<TextMeshProUGUI>().text = lobbyPlayerData.saveData.characterName.ToString();
        charLevelText.GetComponent<TextMeshProUGUI>().text = lobbyPlayerData.saveData.level.ToString();
        charClassText.GetComponent<TextMeshProUGUI>().text = lobbyPlayerData.saveData.characterClass.ToString();
        charIcon.sprite = GetCharacterClassIcon(lobbyPlayerData.saveData.characterClass);
    }
    public void SelectCharacter()
    {
        GameManager.Instance.SelectPlayer(saveProfileData);
        // var statsUI = FindObjectOfType<PlayerStatsUI>();
        var statsUI = FindFirstObjectByType<PlayerStatsUI>();
        // var statsUI = FindObjectsByType<PlayerStatsUI>();
        if (statsUI != null)
            statsUI.RefreshFromSelectedProfile();
        CloseSelectedMenu();
    }
    public void CloseSelectedMenu()
    {
        CharacterSelect menu = GetComponentInParent<CharacterSelect>();
        if (menu != null)
            menu.gameObject.SetActive(false);   
    }
    private Sprite GetCharacterClassIcon(ClassType classType)
    {
        Sprite classIcon = null;
        for (int i = 0; i < classIconSprites.Count; i++)
        {
            if (classIconSprites[i].classType == classType)
            {
                classIcon = classIconSprites[i].sprite;
                break;
            }
        }
        return classIcon;
    }
}
