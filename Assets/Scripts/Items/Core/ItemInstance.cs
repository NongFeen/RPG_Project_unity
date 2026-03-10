using System;
using Unity.Netcode;
using UnityEngine;
[Serializable]
public class ItemInstance 
{
    public string itemID;
    [NonSerialized]
    public Item itemData;
    // public int stackCount;
    public virtual bool IsEmpty => string.IsNullOrEmpty(itemID);
    public ItemInstance(Item itemData, int stackCount = 1)
    {
        if(itemData == null)
        {
            this.itemID = "";
            this.itemData = null;
            // this.stackCount = 0;
            return;
        }
        this.itemID = itemData.id.ToString();
        this.itemData = itemData;
        // this.stackCount = stackCount;
    }
    public static ItemInstance CreateInstance(Item itemData, int stackCount = 1)
    {
        if (itemData is Weapon weapon)
            return new WeaponInstance(weapon);
        else
            return new ItemInstance(itemData, stackCount);
    }
    public static ItemInstance CreateInstance()
    {
        return new ItemInstance(null);
    }
    
}
