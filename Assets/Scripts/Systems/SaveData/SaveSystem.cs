using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
public class SaveSystem
{
    private static SaveProfileData _saveData = new SaveProfileData();
    public static string GetSaveFilePath(string characterName)
    {
        // string characterName = GameManager.Instance.GetSelectedPlayerName();
        string saveFile = Application.persistentDataPath + "/save_"+ characterName + ".json";
        return saveFile;
    }
    public static void Save()
    {
        // Save the currently selected profile (keeps extra fields like highestUnlockedMap).
        if (GameManager.Instance != null && GameManager.Instance.getCurrentSaveProfileData() != null)
        {
            _saveData = GameManager.Instance.getCurrentSaveProfileData();
            _saveData.EnsureDefaults();
        }
        HandleSaveData();

        string characterName = null;
        if (GameManager.Instance != null && GameManager.Instance.localPlayer != null)
            characterName = GameManager.Instance.localPlayer.characterName;

        if (string.IsNullOrWhiteSpace(characterName) && _saveData != null)
            characterName = _saveData.playerSaveData.characterName.ToString();

        if (string.IsNullOrWhiteSpace(characterName))
        {
            Debug.LogWarning("SaveSystem.Save skipped: no characterName available.");
            return;
        }

        File.WriteAllText(GetSaveFilePath(characterName), JsonUtility.ToJson(_saveData, true));
    }
    private static void HandleSaveData()
    {
        InventoryManager.Instance.Save(ref _saveData.itemSaveData);
        if(GameManager.Instance.localPlayer!= null)
            GameManager.Instance.localPlayer.Save(ref _saveData.playerSaveData);
        Console.WriteLine("Save data");
    }
    public static List<SaveProfileData> LoadAllProfiles()
    {
        string folder = Application.persistentDataPath;
        string[] files = Directory.GetFiles(folder, "save_*.json");
        List<SaveProfileData> profiles = new List<SaveProfileData>();
        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            SaveProfileData data = JsonUtility.FromJson<SaveProfileData>(json);
            if (data != null)
                data.EnsureDefaults();
            profiles.Add(data);
        }
        return profiles;
    }

    public static SaveProfileData Load(string characterName)
    {
        string path = GetSaveFilePath(characterName);
        if (!File.Exists(path))
            return default;

        string json = File.ReadAllText(path);
        SaveProfileData data = JsonUtility.FromJson<SaveProfileData>(json);
        if (data != null)
            data.EnsureDefaults();
        return data;
    }
}
