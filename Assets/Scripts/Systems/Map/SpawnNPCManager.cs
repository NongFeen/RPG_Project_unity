using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class SpawnPointData
{
    public string id; 
    public GameObject enemyPrefab;
    public Transform spawnPoint;        // single position of this spawn point
    public int spawnCount = 1;
    public bool spawnOnStart = false;
    public bool isSpawned = false;      // tracks if already spawned
    public bool isLastRoom = false;
    public bool isRepeat = false;
    [SerializeField] public float repeatTimer = 5;//in sec
    public float currentTimer = 0f;
}

public class SpawnNPCManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;

    [Header("Spawn Point Configurations")]
    [SerializeField] private SpawnPointData[] spawnPointConfigs;
    private Dictionary<string, SpawnPointData> spawnPointDict = new();
    public static SpawnNPCManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        foreach (var sp in spawnPointConfigs)
        {
            if (sp != null && !spawnPointDict.ContainsKey(sp.id))
                spawnPointDict.Add(sp.id, sp);
        }

        // if (inputReader != null)
        //     inputReader.SpawnEnemyEvents += OnSpawnEnemyInput;
    }

    public override void OnDestroy()
    {
        // if (inputReader != null)
        //     inputReader.SpawnEnemyEvents -= OnSpawnEnemyInput;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (var sp in spawnPointConfigs)
        {
            if (sp.spawnOnStart && !sp.isSpawned)
                SpawnAtPoint(sp.id,false);
        }
    }

    void Update()
    {
        if (!IsServer) return;
        foreach (var sp in spawnPointConfigs)
        {
            if (!sp.isRepeat)
                continue;

            sp.currentTimer += Time.deltaTime;

            if (sp.currentTimer >= sp.repeatTimer && sp.isSpawned)
            {
                sp.currentTimer = 0f;
                SpawnAtPoint(sp.id,true);
            }
        }
    }

    public void SpawnAtPoint(string id,bool isForce)
    {
        if (!IsServer)
            return;

        if (!spawnPointDict.TryGetValue(id, out var sp))
        {
            Debug.LogWarning($"[Spawner] SpawnPoint ID '{id}' not found!");
            return;
        }

        if (sp.enemyPrefab == null)
        {
            Debug.LogWarning($"[Spawner] SpawnPoint '{id}' has no prefab assigned!");
            return;
        }

        if (sp.isSpawned && isForce == false)
        {
            Debug.Log($"[Spawner] SpawnPoint '{id}' already spawned.");
            return;
        }

        // Spawn enemies
        for (int i = 0; i < sp.spawnCount; i++)
        {
            Vector2 pos = sp.spawnPoint.position;
            pos.x += Random.Range(-2f, 2f);
            pos.y += Random.Range(-2f, 2f);

            GameObject obj = Instantiate(sp.enemyPrefab, pos, Quaternion.identity);

            if (obj.TryGetComponent(out NetworkObject netObj))
                netObj.Spawn(true);
            else
            {
                Debug.LogError($"[Spawner] Prefab {sp.enemyPrefab.name} has no NetworkObject!");
                Destroy(obj);
            }
            if(sp.isLastRoom == true)
            {
                obj.TryGetComponent(out BaseNPC baseNPC);
                baseNPC.isFinalRoomEnemy = true;
                MapManager.Instance.RegisterFinalRoomEnemySpawned();
            }
            MapManager.Instance.RegisterEnemySpawned();
        }

        sp.isSpawned = true; // mark as spawned
    }
    public NetworkObject SpawnBossAtPoint(string id)
    {
        if (!IsServer)
            return null;

        if (!spawnPointDict.TryGetValue(id, out var sp))
        {
            Debug.LogWarning($"Boss SpawnPoint '{id}' not found!");
            return null;
        }

        GameObject obj = Instantiate(
            sp.enemyPrefab,
            sp.spawnPoint.position,
            Quaternion.identity
        );

        if (!obj.TryGetComponent(out NetworkObject netObj))
        {
            Debug.LogError("Boss prefab missing NetworkObject!");
            Destroy(obj);
            return null;
        }

        netObj.Spawn(true);
        sp.isSpawned = true;
        MapManager.Instance.RegisterBossSpawned();
        return netObj;
    }
    
}
