using Unity.Netcode;
using Unity.Collections;

[System.Serializable]
public struct LobbyPlayerData : INetworkSerializable, System.IEquatable<LobbyPlayerData>
{
    public ulong clientId;                 // session identity
    public bool isReady;                   // lobby state
    public PlayerSaveData saveData;        // embedded persistent data

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref isReady);
        serializer.SerializeValue(ref saveData);
    }

    public bool Equals(LobbyPlayerData other)
    {
        return clientId == other.clientId;
    }
}