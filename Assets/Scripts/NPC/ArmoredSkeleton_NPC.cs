using Unity.Netcode;
using UnityEngine;
using Pathfinding;

public class ArmoredSkeleton_NPC : BaseNPC
{
    [Header("Dash Settings")]
    [SerializeField] private float baseAimDuration = 1f;
    [SerializeField] private float baseDashSpeed = 12f;
    [SerializeField] private float minDashDuration = 0.2f;
    [SerializeField] private float maxDashDuration = 2.0f;
    [SerializeField] private float dashDuration = 1f;
    [SerializeField] private float recoverDuration = 1f;
    [SerializeField] private LineRenderer aimLine;

    private AIPath aiPath;
    private float stateTimer;
    private float dashTimer;
    private float nextDashTime;

    private float currentAimDuration;
    private float currentDashSpeed;
    private float minDashInterval;
    private float maxDashInterval;

    private Vector2 dashDirection;

    public enum SkeletonState
    {
        Chase,
        Aim,
        Dash,
        Recover
    }

    public NetworkVariable<SkeletonState> npcState =
        new(SkeletonState.Chase,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    protected override void Awake()
    {
        base.Awake();
        aiPath = GetComponent<AIPath>();
        aiPath.maxSpeed = moveSpeed;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        npcState.OnValueChanged += OnStateChanged;
        OnStateChanged(npcState.Value, npcState.Value);

        ScheduleNextDash();
    }

    public override void OnNetworkDespawn()
    {
        npcState.OnValueChanged -= OnStateChanged;
        base.OnNetworkDespawn();
    }

    protected override void Update()
    {
        if (!IsServer || IsDead) return;

        stateTimer += Time.deltaTime;
        dashTimer += Time.deltaTime;

        UpdateAggressionByHP();
        FindTarget();

        switch (npcState.Value)
        {
            case SkeletonState.Chase:
                UpdateChase();
                break;

            case SkeletonState.Aim:
                UpdateAim();
                break;

            case SkeletonState.Dash:
                UpdateDash();
                break;

            case SkeletonState.Recover:
                UpdateRecover();
                break;
        }
    }

    void SetState(SkeletonState newState)
    {
        npcState.Value = newState;
        stateTimer = 0f;
    }

    //phase 123 and enrage
    void UpdateAggressionByHP()
    {
        float hpPercent = currentHealth.Value / maxHealth;

        if (hpPercent <= 0.1f)
        {
            currentAimDuration = 0.4f;
            currentDashSpeed = 20f;
            minDashInterval = 1f;
            maxDashInterval = 2f;
        }
        else if (hpPercent <= 0.33f)
        {
            currentAimDuration = 0.6f;
            currentDashSpeed = 16f;
            minDashInterval = 2f;
            maxDashInterval = 4f;
        }
        else if (hpPercent <= 0.5f)
        {
            currentAimDuration = 0.8f;
            currentDashSpeed = 14f;
            minDashInterval = 3f;
            maxDashInterval = 5f;
        }
        else
        {
            currentAimDuration = baseAimDuration;
            currentDashSpeed = baseDashSpeed;
            minDashInterval = 4f;
            maxDashInterval = 6f;
        }
    }

    void ScheduleNextDash()
    {
        dashTimer = 0f;
        nextDashTime = Random.Range(minDashInterval, maxDashInterval);
    }

    void UpdateChase()
    {
        if (target == null)
        {
            aiPath.canMove = false;
            return;
        }

        aiPath.canMove = true;
        aiPath.destination = target.position;

        if (dashTimer >= nextDashTime)
        {
            aiPath.canMove = false;
            SetState(SkeletonState.Aim);
        }
    }

    void UpdateAim()
    {
        if (target == null)
        {
            SetState(SkeletonState.Chase);
            return;
        }

        // Vector2 toTarget = target.position - transform.position;
        target.TryGetComponent<Rigidbody2D>(out var targetRd2D);
        Vector2 predictedPos = (Vector2)target.position + targetRd2D.linearVelocity * 0.5f;//

        Vector2 toTarget = predictedPos - (Vector2)transform.position;
        dashDirection = toTarget.normalized;

        if (stateTimer >= currentAimDuration)
        {
            float distance = toTarget.magnitude;

            // Duration = distance / speed
            float computedDuration = distance / currentDashSpeed;

            dashDuration = Mathf.Clamp(
                computedDuration,
                minDashDuration,
                maxDashDuration
            );

            SetState(SkeletonState.Dash);
        }
    }

    void UpdateDash()
    {
        rb.linearVelocity = dashDirection * currentDashSpeed;

        if (stateTimer >= dashDuration)
        {
            rb.linearVelocity = Vector2.zero;
            SetState(SkeletonState.Recover);
        }
    }

    void UpdateRecover()
    {
        if (stateTimer >= recoverDuration)
        {
            ScheduleNextDash();
            SetState(SkeletonState.Chase);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (npcState.Value == SkeletonState.Dash &&
            collision.gameObject.TryGetComponent<PlayerStats>(out var player))
        {
            player.TakeDamage(contactDamage * 3f);
        }
    }

    protected override void Attack()
    {
        if (target == null) return;
        if (target.TryGetComponent<PlayerStats>(out var player))
        {
            player.TakeDamage(contactDamage);
        }
    }

    void OnStateChanged(SkeletonState oldState, SkeletonState newState)
    {
        if (aimLine == null) return;

        aimLine.enabled = newState == SkeletonState.Aim;
    }

    private void LateUpdate()
    {
        if (!aimLine || npcState.Value != SkeletonState.Aim)
            return;

        aimLine.SetPosition(0, transform.position);
        aimLine.SetPosition(1, transform.position + (Vector3)dashDirection * 8f);
    }
}
