using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class WellOfBlessingProjectile : ServerProjectile
{
    public override void OnSpawn(Vector2 direction, float damage,bool isCrit, float critDamageMultiplier)
    {
        this.direction = direction.normalized;
        // print($"Projectile damage {this.damage} (isCrit: {isCrit}) critMul: {critDamageMultiplier} shouldbe {damage * critDamageMultiplier}");
        this.damage = isCrit ? damage * critDamageMultiplier : damage;
        this.isCrit = isCrit;
        lifeTimer = lifeTime;
        // Debug.Log($"Projectile damage {damage} by {weaponOwner.weaponData.name}");
    }
    public override void Update()
    {
        if (IsServer)
        {
            // MovePosition();
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                DestroySelf();
            }
        }
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if(!IsServer)return;
        //only hit player and apply buff equal to duration
        if (canHitPlayer && isFriendly && collision.gameObject.CompareTag("Player"))
        {   
            collision.gameObject.TryGetComponent<PlayerStats>(out var player);
            player.AddBuffServer(BuffType.WellOfBlessing, lifeTimer);
        }
    }
    public override void OnTriggerExit2D(Collider2D collision)
    {
        if (canHitPlayer && isFriendly && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.TryGetComponent<PlayerStats>(out var player);
            player.RemoveBuffServer(BuffType.WellOfBlessing);
        }
    }
    public override void DestroySelf()
    {
        if (IsServer && TryGetComponent<NetworkObject>(out var netObj))
        {
            netObj.Despawn();
        }
    }
}
