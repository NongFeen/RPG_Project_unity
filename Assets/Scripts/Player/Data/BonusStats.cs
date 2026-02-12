using System;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct BonusStats : INetworkSerializable, IEquatable<BonusStats>
{
    [SerializeField]public int bonusHealth;
    [SerializeField]public int bonusDefense;
    [SerializeField]public float bonusCritChance;
    [SerializeField]public float bonusCritDamage;
    [SerializeField]public float bonusDamage;


    public bool Equals(BonusStats other)
    {
        return bonusHealth == other.bonusHealth &&
               bonusDefense == other.bonusDefense &&
               bonusCritChance == other.bonusCritChance &&
               bonusCritDamage == other.bonusCritDamage &&
               bonusDamage == other.bonusDamage;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref bonusHealth);
        serializer.SerializeValue(ref bonusDefense);
        serializer.SerializeValue(ref bonusCritChance);
        serializer.SerializeValue(ref bonusCritDamage);
        serializer.SerializeValue(ref bonusDamage);
    }
    public override string ToString()
    {
        return $"BonusStats(Health: {bonusHealth}, Defense: {bonusDefense}, CritChance: {bonusCritChance}, CritDamage: {bonusCritDamage}, Damage: {bonusDamage} )";
    }
}
