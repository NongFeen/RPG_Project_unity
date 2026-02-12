using UnityEngine;

[System.Serializable]
public struct WeaponStat
{
    public int bonusDamage;
    public float critRate;
    public float critDamage;

    public override string ToString()
    {
        return $"WeaponBonusStats(Damage: {bonusDamage}, CritChance: {critRate}, CritDamage: {critDamage})";
    }
}