using System;
using System.Collections.Generic;
using Pathfinding;
using Unity.Netcode;
using UnityEngine;

public class MapManager : NetworkBehaviour
{
    [Serializable]
    public struct RelicRollChance
    {
        public RelicRarity relicRarity;
        public float weight;
    }
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
    [SerializeField] public NetworkVariable<int> aliveEnemyCount = new(
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
    [Header("Reset On All Dead")]
    [SerializeField] private float allDeadResetDelay = 5f;
    private bool resetPending;
    private float resetTimer;
    [Header("Drops")]
    [SerializeField] public MapItemDrop mapItemDrop;
    [SerializeField] public int mapExperienceReward = 20;
    [SerializeField] public List<RelicRollChance> relicRollChance;
    
    private int pendingExperienceReward = 0;
    private List<WeaponInstance> pendingWeaponDrops = new List<WeaponInstance>();
    private List<RelicInstance> pendingRelicDrops = new List<RelicInstance>();



    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        mapItemDrop = GameDatabase.Instance.GetMapDatabase().GetMapData(GameManager.Instance.selectMapName).mapItemDrop;
        AstarPath.active.Scan();
    }
    private void Update()
    {
        if (!IsServer) return;
        if (currenState.Value == MapState.Completed) return;
        CheckAllPlayersDeadAndReset();
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
    public virtual void CompleteMap()
    {
        if (!IsServer) return;
        if (currenState.Value == MapState.Completed) return;

        currenState.Value = MapState.Completed;

        Debug.Log("Map Completed!");
        int totalExperienceReward = mapExperienceReward + pendingExperienceReward;
        List<WeaponInstance> droppedItems = GenerateItemDrop();
        droppedItems.AddRange(pendingWeaponDrops);
        List<RelicInstance> relicDrops = new List<RelicInstance>(pendingRelicDrops);
        GameManager.Instance.OnGameComplete(totalExperienceReward, droppedItems, relicDrops);
        ClearPendingRewards();


        NotifyMapCompletedClientRpc();
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

    public virtual void OnBossDefeated()
    {
        if (!IsServer) return;
        Debug.Log("Boss defeated!");
        CompleteMap();
    }
    [ClientRpc]
    public void NotifyMapCompletedClientRpc()
    {
        if (IsServer) return; // host already handled completion
        CompleteMapClient();
    }
    private void CompleteMapClient()
    {
        if (currenState.Value == MapState.Completed) return;
        currenState.Value = MapState.Completed;

        Debug.Log("Map Completed! (Client)");
        int totalExperienceReward = mapExperienceReward + pendingExperienceReward;
        List<WeaponInstance> droppedItems = GenerateItemDrop();
        droppedItems.AddRange(pendingWeaponDrops);
        List<RelicInstance> relicDrops = new List<RelicInstance>(pendingRelicDrops);
        GameManager.Instance.OnGameComplete(totalExperienceReward, droppedItems, relicDrops);
        ClearPendingRewards();
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
    public void RegisterEnemyReward(BaseNPC npc)
    {
        if (!IsServer || npc == null) return;

        // add exp
        pendingExperienceReward += npc.expReward;

        //weapon drop
        bool weaponDropped = false;
        int weaponItemId = -1;
        if (npc.weaponDrop != null && npc.weaponDropChance > 0f)
        {
            float roll = UnityEngine.Random.Range(0f, 1f);
            if (roll <= npc.weaponDropChance)
            {
                weaponDropped = true;
                weaponItemId = npc.weaponDrop.id;
                pendingWeaponDrops.Add(WeaponInstance.CreateWeaponInstance(npc.weaponDrop, 1));
            }
        }
        //relic drop
        bool relicDropped = false;
        
        RelicRarity relicRarity = RollRelicRarity(relicRollChance);

        if (npc.relicDropChance > 0f)
        {
            float roll = UnityEngine.Random.Range(0f, 1f);
            if (roll <= npc.relicDropChance)
            {
                relicDropped = true;
                pendingRelicDrops.Add(RelicGenerator.GenerateRelic(relicRarity));
            }
        }
        // let player know if it drop all not(1 drop from enemy = everyone get)
        NotifyEnemyRewardClientRpc(npc.expReward, weaponDropped, weaponItemId, relicDropped, relicRarity);
    }
    public RelicRarity RollRelicRarity(List<RelicRollChance> chances)
    {
        float totalWeight = 0f;

        foreach (var c in chances)
        {
            totalWeight += c.weight;
        }

        float roll = UnityEngine.Random.Range(0f, totalWeight);

        float cumulative = 0f;

        foreach (var c in chances)
        {
            cumulative += c.weight;

            if (roll <= cumulative)
            {
                return c.relicRarity;
            }
        }

        return RelicRarity.Common;
    }
    [ClientRpc]
    private void NotifyEnemyRewardClientRpc(int expReward, bool weaponDropped, int weaponItemId, bool relicDropped, RelicRarity relicRarity)
    {
        if (IsServer) return;

        pendingExperienceReward += Mathf.Max(0, expReward);

        if (weaponDropped && weaponItemId >= 0)
        {
            if (GameDatabase.Instance != null)
            {
                Weapon weapon = GameDatabase.Instance.GetItemDatabase().GetItemByID(weaponItemId) as Weapon;
                if (weapon != null)
                    pendingWeaponDrops.Add(WeaponInstance.CreateWeaponInstance(weapon, 1));
            }
        }

        if (relicDropped)
            pendingRelicDrops.Add(RelicGenerator.GenerateRelic(relicRarity));
    }
    private void ClearPendingRewards()
    {
        pendingExperienceReward = 0;
        pendingWeaponDrops.Clear();
        pendingRelicDrops.Clear();
    }
    private void CheckAllPlayersDeadAndReset()
    {
        if (PlayerManager.Instance == null) return;
        if (PlayerManager.Instance.Players.Count == 0) return;

        bool allDead = true;
        foreach (var player in PlayerManager.Instance.Players)
        {
            if (player == null) continue;
            if (!player.IsGhost)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            if (!resetPending)
            {
                resetPending = true;
                resetTimer = allDeadResetDelay;
            }
            else
            {
                resetTimer -= Time.deltaTime;
                if (resetTimer <= 0f)
                {
                    resetPending = false;
                    ResetMap();
                }
            }
        }
        else if (resetPending)
        {
            resetPending = false;
        }
    }
    private void ResetMap()
    {
        if (!IsServer) return;
        LoadingScreenManager.Instance.LoadScene(GameManager.Instance.selectMapName.ToString());
    }
}
