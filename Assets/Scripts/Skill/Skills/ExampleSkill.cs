using Unity.Netcode;
using UnityEngine;

public class ExampleSkill : SkillBehaviour
{
    //fire a projectile towards target position
    public GameObject projectilePrefab;
    [SerializeField] public int skillDamage = 30;
    public override void Initialize(PlayerStats owner,SkillDefinition def)
    {
        base.Initialize(owner,def);
        projectilePrefab = definition.projectilePrefab;
    }
    public override void ActivateSkill(Vector3 targetPos)
    {
        base.ActivateSkill(targetPos);
        if (!IsServer) return;
        Vector3 dir = (targetPos - owner.transform.position).normalized;
        SpawnProjectileServer(owner.transform.position, dir);
        base.ActivateSkill(targetPos);
    }
    public void SpawnProjectileServer(Vector3 firePointPosition, Vector2 direction)
    {
        // This check is the authoritative gate to ensure this is only done on the server.
        if (!NetworkManager.Singleton.IsServer) return;

        GameObject prefabToUse = projectilePrefab;
        prefabToUse.TryGetComponent<NetworkObject>(out NetworkObject netObjToUse);
        if (prefabToUse == null) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(netObjToUse, NetworkManager.Singleton.LocalClientId,false,false,false,firePointPosition,rot);
        GameObject proj = netObj.gameObject;

        if (proj.TryGetComponent<ServerProjectile>(out ServerProjectile serverProjectile))
        {
            // Pass the direction and damage to the projectile's logic
            serverProjectile.OnSpawn(direction, skillDamage, false, 1f);
        }
        
    }
}

