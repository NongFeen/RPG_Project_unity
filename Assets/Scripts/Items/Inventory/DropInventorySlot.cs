using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropInventorySlot : MonoBehaviour, IDropHandler
{
    // public void OnDrop(PointerEventData eventData)
    // {
    //     print("Droping item");
    //     if (transform.childCount == 0 )
    //     {
    //         DragInventoryItem dragItem = eventData.pointerDrag.GetComponent<DragInventoryItem>();
    //         dragItem.SetParentAfterDrag(transform);
    //     }
    // }
    public void OnDrop(PointerEventData eventData)
    {
        DragInventoryItem dragItem = eventData.pointerDrag.GetComponent<DragInventoryItem>();
        InventorySlot fromSlot = dragItem.GetParentAfterDrag().GetComponent<InventorySlot>();
        InventorySlot toSlot = GetComponent<InventorySlot>();

        if (fromSlot == null || toSlot == null) return;

        // Swap only if the destination is empty
        if (toSlot.IsEmpty)
        {
            // Move UI object
            dragItem.SetParentAfterDrag(transform);

            // Move data
            toSlot.SetItem(fromSlot.GetStoreItem(), fromSlot.GetQuantity());
            fromSlot.ClearSlot();
        }
        else
        {
            Debug.Log("Slot already occupied!");
        }
        // FindAnyObjectByType<Inventory>().SyncSlots();
    }
}
