using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class ExploadsiveProjectile : ServerProjectile
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
            MovePosition();
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
        if(collision.gameObject.CompareTag("Wall"))
        {
            DestroySelf();
        }
        if (canHitPlayer && !isFriendly && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.TryGetComponent<PlayerStats>(out var player);
            player.TakeDamage(this.damage);
            pierce -= 1;
        }
        // print("Player Take Damage"+ this.damage);
        if (isFriendly && collision.gameObject.TryGetComponent<BaseNPC>(out var npc))
        {
            print($"{name} is hitting");
            this.OnProjectileHit(npc);
            npc.OnHit(damage,isCrit);
            pierce -= 1;
        }
    }
    public override void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;
        // print("Hit target");
        if (canHitPlayer && !isFriendly && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.TryGetComponent<PlayerStats>(out var player);
            player.TakeDamage(this.damage);
            pierce -= 1;
        }
        if (isFriendly && collision.gameObject.TryGetComponent<BaseNPC>(out var npc))
        {
            print($"{name} is hitting");
            this.OnProjectileHit(npc);
            npc.OnHit(damage,isCrit);
            pierce -= 1;
        }
        if (pierce < 1 || collision.gameObject.CompareTag("Wall"))
            DestroySelf();
    }
    public override void DestroySelf()
    {
        if (IsServer && TryGetComponent<NetworkObject>(out var netObj))
        {
            netObj.Despawn();
        }
    }
}
