using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotNew : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public ItemInstance StoredItem { get; private set; }
    public bool IsEmpty => StoredItem == null;

    public void SetItem(WeaponInstance instance)
    {
        StoredItem = instance;
        ShowItem();
    }

    public void SetItem(ItemInstance instance)
    {
        StoredItem = instance;
        ShowItem();
    }
    public void ShowItem()
    {
        if (IsEmpty || StoredItem.IsEmpty) return;
        GameObject obj = Instantiate(StoredItem.itemData.itemPrefab, this.transform);
        obj.GetComponent<UnityEngine.UI.Image>().sprite = StoredItem.itemData.image;
    }
    public void ClearSlot()
    {
        StoredItem = null;
        ShowItem();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (StoredItem is WeaponInstance weaponInstance)
        {
            ItemDetailPanel.Instance.Show(weaponInstance);
        }
        else
            ItemDetailPanel.Instance.Show(StoredItem);

    }
}
