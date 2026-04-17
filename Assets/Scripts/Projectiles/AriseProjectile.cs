using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class AriseProjectile : ServerProjectile
{
    [Header("Arise Projectile Config")]
    [SerializeField] private float maxRadius = 5f;
    [SerializeField] private float radiusSpeed = 20f; 
    [SerializeField] private float rotationSpeed = 90f; 
    
    private Vector2 ownerSpawnPos;
    private bool reachedMaxRadius = false;
    private float currentAngle = 0f;

    public override void OnSpawn(Vector2 direction, float damage, bool isCrit, float critDamageMultiplier)
    {
        base.OnSpawn(direction, damage, isCrit, critDamageMultiplier);
        
        // Spawn at owner position
        if (owner != null)
        {
            ownerSpawnPos = owner.transform.position;
            transform.position = ownerSpawnPos;
        }
        
        reachedMaxRadius = false;
        currentAngle = 0f;
    }

    public override void MovePosition()
    {
        if (owner == null) return;

        Vector2 ownerPos = owner.transform.position;
        float distToOwner = Vector2.Distance(transform.position, ownerPos);

        if (!reachedMaxRadius)
        {
            // move towards max radius
            if (distToOwner < maxRadius)
            {
                Vector2 direction = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
                rb.linearVelocity = direction * radiusSpeed;
            }
            else
            {
                reachedMaxRadius = true;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            currentAngle -= rotationSpeed * Time.deltaTime;
            float radians = currentAngle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * maxRadius;
            transform.position = ownerPos + offset;
        }
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if(!IsServer)return;
        //ignore wall
        // if(collision.gameObject.CompareTag("Wall"))
        // {
        //     DestroySelf();
        // }
        if (canHitPlayer && !isFriendly && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.TryGetComponent<PlayerStats>(out var player);
            player.TakeDamage(this.damage);
            //no pierce cap
            // pierce -= 1;
        }
        if (isFriendly && collision.gameObject.TryGetComponent<BaseNPC>(out var npc))
        {
            // print($"{name} is hitting");
            this.OnProjectileHit(npc);
            //no pierce cap
            // pierce -= 1;
        }
        // if(pierce < 1)
        //     DestroySelf();
    }
}
