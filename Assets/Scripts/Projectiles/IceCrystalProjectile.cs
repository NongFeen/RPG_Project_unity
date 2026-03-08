using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class IceCrystalProjectile : ServerProjectile
{
    [SerializeField] GameObject IceShardPrefab;
    private const int shardCount = 8;
    public override void OnProjectileHit(BaseNPC npc)
    {
        //spread in to ice shards
        for (int i = 0; i < shardCount; i++)
        {
            float angle = i * (360f / shardCount);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            SpawnShard(direction);
        }
        base.OnProjectileHit(npc);
    }
    private void SpawnShard(Vector2 direction)
    {
        // This check is the authoritative gate to ensure this is only done on the server.
        if (!NetworkManager.Singleton.IsServer) return;
        ulong senderId = this.NetworkObject.OwnerClientId;
        GameObject prefabToUse = IceShardPrefab;
        prefabToUse.TryGetComponent<NetworkObject>(out NetworkObject netObjToUse);
        if (prefabToUse == null) return;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(netObjToUse, senderId,false,false,false,this.transform.position,rot);
        GameObject proj = netObj.gameObject;
        
        if (proj.TryGetComponent<ServerProjectile>(out ServerProjectile serverProjectile))
        {
            // Pass the direction and damage to the projectile's logic
            serverProjectile.OnSpawn(direction, this.damage/shardCount, isCrit, 1f);// 1f because same damage as icecrystal
        }
    }
}
