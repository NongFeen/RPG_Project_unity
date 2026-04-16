using System;

[Serializable]
public class SaveProfileData
{
    public PlayerSaveData playerSaveData;
    public ItemSaveData itemSaveData;
    public MapName highestUnlockedMap;

    public SaveProfileData()
    {
        playerSaveData = new PlayerSaveData();
        itemSaveData = new ItemSaveData();
        EnsureDefaults();
    }

    public void EnsureDefaults()
    {
        if (!Enum.IsDefined(typeof(MapName), highestUnlockedMap))
            highestUnlockedMap = MapName.Story_01;
    }
}
