using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public List<WeaponInstance> inventoryItems = new List<WeaponInstance>();
    public List<WeaponInstance> equippedItems = new List<WeaponInstance>();
    private int MAX_EQUIPPED_SLOTS = 3;
    public event Action OnInventoryChanged;
    public event Action OnEquipmentChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // TestAddItems();
        // DontDestroyOnLoad(gameObject);
        // equippedItems = new List<ItemInstance>(new ItemInstance[MAX_EQUIPPED_SLOTS]);
    }

    public void AddItem(Item item, int qty = 1)
    {
        inventoryItems.Add(WeaponInstance.CreateWeaponInstance(item, qty));
        print($"Added {item.itemName} x{qty} to inventory.");
        // Debug.Log($"OnInventoryChanged has listeners? {OnInventoryChanged != null}");
        OnInventoryChanged?.Invoke();
    }
    public void AddItemInstance(WeaponInstance itemInstance)
    {
        inventoryItems.Add(itemInstance);
        print($"Added Weapon {itemInstance.itemData.itemName} to inventory.");
        OnInventoryChanged?.Invoke();
    }

    public void EquipItem(WeaponInstance item, int slotIndex)
    {

        if (slotIndex < 0 || slotIndex >= MAX_EQUIPPED_SLOTS) return;
        if (item == null) return;

        // Check if already equipped
        int equippedIndex = equippedItems.IndexOf(item);
        if (equippedIndex >= 0 )
        {
            // UnEquipItem(equippedIndex);
            UnEquipItem(slotIndex);
            equippedItems[slotIndex] = item;
            equippedItems[equippedIndex] = null;
        }
        else
        {
            int inventoryIndex = inventoryItems.IndexOf(item);
            if (inventoryIndex >= 0)
                inventoryItems.RemoveAt(inventoryIndex);

            if (!equippedItems[slotIndex].IsEmpty)
                UnEquipItem(slotIndex);

            equippedItems[slotIndex] = item;
        }

        #region old equiped
        // //change slot from any to any
        // if (equippedItems.Contains(item))
        // {
        //     if (!equippedItems[slotIndex].IsEmpty)
        //         UnEquipItem(slotIndex);
        //     int a = equippedItems.IndexOf(item);
        //     equippedItems[a] = null;
        //     equippedItems[slotIndex] = item;
        //     // OnEquipmentChanged?.Invoke();

        //     OnInventoryChanged?.Invoke();
        //     OnEquipmentChanged?.Invoke();
        //     return;
        // }
        // //UnEquipeItem if occupie
        // if (!equippedItems[slotIndex].IsEmpty)
        // {
        //     UnEquipItem(slotIndex);
        // }
        // //EquipItem
        // equippedItems[slotIndex] = item;
        // inventoryItems.Remove(item);
        #endregion
        OnInventoryChanged?.Invoke();
        OnEquipmentChanged?.Invoke();
        // print($"itemEquiped{equippedItems[slotIndex].itemData.name}");
    }
    public void UnEquipItem(int slot)
    {
        WeaponInstance weapon = equippedItems[slot];
        if (weapon == null || weapon.IsEmpty)
            return;
        inventoryItems.Add(equippedItems[slot]);
        equippedItems[slot] = null;
    }
    public void TestAddItems()
    {
        AddItem(GameDatabase.Instance.GetItemDatabase().GetItemByID(1));
        AddItem(GameDatabase.Instance.GetItemDatabase().GetItemByID(0));
    }
    public Weapon GetWeaponById(int id)
    {
        return GameDatabase.Instance.GetItemDatabase().GetItemByID(id) as Weapon;
    }
    #region Save and Load Item
    public void Save(ref ItemSaveData data)
    {
        data.itemList = inventoryItems;
        data.equipList = equippedItems;
    }
    public void LoadInventoryFromSaveData(ItemSaveData data)
    {
        inventoryItems.Clear();
        equippedItems = new List<WeaponInstance>(new WeaponInstance[MAX_EQUIPPED_SLOTS]);

        // ---- LOAD INVENTORY ----
        foreach (var saved in data.itemList)
        {
            Weapon baseWeapon = GameDatabase.Instance.GetItemDatabase().GetItemByID(int.Parse(saved.itemID)) as Weapon;

            WeaponInstance instance = new WeaponInstance(baseWeapon);
            instance.bonusStat.bonusDamage = saved.bonusStat.bonusDamage;
            instance.bonusStat.critRate = saved.bonusStat.critRate;
            instance.bonusStat.critDamage = saved.bonusStat.critDamage;

            inventoryItems.Add(instance);
        }

        // ---- LOAD EQUIPPED ITEMS ----
        for (int i = 0; i < data.equipList.Count && i < MAX_EQUIPPED_SLOTS; i++)
        {
            var saved = data.equipList[i];
            if (saved == null) continue;

            // If slot is empty in save, continue
            if (string.IsNullOrEmpty(saved.itemID))
                continue;

            Weapon baseWeapon = GameDatabase.Instance.GetItemDatabase().GetItemByID(int.Parse(saved.itemID)) as Weapon;

            WeaponInstance instance = new WeaponInstance(baseWeapon);
            instance.bonusStat.bonusDamage = saved.bonusStat.bonusDamage;
            instance.bonusStat.critRate = saved.bonusStat.critRate;
            instance.bonusStat.critDamage = saved.bonusStat.critDamage;

            equippedItems[i] = instance;
        }

        OnInventoryChanged?.Invoke();
        OnEquipmentChanged?.Invoke();
    }
    #endregion
}
[System.Serializable]
public struct ItemSaveData
{
    public List<WeaponInstance> equipList;
    public List<WeaponInstance> itemList;
}