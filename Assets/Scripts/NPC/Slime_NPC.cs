using UnityEngine;
using Unity.Netcode;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
public class SlimeEnemy : BaseNPC
{

    [Header("Combat")]
    [SerializeField] float contactDamage = 10f;
    [SerializeField] float knockbackForce = 6f;
    [SerializeField] float recoverTime = 1f;

    [Header("Jump")]
    [SerializeField] float minJumpInterval = 3f;
    [SerializeField] float maxJumpInterval = 5f;

    [Header("Roaming")]
    [SerializeField] float roamingRange = 3f;
    [SerializeField] float roamInterval = 3f;

    [Header("Chase Speed")]
    [SerializeField] float slowSpeed = 2f;
    [SerializeField] float fastSpeed = 5f;
    [SerializeField] float baseSpeed = 3f;
    [SerializeField] float burstTime = 1f;
    [SerializeField] float decayTime = 3f;
    [SerializeField] float speedSmooth = 4f;
    [SerializeField] float burstInterval = 2.5f;

    [Header("Randomize")]
    [SerializeField] float speedVariance = 0.4f;
    [SerializeField] float timeVariance = 0.3f;


    // ===============================
    // STATE
    // ===============================

    public enum SlimeState
    {
        Idle,
        Chase,
        Jump,
        Recover
    }

    public NetworkVariable<SlimeState> State = new(
        SlimeState.Idle,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);


    // ===============================
    // COMPONENTS
    // ===============================

    AIPath aiPath;
    AIDestinationSetter destSetter;
    Rigidbody2D rb;


    // ===============================
    // TIMERS
    // ===============================

    float nextRoamTime;
    float nextJumpTime;
    float recoverEndTime;

    float burstTimer;
    float decayTimer;
    float nextBurstTime;
    float targetSpeed;


    // ===============================
    // RANDOM
    // ===============================

    float speedMul = 1f;
    float timeMul = 1f;


    // ===============================
    // INIT
    // ===============================

    protected override void Awake()
    {
        base.Awake();

        aiPath = GetComponent<AIPath>();
        destSetter = GetComponent<AIDestinationSetter>();
        rb = GetComponent<Rigidbody2D>();

        targetSpeed = baseSpeed;
        aiPath.maxSpeed = baseSpeed;
        aiPath.canMove = false;
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        if (IsServer)
        {
            ResetJumpTimer();
            Randomize();
        }

        State.OnValueChanged += OnStateChanged;
    }

    protected override void OnDeath()
    {
        State.OnValueChanged -= OnStateChanged;
        base.OnDeath();
    }


    // ===============================
    // MAIN UPDATE
    // ===============================

    protected override void HandleBehavior()
    {
        if (!IsServer || IsDead) return;

        UpdateState();
        UpdateSpeed();
    }


    // ===============================
    // STATE MACHINE
    // ===============================

    void UpdateState()
    {
        switch (State.Value)
        {
            case SlimeState.Idle:
                UpdateIdle();
                break;

            case SlimeState.Chase:
                UpdateChase();
                break;

            case SlimeState.Jump:
                // Waiting animation
                break;

            case SlimeState.Recover:
                UpdateRecover();
                break;
        }
    }


    void SetState(SlimeState newState)
    {
        if (State.Value == newState) return;

        State.Value = newState;

        switch (newState)
        {
            case SlimeState.Idle:
                aiPath.canMove = true;
                destSetter.enabled = false;
                break;

            case SlimeState.Chase:
                aiPath.canMove = true;
                destSetter.enabled = true;
                StartBurst();
                nextBurstTime = Time.time + burstInterval * timeMul;
                break;

            case SlimeState.Jump:
                aiPath.canMove = false;
                break;

            case SlimeState.Recover:
                aiPath.canMove = false;
                recoverEndTime = Time.time + recoverTime;
                break;
        }
    }


    // ===============================
    // IDLE
    // ===============================

    void UpdateIdle()
    {
        if (target != null &&
            Vector2.Distance(transform.position, target.position) <= detectionRange)
        {
            SetState(SlimeState.Chase);
            return;
        }

        // Roam
        if (Time.time >= nextRoamTime)
        {
            nextRoamTime = Time.time + roamInterval;

            Vector2 pos =
                (Vector2)transform.position +
                Random.insideUnitCircle * roamingRange;

            aiPath.destination = pos;
        }

        // Jump
        if (Time.time >= nextJumpTime)
        {
            SetState(SlimeState.Jump);
        }
    }


    // ===============================
    // CHASE
    // ===============================

    void UpdateChase()
    {
        if (target == null)
        {
            SetState(SlimeState.Idle);
            return;
        }

        float dist = Vector2.Distance(transform.position, target.position);

        if (dist > detectionRange)
        {
            SetState(SlimeState.Idle);
            return;
        }

        if (dist <= attackRange)
        {
            TryAttack();
        }

        destSetter.target = target;

        if (Time.time >= nextBurstTime)
        {
            StartBurst();
            nextBurstTime = Time.time + burstInterval * timeMul;
        }
        UpdateBurst();
    }


    // ===============================
    // SPEED BURST
    // ===============================

    void StartBurst()
    {
        burstTimer = burstTime * timeMul;
        decayTimer = decayTime * timeMul;

        targetSpeed = slowSpeed * speedMul;
    }

    void UpdateBurst()
    {
        if (burstTimer > 0f)
        {
            burstTimer -= Time.deltaTime;

            float t = 1f - (burstTimer / (burstTime * timeMul));

            targetSpeed = Mathf.Lerp(
                slowSpeed * speedMul,
                fastSpeed * speedMul,
                t
            );

            return;
        }

        if (decayTimer > 0f)
        {
            decayTimer -= Time.deltaTime;

            float t = 1f - (decayTimer / (decayTime * timeMul));

            targetSpeed = Mathf.Lerp(
                fastSpeed * speedMul,
                baseSpeed * speedMul,
                t
            );

            return;
        }

        targetSpeed = baseSpeed * speedMul;
    }


    // ===============================
    // RECOVER
    // ===============================

    void UpdateRecover()
    {
        if (Time.time >= recoverEndTime)
        {
            SetState(SlimeState.Idle);
        }
    }


    // ===============================
    // SPEED SMOOTH
    // ===============================

    void UpdateSpeed()
    {
        aiPath.maxSpeed = Mathf.Lerp(
            aiPath.maxSpeed,
            targetSpeed,
            Time.deltaTime * speedSmooth
        );
    }


    // ===============================
    // ATTACK
    // ===============================

    protected override void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;

        Attack();
    }

    protected override void Attack()
    {
        if (target == null) return;
        if (State.Value == SlimeState.Recover) return;

        if (target.TryGetComponent<PlayerStats>(out var player))
        {
            player.TakeDamage(contactDamage);
        }

        Vector2 dir =
            (transform.position - target.position).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

        SetState(SlimeState.Recover);
    }


    // ===============================
    // JUMP EVENT
    // ===============================

    public void OnJumpAnimationEnd()
    {
        if (!IsServer || IsDead) return;

        if (State.Value == SlimeState.Jump)
        {
            ResetJumpTimer();
            SetState(SlimeState.Idle);
        }
    }


    void ResetJumpTimer()
    {
        nextJumpTime =
            Time.time +
            Random.Range(minJumpInterval, maxJumpInterval);
    }


    // ===============================
    // RANDOMIZE
    // ===============================

    void Randomize()
    {
        speedMul = Random.Range(1f - speedVariance, 1f + speedVariance);
        timeMul = Random.Range(1f - timeVariance, 1f + timeVariance);
    }


    // ===============================
    // ANIM
    // ===============================

    void OnStateChanged(SlimeState oldS, SlimeState newS)
    {
        if (!animator) return;

        animator.SetBool("isJump", newS == SlimeState.Jump);
    }


    // ===============================
    // COLLISION
    // ===============================

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsServer || IsDead) return;

        if (col.transform == target)
        {
            Attack();
        }
    }
}
