using UnityEngine;
using Unity.Netcode;
public class PlayerSprite : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] public NetworkVariable<bool> isWalking = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    [SerializeField] public NetworkVariable<bool> isFacingRight = new NetworkVariable<bool>(true,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    [SerializeField] public NetworkVariable<Quaternion> weaponRotation = new NetworkVariable<Quaternion>(Quaternion.identity,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    [SerializeField] public Transform weaponHolder;
    public bool IsWalking => isWalking.Value;
    public bool IsFacingRight => isFacingRight.Value;
    [SerializeField] public bool flag;

    void Start()
    {
        isWalking.OnValueChanged += OnWalkingChanged;
        isFacingRight.OnValueChanged += OnFacingDirectionChanged;
    }
    public override void OnNetworkDespawn()
    {
        isWalking.OnValueChanged -= OnWalkingChanged;
        isFacingRight.OnValueChanged -= OnFacingDirectionChanged;
    }
    public void SetWalking(bool walking)
    {
        if (IsServer || IsOwner)
        {
            isWalking.Value = walking;
        }
    }
    public void SetFacingDirection(bool facingRight)
    {
        if (IsServer || IsOwner)
        {
            isFacingRight.Value = facingRight;
        }
    }
    private void OnWalkingChanged(bool oldVal, bool newVal)
    {
        // print($"Walking changed from {oldVal} to {newVal}");
        animator.SetBool("isWalking", newVal);
    }
    private void OnFacingDirectionChanged(bool oldVal, bool newVal)
    {
        // print($"Facing direction changed from {oldVal} to {newVal}");
        //true = right | false = left
        // Vector3 newScale = transform.localScale;

        //flip player sprite 
        Vector3 newScale = animator.transform.localScale;
        newScale.x = newVal ? Mathf.Abs(newScale.x) : -Mathf.Abs(newScale.x);
        animator.transform.localScale = newScale;

        //flip weaponholder pos
        Vector3 newWeaponHolderPos = weaponHolder.localPosition;
        newWeaponHolderPos.x = newVal ? Mathf.Abs(newWeaponHolderPos.x) : -Mathf.Abs(newWeaponHolderPos.x);
        weaponHolder.localPosition = newWeaponHolderPos;
        
        Vector3 newWeaponHolderScale = weaponHolder.localScale;
        newWeaponHolderScale.y = newVal ? Mathf.Abs(newWeaponHolderScale.y) : -Mathf.Abs(newWeaponHolderScale.y);
        weaponHolder.localScale = newWeaponHolderScale;
    }
}
