using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public List<WeaponInstance> weaponInventoryItems = new List<WeaponInstance>();
    public List<WeaponInstance> equippedWeapons = new List<WeaponInstance>(MAX_EQUIPPED_SLOTS);
    public List<RelicInstance> relicInventoryItems = new List<RelicInstance>();
    public List<RelicInstance> equippedRelics = new List<RelicInstance>(MAX_EQUIPPED_SLOTS);
    private static int MAX_EQUIPPED_SLOTS = 3;
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
    }
    #region Weapon Methods
    public void AddItem(Item item, int qty = 1)
    {
        weaponInventoryItems.Add(WeaponInstance.CreateWeaponInstance(item, qty));
        print($"Added {item.itemName} x{qty} to inventory.");
        // Debug.Log($"OnInventoryChanged has listeners? {OnInventoryChanged != null}");
        OnInventoryChanged?.Invoke();
    }
    public void AddItemInstance(WeaponInstance itemInstance)
    {
        weaponInventoryItems.Add(itemInstance);
        // print($"Added Weapon {itemInstance.itemData.itemName} to inventory.");
        OnInventoryChanged?.Invoke();
    }
    public void EquipItem(WeaponInstance item, int slotIndex)
    {

        if (slotIndex < 0 || slotIndex >= MAX_EQUIPPED_SLOTS) return;
        if (item == null) return;

        // Check if already equipped
        int equippedIndex = equippedWeapons.IndexOf(item);
        if (equippedIndex >= 0 )
        {
            if (equippedIndex == slotIndex) return;

            if (equippedWeapons[slotIndex] != null && !equippedWeapons[slotIndex].IsEmpty)
                UnEquipItem(slotIndex);

            equippedWeapons[slotIndex] = item;
            equippedWeapons[equippedIndex] = null;
        }
        else
        {
            weaponInventoryItems.Remove(item);
            // If the slot is occupied, unequip the old item first
            if (equippedWeapons[slotIndex] != null && !equippedWeapons[slotIndex].IsEmpty)
                UnEquipItem(slotIndex);

            equippedWeapons[slotIndex] = item;
        }

        OnInventoryChanged?.Invoke();
        OnEquipmentChanged?.Invoke();
        // print($"itemEquiped{equippedWeapons[slotIndex].itemData.name}");
    }
    public void UnEquipItem(int slot)
    {
        WeaponInstance weapon = equippedWeapons[slot];
        if (weapon == null || weapon.IsEmpty)
            return;
        weaponInventoryItems.Add(equippedWeapons[slot]);
        equippedWeapons[slot] = null;
    }
    public void TestAddWeaponItems()
    {
        AddItem(GameDatabase.Instance.GetItemDatabase().GetItemByID(1));
        AddItem(GameDatabase.Instance.GetItemDatabase().GetItemByID(0));
    }
    #endregion 
    #region Relic Methods
    public void TestAddRelicItems()
    {
       RelicInstance relicInstance = RelicGenerator.GenerateRelic(RelicRarity.Epic);
       AddRelicInstance(relicInstance);
    }
    public void AddRelicInstance(RelicInstance relicInstance)
    {
        relicInventoryItems.Add(relicInstance);
        // print($"Added Relic of Rarity {relicInstance.rarity} to inventory.");
        OnInventoryChanged?.Invoke();
    }
    public void EquipRelic(RelicInstance relic, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_EQUIPPED_SLOTS) return;
        if (relic == null || relic.IsEmpty) return;

        // Empty RelicInstance objects are slot placeholders, not inventory
        // items. Remove any left by older saves or the previous equip logic.
        relicInventoryItems.RemoveAll(item => item == null || item.IsEmpty);

        int equippedIndex = equippedRelics.IndexOf(relic);
        if (equippedIndex >= 0)
        {
            if (equippedIndex == slotIndex) return;

            if (equippedRelics[slotIndex] != null &&
                !equippedRelics[slotIndex].IsEmpty)
            {
                UnEquipRelic(slotIndex);
            }

            equippedRelics[slotIndex] = relic;
            equippedRelics[equippedIndex] = null;
        }
        else
        {
            int inventoryIndex = relicInventoryItems.IndexOf(relic);
            if (inventoryIndex >= 0)
                relicInventoryItems.RemoveAt(inventoryIndex);

            if (equippedRelics[slotIndex] != null &&
                !equippedRelics[slotIndex].IsEmpty)
            {
                UnEquipRelic(slotIndex);
            }

            equippedRelics[slotIndex] = relic;
        }

        OnInventoryChanged?.Invoke();
        OnEquipmentChanged?.Invoke();
        print("Change Relic");
    }
    public void UnEquipRelic(int slot)
    {
        RelicInstance relic = equippedRelics[slot];
        if (relic == null || relic.IsEmpty)
        {
            equippedRelics[slot] = null;
            return;
        }

        relicInventoryItems.Add(relic);
        equippedRelics[slot] = null;
    }
    #endregion
    #region Helper Methods
    public Stats GetRelicStats()
    {
        Stats relicStats = new Stats();

        foreach (var relic in equippedRelics)
        {
            if (relic == null) continue;

            foreach (var attr in relic.attributes)
            {
                switch (attr.type)
                {
                    case RelicAttributeType.CritRate:
                        relicStats.critRate += attr.value;
                        break;

                    case RelicAttributeType.CritDamage:
                        relicStats.critDamage += attr.value;
                        break;

                    case RelicAttributeType.Health:
                        relicStats.health += attr.value;
                        break;

                    case RelicAttributeType.BonusDamage:
                        relicStats.extraDamage += attr.value;
                        break;
                }
            }
        }
        return relicStats;
    }
    public Weapon GetWeaponById(int id)
    {
        return GameDatabase.Instance.GetItemDatabase().GetItemByID(id) as Weapon;
    }
    #endregion 
    #region Save and Load Item
    public void Save(ref ItemSaveData data)
    {
        data.itemList = weaponInventoryItems;
        data.equipList = equippedWeapons;
        data.relicList = relicInventoryItems;
        data.relicEquipped = equippedRelics;
    }
    public void LoadInventoryFromSaveData(ItemSaveData data)
    {
        weaponInventoryItems.Clear();
        equippedWeapons.Clear();
        relicInventoryItems.Clear();
        equippedRelics.Clear();

        // ---- LOAD WEAPON INVENTORY ----
        foreach (var saved in data.itemList)
        {
            Weapon baseWeapon = GameDatabase.Instance.GetItemDatabase().GetItemByID(int.Parse(saved.itemID)) as Weapon;

            WeaponInstance instance = new WeaponInstance(baseWeapon);
            instance.bonusStat.bonusDamage = saved.bonusStat.bonusDamage;
            instance.bonusStat.critRate = saved.bonusStat.critRate;
            instance.bonusStat.critDamage = saved.bonusStat.critDamage;

            weaponInventoryItems.Add(instance);
        }

        // Preserve the saved slot indices, including empty slots.
        for (int i = 0; i < MAX_EQUIPPED_SLOTS; i++)
            equippedWeapons.Add(null);

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
            equippedWeapons[i] = instance;
        }
        //weapon
        // equippedWeapons = data.equipList;
        // weaponInventoryItems = data.itemList;
        
        //relic
        relicInventoryItems = data.relicList != null
            ? data.relicList.FindAll(relic => relic != null && !relic.IsEmpty)
            : new List<RelicInstance>();

        equippedRelics = data.relicEquipped != null
            ? new List<RelicInstance>(data.relicEquipped)
            : new List<RelicInstance>();

        for (int i = 0; i < equippedRelics.Count; i++)
        {
            if (equippedRelics[i] != null && equippedRelics[i].IsEmpty)
                equippedRelics[i] = null;
        }
        
        if(equippedRelics.Count < MAX_EQUIPPED_SLOTS)
        {
            int toAdd = MAX_EQUIPPED_SLOTS - equippedRelics.Count;
            for(int i = 0; i < toAdd; i++)
            {
                equippedRelics.Add(null);
            }
        }
        OnInventoryChanged?.Invoke();
        OnEquipmentChanged?.Invoke();
    }
    #endregion
}
