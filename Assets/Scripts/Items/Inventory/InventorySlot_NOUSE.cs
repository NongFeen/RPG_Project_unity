using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[System.Serializable]
public class InventorySlot : MonoBehaviour
{
    [SerializeReference] private ItemInstance storedItem = null;
    public ItemInstance StoredItem => storedItem;
    public bool IsEmpty => storedItem == null;
    private int quantity;
    public Image img;
    public int slotIndex; 

    public void ClearSlot()
    {
        storedItem = null;
        quantity = 0;
    }
    void Update()
    {
        if (storedItem is WeaponInstance weapon)
        {
            // print(weapon.bonusDamage);
        }
    }
    public ItemInstance GetStoreItem() => storedItem;
    public int GetQuantity() => quantity;
    public void AddItem(Item item, int quantity = 1)
    {
        storedItem = ItemInstance.CreateInstance(item);

        if (item is Weapon weapon)
        {
            // Debug.Log($"Weapon Bonus Damage {weapon.weaponStat.bonusDamage}");
        }
        this.quantity = quantity;
        GameObject obj = Instantiate(item.itemPrefab, this.transform);
        obj.GetComponent<UnityEngine.UI.Image>().sprite = storedItem.itemData.image;
    }
    public void SetItem(ItemInstance itemInstance, int quantity)
    {
        storedItem = itemInstance;
        this.quantity = quantity;
    }
}
