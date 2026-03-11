using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "System/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items;
    public Dictionary<int, Item> itemDictionary;

    public void Initialize()
    {
        itemDictionary = new Dictionary<int, Item>();
        foreach (var item in items)
        {
            if (!itemDictionary.ContainsKey(item.id))
                itemDictionary[item.id] = item;
            else
                Debug.LogWarning($"Duplicate ID found: {item.id} for item {item.name}");
            // Debug.Log(item.name);
        }
    }
    public Item GetItemByID(int id)
    {
        if (itemDictionary == null)
            Initialize();

        if (itemDictionary.TryGetValue(id, out Item foundItem))
            return foundItem;
        if (id == -1) return null;
        Debug.LogWarning($"Item ID '{id}' not found!");
        return null;
    }
    public Weapon GetWeaponByID(int id)
    {
        if (itemDictionary == null)
            Initialize();
        itemDictionary.TryGetValue(id, out Item foundItem);
        if(foundItem is Weapon weapon)
            return weapon;
        if (id == -1) return null;
        Debug.LogWarning($"Item ID '{id}' not found!");
        return null;
    }
}
