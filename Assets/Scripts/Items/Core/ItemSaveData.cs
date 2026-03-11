using System.Collections.Generic;

[System.Serializable]
public struct ItemSaveData
{
    public List<WeaponInstance> equipList;
    public List<WeaponInstance> itemList;
    public List<RelicInstance> relicList;
    public List<RelicInstance> relicEquipped;

    public static ItemSaveData Create()
    {
        return new ItemSaveData
        {
            equipList = new List<WeaponInstance>(),
            itemList = new List<WeaponInstance>(),
            relicList = new List<RelicInstance>(),
            relicEquipped = new List<RelicInstance>()
        };
    }
}