using System.Collections.Generic;
using Pathfinding;
using Unity.Netcode;
using UnityEngine;

public class MapManager : NetworkBehaviour
{
    public static MapManager Instance;
    [SerializeField] private bool mapHasBoss = false;
    [SerializeField] private HashSet<int> triggeredTiles = new HashSet<int>();
    [SerializeField] private NetworkVariable<bool> bossSpawned = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    [SerializeField] private NetworkVariable<NetworkObjectReference> bossRef = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    [SerializeField] private NetworkVariable<int> aliveEnemyCount = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    [SerializeField]
    private NetworkVariable<MapState> currenState = new(
        MapState.Progress,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    [SerializeField] private NetworkVariable<int> finalRoomEnemyCount = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] public MapItemDrop mapItemDrop;
    [SerializeField] public int mapExperienceReward = 20;


    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        mapItemDrop = GameDatabase.Instance.GetMapDatabase().GetMapData(GameManager.Instance.selectMapName).mapItemDrop;
        AstarPath.active.Scan();
    }
    public void PlayerTriggeredTile(List<int> id)
    {
        if (IsServer)
        {
            print("Server Handling Tile Trigger");
            foreach (var tileId in id)
            {
                print("Handling Tile ID: " + tileId);
                HandleTileTrigger(tileId);
            }
        }
    }
    [ServerRpc]
    private void PlayerTriggeredTileServerRpc(int id)
    {
        HandleTileTrigger(id);
    }
    private void HandleTileTrigger(int id)
    {
        //normal spawn
        if(id < 1000)
        {
            SpawnNPCManager.Instance.SpawnAtPoint(id.ToString(),false);
        }
        else //spawn as boss of map 
        {
            //spawn as boss
            // no way we need 999 spawn point per map
            TrySpawnBoss(id);
        }
    }
    private void TrySpawnBoss(int id)
    {
        if (bossSpawned.Value)
        {
            Debug.Log("Boss already spawned");
            return;
        }

        NetworkObject bossNetObj =
            SpawnNPCManager.Instance.SpawnBossAtPoint(id.ToString());

        if (bossNetObj == null)
        {
            Debug.LogError("Boss spawn failed");
            return;
        }

        bossRef.Value = bossNetObj;
        bossSpawned.Value = true;
    }
    public bool TryGetBoss(out BaseNPC boss)
    {
        boss = null;

        if (!bossRef.Value.TryGet(out NetworkObject netObj))
            return false;

        return netObj.TryGetComponent(out boss);
    }
    public void RegisterEnemySpawned()
    {
        if (!IsServer) return;
        aliveEnemyCount.Value++;
    }
    public void RegisterBossSpawned()
    {
        if (!IsServer) return;
        aliveEnemyCount.Value++;
        bossSpawned.Value = true;
        currenState.Value = MapState.BossFight;
    }
    public void RegisterEnemyDied()
    {
        if (!IsServer) return;

        aliveEnemyCount.Value--;

        CheckMapCompletion();
    }
    public virtual void CheckMapCompletion()
    {
        if (!IsServer) return;

        if (mapHasBoss)
            return; // boss decides completion

        if (aliveEnemyCount.Value <= 0)
        {
            CompleteMap();
        }
    }
    public void CompleteMap()
    {
        if (currenState.Value == MapState.Completed)
        return;

        currenState.Value = MapState.Completed;

        Debug.Log("Map Completed!");
        List<WeaponInstance> droppedItems = GenerateItemDrop();
        GameManager.Instance.OnGameComplete(mapExperienceReward, droppedItems);
    }

    public List<WeaponInstance> GenerateItemDrop()
    {
        //generate dropped items
        List<Item> droppedItems = mapItemDrop.GetDroppedItems();
        List<WeaponInstance> itemInstances = new List<WeaponInstance>();
        if (droppedItems != null)
        {
            foreach (var item in droppedItems)
            {
                if(item is Weapon)
                {
                    print("Dropped Item: " + item.itemName);
                    itemInstances.Add(WeaponInstance.CreateWeaponInstance(item, 1));
                }
            }
        }
        return itemInstances;
    }

    public void OnBossDefeated()
    {
        if (!IsServer) return;
        Debug.Log("Boss defeated!");
        //only server send boss complete to clients
        NotifyMapCompletedClientRpc();
    }
    [ClientRpc]
    public void NotifyMapCompletedClientRpc()
    {
        Debug.Log("Map Completed! (Client RPC)");
        CompleteMap();
    }
    public void RegisterFinalRoomEnemySpawned()
    {
        if (!IsServer) return;
        finalRoomEnemyCount.Value++;
        currenState.Value = MapState.FinalRoom;
    }
    public void RegisterFinalRoomEnemyDied()
    {
        if (!IsServer) return;
        finalRoomEnemyCount.Value--;

        if (finalRoomEnemyCount.Value <= 0)
        {
           CompleteMap();
        }
    }
    public void EnterFinalRoom()
    {
        if (!IsServer) return;
        currenState.Value = MapState.FinalRoom;
    }
    public void PlayerTriggeredExtractTile()
    {
        CompleteMap();
    }
    public virtual void CheckFriendshipCondition(int value)
    {
        
    }
}
