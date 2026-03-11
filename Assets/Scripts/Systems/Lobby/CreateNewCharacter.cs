using TMPro;
using UnityEngine;

public class CreateNewCharacter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameInput;
    public void CreateCharacter()
    {
        string characterName = nameInput.text;

        if (string.IsNullOrWhiteSpace(characterName))
        {
            Debug.LogWarning("Character name required");
            return;
        }

        PlayerSaveData save = new PlayerSaveData
        {
            characterName = characterName,
            level = 1,
            experience = 0,
            characterClass = ClassType.Human, // default class
            bonusStats = new BonusStats()
        };

        SaveProfileData saveProfileData = new SaveProfileData
        {
            playerSaveData = save,
            itemSaveData = ItemSaveData.Create()
        };
        AddStartingItem(ref saveProfileData);
        StartSinglePlayer(saveProfileData);
    }
    void AddStartingItem(ref SaveProfileData save)
    {
        Weapon weapon = GameDatabase.Instance.GetItemDatabase().GetWeaponByID(1);
        WeaponInstance startingItem = new WeaponInstance(weapon);
        save.itemSaveData.equipList.Add(startingItem);
    }
    void StartSinglePlayer(SaveProfileData save)
    {
        GameManager.Instance.SelectPlayer(save);
        GameManager.Instance.StartGame(MapName.Story_01);
    }
}
