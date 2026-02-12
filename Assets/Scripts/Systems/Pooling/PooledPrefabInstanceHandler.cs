using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Implements INetworkPrefabInstanceHandler to intercept NetworkManager spawn/destroy calls
/// and redirect them to the NetworkObjectPool.
/// </summary>
public class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
{
    private GameObject prefab;
    private NetworkObjectPool pool;

    public PooledPrefabInstanceHandler(GameObject prefab, NetworkObjectPool pool)
    {
        this.prefab = prefab;
        this.pool = pool;
    }

    // This method is called when a client receives a message to spawn an object
    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        // Get the object from our pool instead of using the default Instantiate
        NetworkObject networkObject = pool.GetNetworkObject(prefab, position, rotation);
        return networkObject;
    }

    // This method is called when a client receives a message to destroy an object
    public void Destroy(NetworkObject networkObject)
    {
        // Return the object to our pool instead of using the default Destroy
        pool.ReturnNetworkObject(networkObject);
    }
}