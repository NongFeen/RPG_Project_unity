using Unity.Netcode;
using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public struct NetworkWeaponData : INetworkSerializable, IEquatable<NetworkWeaponData>
{
    public int weaponId;
    public WeaponStat weaponStat;

    public NetworkWeaponData(int weaponId, int bonusDamage, float critRate, float critDamage)
    {
        this.weaponId = weaponId;
        this.weaponStat.bonusDamage = bonusDamage;
        this.weaponStat.critRate = critRate;
        this.weaponStat.critDamage = critDamage;
    }
    public bool Equals(NetworkWeaponData other)
    {
        return weaponId == other.weaponId
            && weaponStat.bonusDamage == other.weaponStat.bonusDamage
            && weaponStat.critRate == other.weaponStat.critRate
            && weaponStat.critDamage == other.weaponStat.critDamage;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref weaponId);
        serializer.SerializeValue(ref weaponStat.bonusDamage);
        serializer.SerializeValue(ref weaponStat.critRate);
        serializer.SerializeValue(ref weaponStat.critDamage);
    }
    public static NetworkWeaponData Empty() => new NetworkWeaponData(-1, 0, 0, 0);
    public override string ToString()
    {
        return $"{weaponId} : bDmg:{weaponStat.bonusDamage} CR:{weaponStat.critRate} CD:{weaponStat.critDamage} ";
    }
}
