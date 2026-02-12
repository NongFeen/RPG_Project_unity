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
        HandleSaveData();
        File.WriteAllText(GetSaveFilePath(GameManager.Instance.localPlayer.characterName),JsonUtility.ToJson(_saveData,true));
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
        return JsonUtility.FromJson<SaveProfileData>(json);
    }
}
