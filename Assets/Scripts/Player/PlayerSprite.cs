using UnityEngine;
using Unity.Netcode;
using UnityEngine.U2D.Animation;
using System;
using System.Collections.Generic;
public class PlayerSprite : NetworkBehaviour
{
    [Serializable]
    public struct ClassSpriteLibrary
    {
        public ClassType classType;
        public SpriteLibraryAsset spriteLibraryAsset;
    }

    [SerializeField] private Animator animator;
    [SerializeField] public NetworkVariable<bool> isWalking = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    [SerializeField] public NetworkVariable<bool> isFacingRight = new NetworkVariable<bool>(true,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    [SerializeField] public NetworkVariable<Quaternion> weaponRotation = new NetworkVariable<Quaternion>(Quaternion.identity,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    [SerializeField] public SpriteLibrary spriteLibrary;
    [SerializeField] public Transform weaponHolder;
    [SerializeField] private SpriteLibraryAsset defaultSpriteLibraryAsset;
    [SerializeField] private List<ClassSpriteLibrary> classSpriteLibraries = new List<ClassSpriteLibrary>();
    private PlayerStats playerStats;
    public bool IsWalking => isWalking.Value;
    public bool IsFacingRight => isFacingRight.Value;

    void Start()
    {
        isWalking.OnValueChanged += OnWalkingChanged;
        isFacingRight.OnValueChanged += OnFacingDirectionChanged;
    }
    public override void OnNetworkSpawn()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats not found on PlayerSprite owner.");
            return;
        }

        playerStats.playerClass.OnValueChanged += OnClassChanged;
        ApplyClassSpriteLibrary(playerStats.playerClass.Value);
    }
    public override void OnNetworkDespawn()
    {
        isWalking.OnValueChanged -= OnWalkingChanged;
        isFacingRight.OnValueChanged -= OnFacingDirectionChanged;
        if (playerStats != null)
        {
            playerStats.playerClass.OnValueChanged -= OnClassChanged;
        }
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

    private void OnClassChanged(ClassType oldClass, ClassType newClass)
    {
        ApplyClassSpriteLibrary(newClass);
    }

    private void ApplyClassSpriteLibrary(ClassType classType)
    {
        if (spriteLibrary == null)
        {
            Debug.LogWarning("SpriteLibrary reference is missing on PlayerSprite.");
            return;
        }

        SpriteLibraryAsset targetAsset = null;
        for (int i = 0; i < classSpriteLibraries.Count; i++)
        {
            if (classSpriteLibraries[i].classType == classType)
            {
                targetAsset = classSpriteLibraries[i].spriteLibraryAsset;
                break;
            }
        }

        if (targetAsset == null)
        {
            targetAsset = defaultSpriteLibraryAsset;
        }

        if (targetAsset == null)
        {
            Debug.LogWarning($"No SpriteLibraryAsset configured for class {classType} and no default assigned.");
            return;
        }

        spriteLibrary.spriteLibraryAsset = targetAsset;
    }
}
