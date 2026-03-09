using UnityEngine;

[System.Serializable]
public struct RelicStat
{
    public int bonusDamage;
    public float critRate;
    public float critDamage;
    public float bonusHealth;

    public override string ToString()
    {
        return $"RelicBonusStats(Damage: {bonusDamage}, CritChance: {critRate}, CritDamage: {critDamage}, Health: {bonusHealth})";
    }
}