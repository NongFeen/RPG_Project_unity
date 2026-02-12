using Unity.Netcode;
using Unity.Collections;
using System;
[System.Serializable]
public struct PlayerSaveData : INetworkSerializable,IEquatable<PlayerSaveData>
{
    public FixedString32Bytes characterName;
    public int level;
    public int experience;
    public ClassType characterClass;
    public int upgradePoints;
    public BonusStats bonusStats;
    public bool Equals(PlayerSaveData other)
    {
        return characterName == other.characterName
            && level == other.level
            && experience == other.experience
            && characterClass == other.characterClass
            && upgradePoints == other.upgradePoints
            && bonusStats.Equals(other.bonusStats);
    }


    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref characterName);
        serializer.SerializeValue(ref level);
        serializer.SerializeValue(ref experience);
        serializer.SerializeValue(ref characterClass);
        serializer.SerializeValue(ref upgradePoints);
        serializer.SerializeValue(ref bonusStats);
    }
}
