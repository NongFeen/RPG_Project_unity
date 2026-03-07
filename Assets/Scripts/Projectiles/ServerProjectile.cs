using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class ServerProjectile : NetworkBehaviour
{
    [SerializeField] public float speed = 3f;
    [SerializeField] public float lifeTime = 3f;
    [SerializeField] public int pierce = 1;
    [SerializeField] public bool canHitPlayer = false; //cannot damage player but can hit player
    [SerializeField] public bool isFriendly = true;//cannot hit player but hit enemy
    [SerializeField] public Vector2 direction;
    [SerializeField] public float damage;
    [SerializeField] public bool isCrit=false;
    [SerializeField] public Player owner;

    public float lifeTimer = 0;
    public virtual void OnSpawn(Vector2 direction, float damage,bool isCrit, float critDamageMultiplier)
    {
        this.direction = direction.normalized;
        // print($"Projectile damage {this.damage} (isCrit: {isCrit}) critMul: {critDamageMultiplier} shouldbe {damage * critDamageMultiplier}");
        this.damage = isCrit ? damage * critDamageMultiplier : damage;
        this.isCrit = isCrit;
        lifeTimer = lifeTime;
        // Debug.Log($"Projectile damage {damage} by {weaponOwner.weaponData.name}");
    }
    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var client))
        {
            owner = client.PlayerObject.GetComponent<Player>();
        }
    }
    public virtual void Update()
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
    public virtual void OnTriggerEnter2D(Collider2D collision)
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
        if (isFriendly && collision.gameObject.TryGetComponent<BaseNPC>(out var npc))
        {
            print($"{name} is hitting");
            this.OnProjectileHit(npc);
            npc.OnHit(damage,isCrit);
            pierce -= 1;
        }
        if(pierce < 1)
            DestroySelf();
    }
    public virtual void OnTriggerExit2D(Collider2D collision)
    {
    }
    // public virtual void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (!IsServer) return;
    //     // print("Hit target");
    //     if (canHitPlayer && !isFriendly && collision.gameObject.CompareTag("Player"))
    //     {
    //         collision.gameObject.TryGetComponent<PlayerStats>(out var player);
    //         player.TakeDamage(this.damage);
    //         pierce -= 1;
    //     }
    //     if (isFriendly && collision.gameObject.TryGetComponent<BaseNPC>(out var npc))
    //     {
    //         print($"{name} is hitting");
    //         OnProjectileHit(npc);
    //         npc.OnHit(damage,isCrit);
    //         pierce -= 1;
    //     }
    //     if (pierce < 1 || collision.gameObject.CompareTag("Wall"))
    //         DestroySelf();
    // }
    public virtual void DestroySelf()
    {
        if (!IsServer) return;

        if (TryGetComponent<NetworkObject>(out var netObj))
        {
            if (netObj.IsSpawned)
                netObj.Despawn();
            else
                Destroy(gameObject);
        }
    }
    public virtual void MovePosition()
    {
        // transform.position += (Vector3)(speed * Time.deltaTime * direction);
        TryGetComponent<Rigidbody2D>(out var rb);
        rb.linearVelocity = direction * speed;
    }
    public virtual void OnProjectileHit(BaseNPC npc){
        print("Hit target");
    }
}
