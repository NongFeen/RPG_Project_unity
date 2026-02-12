using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using NUnit.Framework;
using System;

public class PlayerEquipedItem : NetworkBehaviour
{
    [Header("Equip Item")]
    [SerializeField] public List<WeaponBehaviour> equipSlots;
    [SerializeField] public NetworkVariable<int> activeSlot = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone
    );
    public NetworkList<NetworkWeaponData> equippedWeapons = new NetworkList<NetworkWeaponData>(null,
    NetworkVariableReadPermission.Everyone);
    public WeaponBehaviour activeWeapon;
    [Header("other script")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private PlayerAiming playerAiming;
    public GameObject weaponParent;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            if (IsOwner)
            {
                // equipSlots = InventoryManager.Instance.equippedItems;
                InventoryManager.Instance.OnEquipmentChanged += RefreshEquipItem;
                inputReader.SelectActiveItemEvents += OnSwitchWeapon;
            }
            activeSlot.OnValueChanged += OnActiveSlotChanged;
            RefreshEquipItem(); // sync once on start
        }
    }
    public override void OnNetworkSpawn()
    {
        if(!IsOwner) return;
        GameManager.Instance.SetLocalPlayer(GetComponent<Player>());
        RefreshEquipItem();
    }
    public override void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnEquipmentChanged -= RefreshEquipItem;
            inputReader.SelectActiveItemEvents -= OnSwitchWeapon;
            activeSlot.OnValueChanged -= OnActiveSlotChanged;
        }

        base.OnDestroy();
    }
    private void OnEnable()
    {
        equippedWeapons.OnListChanged += UpdatePlayerEquipSlots;
        if (!IsOwner) return;
        // activeSlot.OnValueChanged += OnActiveSlotChanged;
    }

    private void OnDisable()
    {
        equippedWeapons.OnListChanged -= UpdatePlayerEquipSlots;
        if (!IsOwner) return;
        // activeSlot.OnValueChanged -= OnActiveSlotChanged;
    }

    // Switch Active Item //
    public void OnSwitchWeapon(int selectedSlot)
    {
        //same slot
        if (selectedSlot == activeSlot.Value)
            return;
        // invalid slot
        // if (selectedSlot < 0 || selectedSlot >= equipSlots.Count || equipSlots[selectedSlot].IsEmpty)
        if (selectedSlot < 0 || selectedSlot >= equipSlots.Count)
            return;
        if (IsOwner)
            SetActiveSlotServerRpc(selectedSlot);
    }

    [ServerRpc]
    private void SetActiveSlotServerRpc(int slotIndex)
    {
        activeSlot.Value = slotIndex;
        // SetActiveWeapon(slotIndex);
        OnSwitchWeapon(slotIndex);
    }

    private void OnActiveSlotChanged(int oldValue, int newValue)
    {
        OnStowWeapon();
        SetActiveWeapon(newValue);
        OnDrawWeapon();
    }

    public void SetActiveWeapon(int slotIndex)
    {
        if (equipSlots[slotIndex] == null)
        {
            return;
        }
        activeWeapon = equipSlots[slotIndex];
    }

    // Change Loadout // 
    private void RefreshEquipItem()
    {
        // Debug.Log("Refreshing EquipItem");
        if (!IsOwner) return;
        // clear old object
        if (weaponParent == null) return;
        foreach(Transform child in weaponParent.transform)
        {
            Destroy(child.gameObject); 
        }
        // Initialize new weapon prefabs
        for (int i = 0; i < equipSlots.Count; i++)
        {
            WeaponInstance instance = InventoryManager.Instance.equippedItems[i];
            if (instance == null || instance.weaponData == null) continue;

            WeaponBehaviour behaviour = CreateWeaponObject(instance);
            equipSlots[i] = behaviour;
        }
        if (IsOwner)
        {
            UpdateEquipWeaponServerRPC(GetEquipedWeaponNetworkList());
            SetActiveWeapon(activeSlot.Value);
        }
    }
    private WeaponBehaviour CreateWeaponObject(WeaponInstance weaponInstance)
    {
        if (weaponInstance == null || weaponInstance.weaponData == null)
        {
            Debug.LogWarning("Tried to create weapon object but instance or data is null!");
            return null;
        }
        GameObject prefab = weaponInstance.weaponData.weaponPrefab;
        if(prefab == null)
        {
            Debug.LogWarning("Tried to create weapon object but instance or data is null!");
            return null;
        }
        GameObject weaponObj = Instantiate(prefab, weaponParent.transform);
        
        if (!weaponObj.TryGetComponent<WeaponBehaviour>(out var behaviour))
        {
            Debug.LogWarning($"Prefab {prefab.name} has no WeaponBehaviour component!");
            return null;
        }
        behaviour.SetDefault(weaponInstance);
        return behaviour;
    }
    [ServerRpc]
    private void UpdateEquipWeaponServerRPC(NetworkWeaponData[] newList)
    {
        // print($"Sending EquipList {newList}");
        //tell server new equip item list
        
        equippedWeapons.Clear();
        foreach (var data in newList)
        {
            equippedWeapons.Add(data);
        }
        SetActiveWeapon(activeSlot.Value);
    }
    private NetworkWeaponData[] GetEquipedWeaponNetworkList()
    {
        NetworkWeaponData[] networkEquipeSlot = new NetworkWeaponData[equipSlots.Count];
        for (int i = 0; i < equipSlots.Count; i++)
        {
            // if (equipSlots[i] == null || equipSlots[i].IsEmpty)// this bug null ref
            if (equipSlots[i] == null )// this bug null ref
            {
                networkEquipeSlot[i] = NetworkWeaponData.Empty();
                continue;
            }
            // NetworkWeaponData nWeapon = new NetworkWeaponData
            // {
            //     weaponId = equipSlots[i].weaponInstance.weaponData.id,
            //     bonusDamage = equipSlots[i].bonusStat.bonusDamage,
            //     critRate = equipSlots[i].bonusStat.critRate,
            //     critDamage = equipSlots[i].bonusStat.critDamage
            // };
            NetworkWeaponData nWeapon = new NetworkWeaponData()
            {
                weaponId = equipSlots[i].weaponInstance.weaponData.id,
                weaponStat = new WeaponStat
                {
                    bonusDamage = equipSlots[i].bonusStat.bonusDamage,
                    critRate = equipSlots[i].bonusStat.critRate,
                    critDamage = equipSlots[i].bonusStat.critDamage
                }
            };
            networkEquipeSlot[i] = nWeapon;
        }
        return networkEquipeSlot;
    }
    private void UpdatePlayerEquipSlots(NetworkListEvent<NetworkWeaponData> changeEvent)
    {
        List<WeaponInstance> newEquipList = new List<WeaponInstance>();
        foreach(Transform child in weaponParent.transform)
        {
            Destroy(child.gameObject); 
        }
        foreach (NetworkWeaponData weaponData in equippedWeapons)
        {
            // print("WeaponID" + weaponData.weaponId);
            if (weaponData.weaponId == -1)
            {
                newEquipList.Add(null);
                continue;
            }
            Weapon so = InventoryManager.Instance.GetWeaponById(weaponData.weaponId);
            // WeaponInstance weaponInstance = new WeaponInstance(so)
            // {
            //     bonusDamage = weaponData.bonusDamage,
            //     critRate = weaponData.critRate,
            //     critDamage = weaponData.critDamage
            // };
            WeaponInstance weaponInstance = new WeaponInstance(so)
            {
                bonusStat = new WeaponStat
                {
                    bonusDamage = weaponData.weaponStat.bonusDamage,
                    critRate = weaponData.weaponStat.critRate,
                    critDamage = weaponData.weaponStat.critDamage
                }
            };
            newEquipList.Add(weaponInstance);
        }
        // Initialize new weapon prefabs
        for (int i = 0; i < newEquipList.Count; i++)
        {
            WeaponInstance instance = newEquipList[i];
            if (instance == null || instance.weaponData == null) continue;
            WeaponBehaviour behaviour = CreateWeaponObject(instance);
            equipSlots[i] = behaviour;
        }
        SetActiveWeapon(activeSlot.Value);
    }
    //doing weapon stuff
    public void OnStowWeapon()
    {
        if (activeWeapon.weaponInstance.weaponData != null)
        {
            activeWeapon.OnStowWeapon();
        }
    }
    public void OnDrawWeapon()
    {
        if (activeWeapon.weaponInstance.weaponData != null)
        {
            activeWeapon.OnDrawWeapon();
            Debug.Log($"Drawing {activeWeapon.weaponInstance.weaponData.name}");
            // 🔹 Play draw animation
            // 🔹 Enable weapon model
        }
    }
    public void OnReload()
    {
        if (activeWeapon == null) return;
        
        // activeWeapon.OnReload();
        // Debug.Log($"Reloading {activeWeapon.itemData.name}");
    }
    public void OnSpecialReload()
    {
        if (activeWeapon == null) return;

        // Debug.Log($"Special Reload {activeWeapon.itemData.name}");
    }
    public void OnSpecialShoot()
    {
        if (activeWeapon == null) return;

        // Debug.Log($"Special Shoot {activeWeapon.itemData.name}");
    }
}
