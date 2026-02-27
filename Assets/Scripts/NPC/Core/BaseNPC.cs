using UnityEngine;
using Unity.Netcode;

public abstract class BaseNPC : NetworkBehaviour
{
    [Header("Base Enemy Settings")]
    [SerializeField] protected string npcName = "baseNPC";
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected float attackCooldown = 1.5f;
    [SerializeField] protected float contactDamage= 1f;
    [SerializeField] public bool isBoss = false;
    [SerializeField] public bool isFinalRoomEnemy = false;
    [SerializeField] private GameObject damagePopupPrefab;
    [Header("NPC Current data")]
    [SerializeField] protected float displayHealth;
    [SerializeField] protected float lastAttackTime;
    [SerializeField] protected Transform target;
    [SerializeField]public NetworkVariable<float> currentHealth = new NetworkVariable<float>(
    100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] public bool IsDead => currentHealth.Value <= 0;
    [Header("Animation")]
    [SerializeField] public Animator animator;
    [SerializeField] public LayerMask obstacleLayer;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            OnSpawned();
        }
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        currentHealth.OnValueChanged += OnHealthChanged;
        displayHealth = maxHealth;
    }

    protected virtual void Awake()
    {
    }

    protected virtual void Update()
    {
        if (!IsServer || IsDead) return;

        FindTarget();
        HandleBehavior();
    }

    protected virtual void FindTarget()
    {
        // Simple range-based detection
        var players = GameObject.FindGameObjectsWithTag("Player");
        float closest = detectionRange + 1f;
        Transform closestPlayer = null;

        foreach (var p in players)
        {
            p.TryGetComponent<PlayerStats>(out PlayerStats pStats);
            if(pStats.currentHP.Value <=0 ) continue;
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < closest && dist <= detectionRange)
            {
                closest = dist;
                closestPlayer = p.transform;
            }
        }

        target = closestPlayer;
    }

    protected virtual void HandleBehavior()
    {
        if (target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist > attackRange)
            MoveTowards(target.position);
        else
            TryAttack();
    }

    protected virtual void MoveTowards(Vector3 destination)
    {
        Vector2 dir = (destination - transform.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
    }

    protected virtual void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;
        Attack();
    }

    protected abstract void Attack(); // To override in subclasses

    private void OnHealthChanged(float oldValue, float newValue)
    {
        // if(newValue >=0)
        displayHealth = newValue;
    }
    public virtual void OnHit(float damage, bool isCrit)
    {
        if (!IsServer || IsDead) return;
        currentHealth.Value -= damage;
        OnDamageTaken(damage,isCrit);

        if (currentHealth.Value <= 0)
            Die();
    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // if (!IsServer || IsDead) return;
        // print($"{name} is taking hit");
        // if (collision.CompareTag("Player"))
        // {
        //     print("hit player");
        //     // if (collision.TryGetComponent<PlayerHealth>(out var health))
        //     // {
        //     //     health.TakeDamage(contactDamage);
        //     // }
        // }
    }
    protected virtual void OnSpawned() { }
    protected virtual void OnDamageTaken(float damage, bool isCrit)
    {
        // print($"{name} taking damage for {damage}");
        ShowDamagePopupClientRpc(damage, isCrit);
    }
    protected virtual void Die()
    {
        print($"{name} is dead");
        OnDeath();
        // SpawnDeathEffectClientRpc();

        if (!IsServer) return;
        if(MapManager.Instance != null)
        {
            if (isBoss)
            {
                MapManager.Instance.OnBossDefeated();
            }
            else
            {
                MapManager.Instance.RegisterEnemyDied();
            }
            if(isFinalRoomEnemy)
            {
                MapManager.Instance.RegisterFinalRoomEnemyDied();
            }
        }
        NetworkObject.Despawn();
    }

    protected virtual void OnDeath()
    {
    }
    // protected virtual void SpawnDeathEffectClientRpc()
    // {
    //     // Destroy(gameObject);
    //     Debug.Log("💥This NPC DEAD");
    // }// [ClientRpc]
    [ClientRpc]
    private void ShowDamagePopupClientRpc(float damage, bool isCrit)
    {
        if (damagePopupPrefab == null) return;

        // Spawn slightly above NPC head
        Vector3 pos = transform.position + new Vector3(0, 1.2f, 0);

        GameObject popup = Instantiate(damagePopupPrefab, pos, Quaternion.identity);

        // var text = popup.GetComponentInChildren<TMPro.TextMeshPro>();
        popup.TryGetComponent<TMPro.TextMeshPro>(out var text);
        if (text != null)
        {
            text.text = damage.ToString("0");
            if (isCrit)
                text.color = Color.yellow;
        }
    }
    // protected bool IsTargetInRange(float range)
    // {
    //     if (currentTarget == null) return false;
    //     return Vector2.Distance(transform.position, currentTarget.position) <= range;
    // }
    public virtual bool HasLineOfSight(Transform target)
    {
        if (!target) return false;

        Vector2 origin = GetComponent<Collider2D>().bounds.center;

        Collider2D col = target.GetComponent<Collider2D>();
        if (col == null) return false;

        Vector2 targetCenter = col.bounds.center;

        Vector2 direction = (targetCenter - origin).normalized;
        float distance = Vector2.Distance(origin, targetCenter);

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            distance,
            obstacleLayer
        );

        return hit.collider == null;
    }
}
