using Unity.Netcode;
using UnityEngine;
using Pathfinding;

public class Proton_NPC : BaseNPC
{

    [Header("Idle Roaming")]
    [SerializeField] private float roamRadius = 6f;
    [SerializeField] private float roamInterval = 3f;
    [SerializeField] private float idleSpeedMultiplier = 0.6f;

    [Header("Dash Settings")]
    [SerializeField] private float aimDuration = 1f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashMaxDuration = 5f;
    private Vector2 dashTargetPosition;
    [SerializeField] private float decelerationRate = 15f;
    private AIPath aiPath;

    private float stateTimer;
    private float nextRoamTime;

    private Vector2 dashDirection;


    public enum ProtonState
    {
        Idle,
        Chase,
        Aim,
        Dash,
        Recover
    }

    public NetworkVariable<ProtonState> npcState =
        new(ProtonState.Idle,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    protected override void Awake()
    {
        base.Awake();
        aiPath = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
            SetState(ProtonState.Idle);
    }

    protected override void Update()
    {
        if (!IsServer || IsDead) return;

        stateTimer += Time.deltaTime;

        FindTarget();

        switch (npcState.Value)
        {
            case ProtonState.Idle:
                UpdateIdle();
                break;

            case ProtonState.Chase:
                UpdateChase();
                break;

            case ProtonState.Aim:
                UpdateAim();
                break;

            case ProtonState.Dash:
                UpdateDash();
                break;

            case ProtonState.Recover:
                UpdateRecover();
                break;
        }

        if(target != null && npcState.Value == ProtonState.Dash)
        {
            float dist = Vector2.Distance(transform.position, target.position);
            if (dist < attackRange)
            {
                TryAttack();
            }
        }
    }

    void SetState(ProtonState newState)
    {
        npcState.Value = newState;
        stateTimer = 0f;
    }

    void UpdateIdle()
    {
        aiPath.canMove = true;
        aiPath.maxSpeed = moveSpeed * idleSpeedMultiplier;

        if (target != null)
        {
            if (HasLineOfSight(target))
                SetState(ProtonState.Aim);
            else
                SetState(ProtonState.Chase);

            return;
        }

        if (Time.time >= nextRoamTime || aiPath.reachedEndOfPath)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 roamPoint = transform.position + (Vector3)(randomDir * roamRadius);

            aiPath.destination = roamPoint;
            aiPath.SearchPath();

            nextRoamTime = Time.time + roamInterval;
        }
    }
    void UpdateChase()
    {
        if (target == null)
        {
            SetState(ProtonState.Idle);
            return;
        }

        aiPath.canMove = true;
        aiPath.maxSpeed = moveSpeed;
        aiPath.destination = target.position;

        // If LOS found → stop and charge
        if (HasLineOfSight(target))
        {
            aiPath.canMove = false;
            rb.linearVelocity = Vector2.zero;
            SetState(ProtonState.Aim);
        }
    }
    void UpdateAim()
    {
        if (target == null)
        {
            SetState(ProtonState.Idle);
            return;
        }

        aiPath.canMove = false;
        rb.linearVelocity = Vector2.zero;

        Vector2 toTarget = target.position - transform.position;
        dashDirection = toTarget.normalized;    
        dashTargetPosition = target.position;
        if (stateTimer >= aimDuration)
        {
            SetState(ProtonState.Dash);
        }
    }

    void UpdateDash()
    {
        rb.linearVelocity = dashDirection * dashSpeed;

        Vector2 toTarget = dashTargetPosition - (Vector2)transform.position;

        if (Vector2.Dot(toTarget, dashDirection) <= 0f)
        {
            // We passed the point → start slowing down
            SetState(ProtonState.Recover);
        }
        if(stateTimer >= dashMaxDuration)
        {
            SetState(ProtonState.Idle);
        }
    }

    void UpdateRecover()
    {
        // Smooth deceleration
        rb.linearVelocity = Vector2.Lerp(
            rb.linearVelocity,
            Vector2.zero,
            decelerationRate * Time.deltaTime
        );

        if (rb.linearVelocity.magnitude < 0.1f)
        {
            rb.linearVelocity = Vector2.zero;

            if (target != null && HasLineOfSight(target))
                SetState(ProtonState.Aim);
            else if(target != null)
                SetState(ProtonState.Chase);
            else
                SetState(ProtonState.Idle);
        }
    }

    protected override void FindTarget()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        Transform closest = null;
        float closestDist = detectionRange;

        foreach (var p in players)
        {
            if (!p.TryGetComponent<PlayerStats>(out var stats))
                continue;

            if (stats.currentHP.Value <= 0)
                continue;

            float dist = Vector2.Distance(transform.position, p.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = p.transform;
            }
        }

        target = closest;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;
        
        if (npcState.Value == ProtonState.Dash)
        {
            if (collision.gameObject.CompareTag("Wall")&& !HasLineOfSight(target))
            {
                print("hit wall");
                SetState(ProtonState.Recover);
            }
        }
        
    }
    public override bool HasLineOfSight(Transform target)
    {
        if (!target) return false;

        Collider2D targetCol = target.GetComponent<Collider2D>();
        Collider2D myCol = GetComponent<Collider2D>();

        if (!targetCol || !myCol) return false;

        Vector2 origin = myCol.bounds.center;

        Bounds tb = targetCol.bounds;

        // 3 points: bottom, center, top
        Vector2 bottom = new Vector2(tb.center.x, tb.min.y);
        Vector2 center = tb.center;
        Vector2 top = new Vector2(tb.center.x, tb.max.y);

        return
            IsClear(origin, bottom) &&
            IsClear(origin, center) &&
            IsClear(origin, top);
    }

    bool IsClear(Vector2 from, Vector2 to)
    {
        Vector2 direction = (to - from).normalized;
        float distance = Vector2.Distance(from, to);

        RaycastHit2D hit = Physics2D.Raycast(
            from,
            direction,
            distance,
            obstacleLayer
        );

        return hit.collider == null;
    }
    protected override void Attack()
    {
        if (target == null) return;

        if (target.TryGetComponent<PlayerStats>(out var player))
        {
            player.TakeDamage(contactDamage);
        }
    }
}