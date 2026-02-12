using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions.Must;
[System.Serializable]
public class WeaponInstance : ItemInstance
{
    [NonSerialized] public Weapon weaponData;
    [NonSerialized] public int maxAmmo;
    [NonSerialized] public float fireRate;
    public event Action<int, int> OnAmmoChange;
    public Weapon WeaponData => (Weapon)itemData;
    public WeaponStat bonusStat;
    public WeaponInstance(Weapon weaponData) : base(weaponData)
    {
        OnCreate();
    }
    public void OnCreate()
    {
        if (itemData is Weapon weapon)
        {
            weaponData = weapon;
            bonusStat.bonusDamage =  UnityEngine.Random.Range(0, 30);
            bonusStat.critDamage = UnityEngine.Random.Range(0, 0.3f);
            bonusStat.critRate = UnityEngine.Random.Range(0f, 0.15f);
            this.maxAmmo = weaponData.baseMaxammo;
            this.fireRate = weaponData.baseFirerate;
        }
    }

    public override string ToString()
    {
        if (weaponData == null) return $"null";
        return $"{weaponData.name} bDmg:{bonusStat.bonusDamage} CR:{bonusStat.critRate} CD:{bonusStat.critDamage}";
    }
    public static WeaponInstance CreateWeaponInstance(Item itemData, int stackCount = 1)
    {
        if (itemData is Weapon weapon)
            return new WeaponInstance(weapon);
        else return null;
    }
}