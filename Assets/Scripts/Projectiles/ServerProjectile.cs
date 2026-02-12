using Unity.Netcode;
using UnityEngine;

public class ServerProjectile : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int pierce = 1;
    [SerializeField] private bool canHitPlayer = false;
    [SerializeField] private Vector2 direction;
    [SerializeField] private float damage;
    [SerializeField] private bool isCrit=false;
    public void OnSpawn(Vector2 direction, float damage,bool isCrit, float critDamageMultiplier)
    {
        this.direction = direction.normalized;
        print($"Projectile damage {this.damage} (isCrit: {isCrit}) critMul: {critDamageMultiplier} shouldbe {damage * critDamageMultiplier}");
        this.damage = isCrit ? damage * critDamageMultiplier : damage;
        this.isCrit = isCrit;
        // Debug.Log($"Projectile damage {damage} by {weaponOwner.weaponData.name}");
    }
    void Update()
    {
        if (IsServer)
        {
            MovePosition();
        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!canHitPlayer) return;
            pierce -= 1;

        }
        if (collision.gameObject.TryGetComponent<BaseNPC>(out var npc))
        {
            print($"{name} is hitting");
            this.OnProjectileHit(npc);
            npc.OnHit(damage,isCrit);
            pierce -= 1;
        }
        if (pierce < 1 || collision.gameObject.CompareTag("Wall"))
            DestroySelf();
    }
    private void DestroySelf()
    {
        if (IsServer && TryGetComponent<NetworkObject>(out var netObj))
        {
            netObj.Despawn();
        }
    }
    private void MovePosition()
    {
        transform.position += (Vector3)(speed * Time.deltaTime * direction);
    }
    private void OnProjectileHit(BaseNPC npc){
        
    }
}
