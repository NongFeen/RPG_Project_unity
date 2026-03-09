using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicInventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform invSlotParent;
    [SerializeField] private Transform equipedSlotParent;
    
    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
            Debug.Log("RelicInventoryUI subscribed");
            RefreshUI();
        }
        else
        {
            StartCoroutine(WaitForInventoryManager());
        }
    }
    private void OnDisable()
    {
        Debug.Log("RelicInventoryUI unsubscribed");
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
    }
    public void RefreshUI()
    {
        print("Refresing UI");
        //inv
        foreach (Transform child in invSlotParent)
            Destroy(child.gameObject);
        foreach (RelicInstance relic in InventoryManager.Instance.relicInventoryItems)
        {
            GameObject obj = Instantiate(slotPrefab, invSlotParent.transform);
            RelicInventorySlot slot = obj.GetComponent<RelicInventorySlot>();
            slot.SetRelic(relic);
        }
        //equip
        foreach (Transform child in equipedSlotParent)
            Destroy(child.gameObject);
        // print(InventoryManager.Instance.equippedItems.Count);
        foreach (var relic in InventoryManager.Instance.equippedRelics)
        {
            GameObject obj = Instantiate(slotPrefab, equipedSlotParent.transform);
            RelicInventorySlot slot = obj.GetComponent<RelicInventorySlot>();
            slot.SetRelic(relic);
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
