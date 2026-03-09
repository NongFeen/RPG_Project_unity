using System;
using Unity.Netcode;
using UnityEngine;
[System.Serializable]
public struct Stats : INetworkSerializable, IEquatable<Stats>
{
    public float health;
    public float defense;
    public float critRate;
    public float critDamage;
    public float extraDamage;


    public bool Equals(Stats other)
    {
        return health == other.health &&
               defense == other.defense &&
               critRate == other.critRate &&
               critDamage == other.critDamage &&
               extraDamage == other.extraDamage;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref health);
        serializer.SerializeValue(ref defense);
        serializer.SerializeValue(ref critRate);
        serializer.SerializeValue(ref critDamage);
        serializer.SerializeValue(ref extraDamage);
    }
    public static Stats operator +(Stats a, Stats b)
    {
        return new Stats
        {
            health = a.health + b.health,
            defense = a.defense + b.defense,
            critRate = a.critRate + b.critRate,
            critDamage = a.critDamage + b.critDamage,
            extraDamage = a.extraDamage + b.extraDamage
        };
    }
    public static Stats operator +(Stats a, BonusStats b)
    {
        return new Stats
        {
            health = a.health + b.bonusHealth,
            defense = a.defense + b.bonusDefense,
            critRate = a.critRate + b.bonusCritChance,
            critDamage = a.critDamage + b.bonusCritDamage,
            extraDamage = a.extraDamage + b.bonusDamage
        };
    }
    public override string ToString()
    {
        return $"Health: {health}, Defense: {defense}, CritRate: {critRate}, CritDamage: {critDamage}, ExtraDamage: {extraDamage}";
    }
}
