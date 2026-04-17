using UnityEngine;
using Unity.Netcode;
using Pathfinding;

public abstract class BaseNPC : NetworkBehaviour
{
    [Header("Base Enemy Settings")]
    [SerializeField] public string npcName = "baseNPC";
    [SerializeField] public float maxHealth = 100f;
    [SerializeField] public float moveSpeed = 2f;
    [SerializeField] public float detectionRange = 5f;
    [SerializeField] public float attackRange = 1f;
    [SerializeField] public float attackCooldown = 1.5f;
    [SerializeField] public float contactDamage= 1f;
    [SerializeField] public bool isBoss = false;
    [SerializeField] public bool isFinalRoomEnemy = false;
    [SerializeField] public GameObject damagePopupPrefab;
    [Header("Rewards")]
    [SerializeField] public int expReward = 5;
    [SerializeField] public Weapon weaponDrop;
    [SerializeField][Range(0f, 1f)] public float weaponDropChance = 0.1f;
    [SerializeField][Range(0f, 1f)] public float relicDropChance = 0.05f;
    // [SerializeField] public RelicRarity relicDropRarity = RelicRarity.Common;

    // public RelicRarity RelicDropRarity => relicDropRarity;
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
    [Header("Visual Facing")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform spriteRoot;
    [SerializeField] private float faceDeadzone = 0.01f;
    public AIPath aiPath;

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
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        aiPath = GetComponent<AIPath>();
        if (spriteRoot == null)
        {
            spriteRoot = GetComponentInChildren<Transform>();
        }
        // Default all enemies to face left.
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
        }
        else if (spriteRoot != null)
        {
            Vector3 scale = spriteRoot.localScale;
            scale.x = Mathf.Abs(scale.x);
            spriteRoot.localScale = scale;
        }
    }

    protected virtual void Update()
    {
        if (!IsServer || IsDead) return;

        FindTarget();
        HandleBehavior();
    }

    protected virtual void LateUpdate()
    {
        UpdateFacing();
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

    private void UpdateFacing()
    {
        float vx = 0f;
        bool hasVelocity = false;

        if (rb != null)
        {
            vx = rb.linearVelocity.x;
            hasVelocity = Mathf.Abs(vx) > faceDeadzone;
        }

        if (!hasVelocity && aiPath != null)
        {
            vx = aiPath.desiredVelocity.x;
            hasVelocity = Mathf.Abs(vx) > faceDeadzone;
        }

        if (!hasVelocity) return;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = vx > 0f;
            return;
        }

        if (spriteRoot == null) return;

        Vector3 scale = spriteRoot.localScale;
        float sign = vx < 0f ? -1f : 1f;
        scale.x = Mathf.Abs(scale.x) * sign;
        spriteRoot.localScale = scale;
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
            MapManager.Instance.RegisterEnemyReward(this);
            if (isBoss)
            {
                MapManager.Instance.RegisterEnemyDied();
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
    public string GetNPCName()
    {
        return npcName;
    }
}
