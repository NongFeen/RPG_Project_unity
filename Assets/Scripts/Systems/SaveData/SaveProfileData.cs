using System;

[Serializable]
public class SaveProfileData
{
    public PlayerSaveData playerSaveData;
    public ItemSaveData itemSaveData;

    public SaveProfileData()
    {
        playerSaveData = new PlayerSaveData();
        itemSaveData = new ItemSaveData();
    }
}
