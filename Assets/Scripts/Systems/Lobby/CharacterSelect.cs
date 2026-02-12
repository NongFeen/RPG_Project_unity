using System;
using UnityEngine;
using static SaveSystem;

public class CharacterSelect : MonoBehaviour
{
    [SerializeField]GameObject selectCharacterCardPrefab;
    [SerializeField]GameObject createCharacterCardPrefab;
    [SerializeField]GameObject characterList;
    // [SerializeField] GameObject lobbyMenu;
    public void OnEnable()
    {
        foreach (Transform child in characterList.transform)
            Destroy(child.gameObject);

        var allProfiles = SaveSystem.LoadAllProfiles();
        // print(allProfiles.Count);
        for (int i = 0; i < allProfiles.Count; i++)
        {
            CreateCharacterCard(i, allProfiles[i]);
        }
    }
    void CreateCharacterCard(int index, SaveProfileData saveProfileData)
    {
        var card = Instantiate(selectCharacterCardPrefab,characterList.transform);
        card.GetComponent<CharacterCardUI>().SetUp(index,saveProfileData);
    }
    public void CloseSelectedMenu()
    {
        this.gameObject.SetActive(false);
    }
    public void OpenSelectMenu()
    {
        this.gameObject.SetActive(true);
    }
}
