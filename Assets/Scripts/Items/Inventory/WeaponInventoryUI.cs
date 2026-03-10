using System.Collections;
using UnityEngine;

public class WeaponInventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform invSlotParent;
    [SerializeField] private Transform equipedSlotParent;

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
            // Debug.Log("WeaponInventoryUI subscribed");
            RefreshUI();
        }
        else
        {
            StartCoroutine(WaitForInventoryManager());
        }
    }
    private void OnDisable()
    {
        // Debug.Log("WeaponInventoryUI unsubscribed");
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
    }
    public void RefreshUI()
    {
        print("Refresing UI");
        //inv
        foreach (Transform child in invSlotParent)
            Destroy(child.gameObject);
        foreach (WeaponInstance item in InventoryManager.Instance.weaponInventoryItems)
        {
            // print("test");
            // InventorySlotNew slot = Instantiate(slotPrefab, invSlotParent);
            GameObject obj = Instantiate(slotPrefab, invSlotParent.transform);
            InventorySlotNew slot = obj.GetComponent<InventorySlotNew>();
            slot.SetItem(item);
        }
        //equip
        foreach (Transform child in equipedSlotParent)
            Destroy(child.gameObject);
        // print(InventoryManager.Instance.equippedItems.Count);
        foreach (var item in InventoryManager.Instance.equippedWeapons)
        {
            GameObject obj = Instantiate(slotPrefab, equipedSlotParent.transform);
            InventorySlotNew slot = obj.GetComponent<InventorySlotNew>();
            slot.SetItem(item);
        }
    }
    private IEnumerator WaitForInventoryManager()
    {
        while (InventoryManager.Instance == null)
            yield return null;

        InventoryManager.Instance.OnInventoryChanged += RefreshUI;
        RefreshUI();
    }
}
