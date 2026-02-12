using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Inventory : MonoBehaviour
{
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private List<InventorySlot> inventorySlots;
    // [SerializeField] private EquipedItemInventory equipedItemInventory;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private int slotCount = 1;

    private void Awake()
    {
        GenerateSlots();
    }
    private void GenerateSlots()
    {
        for (int i = 0; i < slotCount; i++)
        {
            GameObject obj = Instantiate(inventorySlotPrefab, this.transform);
            InventorySlot slot = obj.GetComponent<InventorySlot>();
            slot.ClearSlot();
            slot.slotIndex = i;
            inventorySlots.Add(slot);
        }
        AddItem(itemDatabase.GetItemByID(0));
        AddItem(itemDatabase.GetItemByID(1));
        // AddItem(itemDatabase.GetItemByID(2));

    }
    public void AddItem(Item item, int quantity = 1)
    {
        foreach (InventorySlot invSlot in inventorySlots)
        {
            // print(invSlot.IsEmpty);
            if (invSlot.IsEmpty)
            {
                invSlot.AddItem(item, quantity);
                break;
            }
        }
    }
    public void TestAddItem()
    {
        AddItem(itemDatabase.GetItemByID(1));
        // AddItem(itemDatabase.GetItemByID(2));
    }
    public void SyncSlots()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i] = transform.GetChild(i).GetComponent<InventorySlot>();
        }
    }
}
