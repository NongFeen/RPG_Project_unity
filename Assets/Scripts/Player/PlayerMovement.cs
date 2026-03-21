using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerSprite))]
public class PlayerMovement : NetworkBehaviour
{
    // === Serialized Fields ===
    [Header("Dependencies")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 6f;
    // private readonly NetworkVariable<bool> isWalking = new NetworkVariable<bool>(false);
    // private readonly NetworkVariable<bool> isFacingRight = new NetworkVariable<bool>(true);
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 externalVelocity;
    private static readonly float MIN_MOVEMENT_THRESHOLD = 0.01f; 
    // public bool IsWalking => isWalking.Value;
    // public bool IsFacingRight => isFacingRight.Value;
    public float BaseMoveSpeed => baseMoveSpeed;

    private void Awake()
    {
        if (!TryGetComponent(out rb))
        {
            Debug.LogError($"Rigidbody2D not found on {gameObject.name}.");
            enabled = false; 
        }
    }

    public override void OnNetworkSpawn()
    {
        // isFacingRight.OnValueChanged += OnFacingDirectionChanged;
        // isWalking.OnValueChanged += OnWalkingChanged;

        // init for all client
        // OnFacingDirectionChanged(false, isFacingRight.Value); 
        // OnWalkingChanged(false, isWalking.Value); 
        if (IsOwner)
        {
            inputReader.MoveEvents += HandleMovementInput;
        }
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        // isFacingRight.OnValueChanged -= OnFacingDirectionChanged;
        // isWalking.OnValueChanged -= OnWalkingChanged;
        if (IsOwner)
        {
            inputReader.MoveEvents -= HandleMovementInput;
        }

        base.OnNetworkDespawn();
    }

    private void HandleMovementInput(Vector2 moveDir)
    {
        moveInput = moveDir;
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        gameObject.TryGetComponent<PlayerStats>(out var playerStats);
        if(playerStats.IsGhost)return;

        Vector2 movementVelocity = baseMoveSpeed * moveInput;
        rb.linearVelocity = movementVelocity + externalVelocity;
        externalVelocity = Vector2.Lerp(externalVelocity, Vector2.zero, 10f * Time.fixedDeltaTime);

        //animation
        bool currentWalkingState = rb.linearVelocity.sqrMagnitude > MIN_MOVEMENT_THRESHOLD;
        if (currentWalkingState != GetComponent<PlayerSprite>().isWalking.Value)
        {
            GetComponent<PlayerSprite>().SetWalking(currentWalkingState);
        }
    }
    [ClientRpc]
    public void TeleportClientRpc(Vector3 pos)
    {
        transform.position = pos;
    }
    public void AddExternalVelocity(Vector2 force)
    {
        externalVelocity += force;
    }
    public Vector2 GetMoveInput()
    {
        return moveInput;
    }
}