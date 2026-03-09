using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemDetailPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescText;
    [SerializeField] private GameObject itemDetailPanelObject;
    [SerializeField] private GameObject weaponStatsPanel;
    
    private ItemInstance currentShowItem;
    public static ItemDetailPanel Instance { get; private set; }
    //stats
    [SerializeField] private List<StatTextEntry> statTextEntries;
    [System.Serializable]
    public class StatTextEntry
    {
        public string statId;
        public TextMeshProUGUI text;
    }
    private Dictionary<string, TextMeshProUGUI> statDict;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        itemDetailPanelObject.SetActive(false); // hide on start
        statDict = new Dictionary<string, TextMeshProUGUI>();
        foreach (var entry in statTextEntries)
        statDict.Add(entry.statId, entry.text);
    }

    public void Show(WeaponInstance item)
    {
        currentShowItem = item;
        itemNameText.text = item.itemData.itemName;
        itemDescText.text = item.itemData.description;
        statDict["BaseDamage"].text = item.weaponData.baseDamage.ToString();
        statDict["BaseFireRate"].text = item.weaponData.baseFirerate.ToString();
        statDict["BaseMaxAmmo"].text = item.weaponData.baseMaxammo.ToString();
        statDict["BaseReloadSpeed"].text = item.weaponData.baseReloadSpeed.ToString("F2");
        statDict["BonusDamage"].text = item.bonusStat.bonusDamage.ToString();
        statDict["CritRate"].text = (item.bonusStat.critRate * 100).ToString("F2") + "%";
        statDict["CritDamage"].text = (item.bonusStat.critDamage * 100).ToString("F2") + "%";
        itemDetailPanelObject.SetActive(true);
        weaponStatsPanel.SetActive(true);
    }
    public void Show(ItemInstance item)
    {
        currentShowItem = item;
        itemNameText.text = item.itemData.itemName;
        itemDescText.text = item.itemData.description;
        // statDict["BaseDamage"].text = item.weaponData.baseDamage.ToString();
        // statDict["BaseFireRate"].text = item.weaponData.baseFirerate.ToString();
        // statDict["BaseMaxAmmo"].text = item.weaponData.baseMaxammo.ToString();
        // statDict["BaseReloadSpeed"].text = item.weaponData.baseReloadSpeed.ToString("F2");
        // statDict["BonusDamage"].text = item.bonusDamage.ToString();
        // statDict["CritRate"].text = item.critRate.ToString() + "%";
        // statDict["CritDamage"].text = item.critDamage.ToString() + "%";
        itemDetailPanelObject.SetActive(true);
        weaponStatsPanel.SetActive(true);
    }
    public void Show(RelicInstance relic)
    {
        //hide weapon stats
        // item name -> rarity
        //Item description -> relic attributes
        currentShowItem = relic;
        itemNameText.text = relic.rarity.ToString() + " Relic";
        string attrString = "";
        foreach (RelicAttribute attr in relic.attributes)
        {
            attrString += $"{attr}\n";
        }
        itemDescText.text = attrString;
        itemDetailPanelObject.SetActive(true);
        weaponStatsPanel.SetActive(false);
    }
    public void Hide()
    {
        // print("hiding panel");
        this.itemDetailPanelObject.SetActive(false);
    }
    public void EqiupItem(int slot)
    {
        if (currentShowItem == null)
        {
            Debug.LogWarning("No item selected to equip.");
            return;
        }
        if(currentShowItem is WeaponInstance weapon)
        {
            InventoryManager.Instance.EquipItem(weapon, slot);
        }else
        {
            Debug.LogWarning("Current item is not a weapon instance.");
            return;
        }

        currentShowItem = null;
        Hide();
    }
}
