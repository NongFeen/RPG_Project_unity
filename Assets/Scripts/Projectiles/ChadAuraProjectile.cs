using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class ChadAuraProjectile : ServerProjectile
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(owner != null)
        {
            transform.SetParent(owner.transform);
            transform.localPosition = Vector3.zero;
        }
    }
    public override void OnSpawn(Vector2 direction, float damage,bool isCrit, float critDamageMultiplier)
    {
        this.direction = direction.normalized;
        // print($"Projectile damage {this.damage} (isCrit: {isCrit}) critMul: {critDamageMultiplier} shouldbe {damage * critDamageMultiplier}");
        this.damage = isCrit ? damage * critDamageMultiplier : damage;
        this.isCrit = isCrit;
        lifeTimer = lifeTime;
        transform.localPosition = Vector3.zero;
        // Debug.Log($"Projectile damage {damage} by {weaponOwner.weaponData.name}");
    }
    
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if(!IsServer)return;
        //only hit player and apply buff equal to duration
        if (canHitPlayer && isFriendly && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.TryGetComponent<PlayerStats>(out var player);
            player.AddBuffServer(BuffType.ChadAura, lifeTimer);
        }
    }
    public override void OnTriggerExit2D(Collider2D collision)
    {
        if (canHitPlayer && isFriendly && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.TryGetComponent<PlayerStats>(out var player);
            player.RemoveBuffServer(BuffType.ChadAura);
        }
    }
    public override void DestroySelf()
    {
        if (IsServer && TryGetComponent<NetworkObject>(out var netObj))
        {
            netObj.Despawn();
        }
    }
    public override void MovePosition()
    {   
        transform.position = owner.transform.position;
    }
}
