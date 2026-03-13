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
        // public event Action<int, int> OnAmmoChange;//maybe use ifneed to do something with ammo
        public Weapon WeaponData => (Weapon)itemData;
        public WeaponStat bonusStat;
        public WeaponInstance(Weapon weaponData) : base(weaponData)
        {
            OnCreate();
        }
        public WeaponInstance(Weapon weaponData, WeaponStat weaponBonus) : base(weaponData)
        {
            this.weaponData = weaponData;
            this.bonusStat = weaponBonus;
        }
        public void OnCreate()
        {
            if (itemData is Weapon weapon)
            {
                weaponData = weapon;
                bonusStat.bonusDamage =  UnityEngine.Random.Range(weaponData.randomStatsRange.minBonusDamage, weaponData.randomStatsRange.maxBonusDamage);
                bonusStat.critDamage = UnityEngine.Random.Range(weaponData.randomStatsRange.minCritDamage, weaponData.randomStatsRange.maxCritDamage);
                bonusStat.critRate = UnityEngine.Random.Range(weaponData.randomStatsRange.minCritRate, weaponData.randomStatsRange.maxCritRate);
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