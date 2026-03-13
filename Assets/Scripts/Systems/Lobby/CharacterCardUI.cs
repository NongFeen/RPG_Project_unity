using System;
using TMPro;
using UnityEngine;
using static SaveSystem;

public class CharacterCardUI : MonoBehaviour
{
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
    }
    public void SetUp(int index,LobbyPlayerData lobbyPlayerData)
    {
        // print(lobbyPlayerData.playerSaveData.characterName.ToString());
        charNameText.GetComponent<TextMeshProUGUI>().text = lobbyPlayerData.saveData.characterName.ToString();
        charLevelText.GetComponent<TextMeshProUGUI>().text = lobbyPlayerData.saveData.level.ToString();
        charClassText.GetComponent<TextMeshProUGUI>().text = lobbyPlayerData.saveData.characterClass.ToString();
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
}
