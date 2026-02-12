using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "itemDropOnMap", menuName = "Maps/Item Drop On Map")]
public class MapItemDrop : ScriptableObject
{
    [SerializeField]public List<ItemDropEntry> possibleDrops;

    public List<Item> GetDroppedItems()
    {
        List<Item> droppedItems = new List<Item>();
        foreach (var entry in possibleDrops)
        {
            float roll = Random.Range(0f, 1f);
            if (roll <= entry.dropChance)
            {
                droppedItems.Add(entry.item);
            }
        }
        return droppedItems;
    }
}
[System.Serializable]
public class ItemDropEntry
{
    public Item item;
    [Range(0f,1f)] public float dropChance; // Value between 0 and 1
}