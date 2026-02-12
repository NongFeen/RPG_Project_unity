using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
public class SlimeEnemyAstar : BaseNPC
{
    [Header("Combat")]
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float knockbackForce = 6f;

    [Header("Jump")]
    [SerializeField] private float minJumpInterval = 3f;
    [SerializeField] private float maxJumpInterval = 5f;

    private float nextJumpTime;
    private float jumpTimer;

    private AIPath aiPath;
    private AIDestinationSetter destinationSetter;

    public NetworkVariable<SlimeState> slimeState = new(
        SlimeState.idle,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public enum SlimeState
    {
        idle,
        chasing,
        jumping,
        recovering
    }

    protected override void Awake()
    {
        base.Awake();

        aiPath = GetComponent<AIPath>();
        destinationSetter = GetComponent<AIDestinationSetter>();

        aiPath.maxSpeed = moveSpeed;
        aiPath.canMove = false;
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        if (IsServer)
            SetNextJumpTime();

        slimeState.OnValueChanged += OnSlimeStateChanged;
    }

    protected override void OnDeath()
    {
        slimeState.OnValueChanged -= OnSlimeStateChanged;
        base.OnDeath();
    }

    protected override void Update()
    {
        if (!IsServer || IsDead) return;

        FindTarget();

        jumpTimer += Time.deltaTime;

        HandleState();
        HandleIdleJump();
    }

    // =========================================
    // STATE MACHINE
    // =========================================

    private void HandleState()
    {
        if (slimeState.Value == SlimeState.jumping ||
            slimeState.Value == SlimeState.recovering)
        {
            aiPath.canMove = false;
            return;
        }

        if (target == null)
        {
            SetState(SlimeState.idle);
            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            aiPath.canMove = false;
            TryAttack();
        }
        else if (distance <= detectionRange)
        {
            SetState(SlimeState.chasing);

            aiPath.canMove = true;
            destinationSetter.target = target;
        }
        else
        {
            SetState(SlimeState.idle);

            aiPath.canMove = false;
            destinationSetter.target = null;
        }
    }

    private void SetState(SlimeState state)
    {
        if (slimeState.Value != state)
            slimeState.Value = state;
    }

    // =========================================
    // IDLE JUMP
    // =========================================

    private void HandleIdleJump()
    {
        if (slimeState.Value != SlimeState.idle)
            return;

        if (jumpTimer >= nextJumpTime)
        {
            jumpTimer = 0f;
            SetNextJumpTime();

            StartCoroutine(JumpRoutine());
        }
    }

    private void SetNextJumpTime()
    {
        nextJumpTime = Random.Range(minJumpInterval, maxJumpInterval);
    }

    private IEnumerator JumpRoutine()
    {
        SetState(SlimeState.jumping);

        aiPath.canMove = false;

        yield return new WaitForSeconds(0.2f);

        SetState(SlimeState.idle);
    }

    // =========================================
    // ATTACK
    // =========================================

    protected override void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;

        Attack();
    }

    protected override void Attack()
    {
        if (target == null) return;
        if (slimeState.Value == SlimeState.recovering) return;

        SetState(SlimeState.jumping);

        // Damage
        if (target.TryGetComponent<PlayerStats>(out var player))
        {
            player.TakeDamage(contactDamage);
        }

        // Knockback
        Vector2 dir =
            (transform.position - target.position).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

        StartCoroutine(RecoverRoutine());
    }

    private IEnumerator RecoverRoutine()
    {
        SetState(SlimeState.recovering);

        aiPath.canMove = false;

        yield return new WaitForSeconds(1f);

        SetState(SlimeState.idle);
    }

    // =========================================
    // ANIMATION
    // =========================================

    private void OnSlimeStateChanged(SlimeState oldState, SlimeState newState)
    {
        if (!animator) return;

        switch (newState)
        {
            case SlimeState.idle:
                animator.SetBool("isJump", false);
                break;

            case SlimeState.chasing:
                animator.SetBool("isJump", false);
                break;

            case SlimeState.jumping:
                animator.SetBool("isJump", true);
                break;

            case SlimeState.recovering:
                animator.SetBool("isJump", false);
                break;
        }
    }

    // =========================================
    // COLLISION DAMAGE
    // =========================================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer || IsDead) return;

        if (collision.transform == target)
        {
            Attack();
        }
    }
}
