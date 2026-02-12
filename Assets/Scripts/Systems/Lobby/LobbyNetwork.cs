using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class LobbyNetwork : NetworkBehaviour
{
    public static LobbyNetwork Instance;
    public NetworkList<LobbyPlayerData> playersProfileData;
    public NetworkVariable<MapName> MapName = new NetworkVariable<MapName>(global::MapName.Story_01, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playersProfileData = new NetworkList<LobbyPlayerData>(null,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        playersProfileData.Add(new LobbyPlayerData
        {
            clientId = clientId,
            isReady = false,
            saveData = default
        });
        print($"Client connected: {clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        for (int i = playersProfileData.Count - 1; i >= 0; i--)
        {
            if (playersProfileData[i].clientId == clientId)
            {
                playersProfileData.RemoveAt(i);
                break;
            }
        }
    }
    
    public List<LobbyPlayerData> GetLobbyPlayerDatas()
    {
        var list = new List<LobbyPlayerData>(playersProfileData.Count);
        for (int i = 0; i < playersProfileData.Count; i++)
        {
            list.Add(playersProfileData[i]);
        }
        return list;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SubmitPlayerProfileServerRpc(
        PlayerSaveData playerSaveData,
        ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        for (int i = 0; i < playersProfileData.Count; i++)
        {
            if (playersProfileData[i].clientId == senderClientId)
            {
                var data = playersProfileData[i];
                data.saveData = playerSaveData;
                playersProfileData[i] = data;
                break;
            }
        }
        Debug.Log($"📩 Received profile from client {senderClientId}");
    }

    public void PrintAllLobbyPlayerData()
    {
        if (playersProfileData == null)
        {
            Debug.Log("Lobby players list is null");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== LOBBY PLAYER DATA =====");

        for (int i = 0; i < playersProfileData.Count; i++)
        {
            var p = playersProfileData[i];
            var s = p.saveData;

            sb.AppendLine($"Player [{i}]");
            sb.AppendLine($"  ClientId      : {p.clientId}");
            sb.AppendLine($"  Ready         : {p.isReady}");
            sb.AppendLine($"  CharacterName : {s.characterName.ToString()}");
            sb.AppendLine($"  Level         : {s.level}");
            sb.AppendLine($"  Experience    : {s.experience}");
            sb.AppendLine($"  Class         : {s.characterClass}");
            sb.AppendLine("--------------------------------");
        }

        Debug.Log(sb.ToString());
    }
    [ContextMenu("Print Lobby Player Data")]
    private void PrintLobbyPlayerData_ContextMenu()
    {
        PrintAllLobbyPlayerData();
    }
    public MapName GetCurrentMapName()
    {
        return MapName.Value;
    }
    public void HostSelectMap(MapName newMap)
    {
        if (!IsServer) return;
        MapName.Value = newMap;
    }

    public PlayerSaveData GetSaveData(ulong clientId)
    {
        for (int i = 0; i < playersProfileData.Count; i++)
        {
            if (playersProfileData[i].clientId == clientId)
            {
                return playersProfileData[i].saveData;
            }
        }

        return default;
    }

}
