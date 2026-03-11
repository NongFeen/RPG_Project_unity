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
        print("Refresing Weapon UI");
        //inv
        foreach (Transform child in invSlotParent)
            Destroy(child.gameObject);
        
        foreach (WeaponInstance item in InventoryManager.Instance.weaponInventoryItems)
        {
            GameObject obj = Instantiate(slotPrefab, invSlotParent.transform);
            WeaponInventorySlot slot = obj.GetComponent<WeaponInventorySlot>();
            slot.SetWeapon(item);
        }
        //equip
        foreach (Transform child in equipedSlotParent)
            Destroy(child.gameObject);
        // print(InventoryManager.Instance.equippedItems.Count);
        foreach (WeaponInstance item in InventoryManager.Instance.equippedWeapons)
        {
            GameObject obj = Instantiate(slotPrefab, equipedSlotParent.transform);
            WeaponInventorySlot slot = obj.GetComponent<WeaponInventorySlot>();
            slot.SetWeapon(item);
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
