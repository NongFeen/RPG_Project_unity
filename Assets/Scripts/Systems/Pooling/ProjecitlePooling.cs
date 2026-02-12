using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Pool; // Requires Unity 2021+

/// <summary>
/// Manages object pooling for networked prefabs using Unity's built-in ObjectPool.
/// Must be present in the scene and added to the NetworkManager's list of NetworkPrefabs.
/// </summary>
public class NetworkObjectPool : NetworkBehaviour
{
    [Header("Hierarchy Management")]
    public static NetworkObjectPool Singleton { get; private set; }

    // Use a serializable struct to manage settings for each prefab in the Inspector
    [System.Serializable]
    public class PoolConfig
    {
        public GameObject Prefab;
        public int PrePoolAmount = 100; // Default to 100
        public int MaxPoolSize = 1000;
    }
    [SerializeField] private List<PoolConfig> poolsToCreate;
    // [SerializeField] private List<PoolConfig> prefabsToPool;
    // A dictionary to hold a pool for each different prefab type
    // [SerializeField] private Transform poolParent;
    private Dictionary<GameObject, ObjectPool<NetworkObject>> pools = new Dictionary<GameObject, ObjectPool<NetworkObject>>();
    // A map to quickly find the original prefab from a pooled instance
    private Dictionary<NetworkObject, GameObject> instanceToPrefabMap = new Dictionary<NetworkObject, GameObject>();

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
            InitializePools();
        }
    }

    private void InitializePools()
    {
        foreach (var config in poolsToCreate)
        {
            if (config.Prefab.GetComponent<NetworkObject>() == null)
            {
                Debug.LogError($"Prefab {config.Prefab.name} does not have a NetworkObject component.");
                continue;
            }

            pools[config.Prefab] = new ObjectPool<NetworkObject>(
                () => CreatePooledItem(config.Prefab), // Create func
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPooledItem,
                collectionCheck: false,
                defaultCapacity: config.PrePoolAmount, // Sets initial List capacity
                maxSize: config.MaxPoolSize
            );

            // --- ADDED: Pre-warm the pool immediately after creation ---
            PreWarmPool(config.Prefab, config.PrePoolAmount);
        }
    }

    /// <summary>
    /// Manually pre-instantiates objects to fill the pool initially.
    /// </summary>
    private void PreWarmPool(GameObject prefab, int amount)
    {
        List<NetworkObject> initialObjects = new List<NetworkObject>();
        for (int i = 0; i < amount; i++)
        {
            // Get calls CreatePooledItem(), which parents and deactivates it
            NetworkObject obj = pools[prefab].Get(); 
            initialObjects.Add(obj);
        }

        // Now release them all back into the pool so they are ready for use
        foreach (var obj in initialObjects)
        {
            pools[prefab].Release(obj);
        }
        
        Debug.Log($"Pre-warmed pool for {prefab.name} with {amount} objects.");
    }

    // --- ObjectPool Callbacks ---

    private NetworkObject CreatePooledItem(GameObject prefab)
    {
        // Instantiate the object locally
        GameObject instance = Instantiate(prefab);
        
        NetworkObject netObj = instance.GetComponent<NetworkObject>();
        // if (poolParent != null)
        // {
        //     instance.transform.SetParent(poolParent);
        // }
        // Map the new instance back to its source prefab
        instanceToPrefabMap[netObj] = prefab;
        
        return netObj;
    }

    private void OnReleaseToPool(NetworkObject netObj)
    {
        // Deactivate the object when it's returned to the pool
        netObj.gameObject.SetActive(false);
    }

    private void OnGetFromPool(NetworkObject netObj)
    {
        // Handled by the custom Instantiate method in PooledPrefabInstanceHandler
        // We ensure it's active before use.
        netObj.gameObject.SetActive(true);
    }

    private void OnDestroyPooledItem(NetworkObject netObj)
    {
        // Clean up when the pool max size is exceeded
        instanceToPrefabMap.Remove(netObj);
        Destroy(netObj.gameObject);
    }

    // --- Network Registration ---

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Only the server registers handlers that clients will use
        {
            foreach (var prefab in poolsToCreate)
            {
                // Register a custom handler for each pooled prefab
                NetworkManager.Singleton.PrefabHandler.AddHandler(
                    prefab.Prefab.GetComponent<NetworkObject>(), 
                    new PooledPrefabInstanceHandler(prefab.Prefab, this)
                );
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            foreach (var prefab in poolsToCreate)
            {
                // Unregister handlers when the pool manager despawns
                NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab.Prefab.GetComponent<NetworkObject>());
            }
        }
    }

    // --- Public Pool Methods (Used by the Handler) ---

    // Called by the PooledPrefabInstanceHandler when the NetworkManager needs a new instance
    public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (pools.TryGetValue(prefab, out ObjectPool<NetworkObject> pool))
        {
            NetworkObject obj = pool.Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.gameObject.SetActive(true); // Ensure active for spawning
            return obj;
        }
        
        Debug.LogError($"Pool for prefab {prefab.name} not found!");
        return Instantiate(prefab, position, rotation).GetComponent<NetworkObject>();
    }

    // Called by the PooledPrefabInstanceHandler when the NetworkManager destroys an instance
    public void ReturnNetworkObject(NetworkObject networkObject)
    {
        if (instanceToPrefabMap.TryGetValue(networkObject, out GameObject prefab))
        {
            if (pools.TryGetValue(prefab, out ObjectPool<NetworkObject> pool))
            {
                pool.Release(networkObject); // Puts it back in the pool and calls OnReleaseToPool
            }
        }
        else
        {
            // If the object wasn't in our map/pool, destroy it normally
            Destroy(networkObject.gameObject);
        }
    }
}