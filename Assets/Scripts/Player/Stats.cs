using System;
using Unity.Netcode;
using UnityEngine;
[System.Serializable]
public struct Stats : INetworkSerializable, IEquatable<Stats>
{
    public int health;
    public int defense;
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
    public override string ToString()
    {
        return $"Health: {health}, Defense: {defense}, CritRate: {critRate}, CritDamage: {critDamage}, ExtraDamage: {extraDamage}";
    }
}
