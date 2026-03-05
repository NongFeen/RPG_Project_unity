using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class BoltChargeProjectile : ServerProjectile
{
    [Header("Bolt Charge Settings")]
    [SerializeField] private float chainRadius = 10f;
    [SerializeField] private float chainDamageMultiplier = 0.5f;
    [SerializeField] private LayerMask npcLayerMask;
    public override void OnSpawn(Vector2 direction, float damage,bool isCrit, float critDamageMultiplier)
    {
        this.direction = direction.normalized;
        // print($"Projectile damage {this.damage} (isCrit: {isCrit}) critMul: {critDamageMultiplier} shouldbe {damage * critDamageMultiplier}");
        this.damage = isCrit ? damage * critDamageMultiplier : damage;
        this.isCrit = isCrit;
        lifeTimer = lifeTime;
    }
    public override void Update()
    {
        if (IsServer)
        {
            MovePosition();
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                DestroySelf();
            }
        }
    }
    public override void OnProjectileHit(BaseNPC npc)
    {
        base.OnProjectileHit(npc);
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, chainRadius,npcLayerMask);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == npc.gameObject) continue;
            hitCollider.TryGetComponent(out BaseNPC nearbyNPC);
            if (nearbyNPC != null)
            {
                print($"{name} is chaining to {nearbyNPC.name}");
                nearbyNPC.OnHit(damage * chainDamageMultiplier, isCrit);
            }
        }
    }
}
