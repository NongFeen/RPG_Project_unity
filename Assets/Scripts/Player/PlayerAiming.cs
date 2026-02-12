using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
[RequireComponent(typeof(PlayerSprite))]
public class PlayerAiming : NetworkBehaviour
{
    // [SerializeField] private Transform weaponDisplayRoot;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform weaponDisplayRoot;
    [SerializeField] private List<Transform> weaponTransformList;
    [SerializeField] private PlayerEquipedItem playerEquippedItem;
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private GameObject weaponHolderPrefab;
    [SerializeField] private PlayerSprite playerSprite;
    private void Update()
    {
        if (playerEquippedItem.activeWeapon == null) return;
        if (IsOwner)
        {
            Vector2 aimPos = inputReader.AimPosition;
            Vector2 aimWorldPos = Camera.main.ScreenToWorldPoint(aimPos);
            Vector2 direction = aimWorldPos - (Vector2)weaponDisplayRoot.position;

            // Rotate weapon
            Vector2 rotatedDir = new Vector2(-direction.y, direction.x);
            weaponDisplayRoot.up = rotatedDir;

            // Handle sprite facing direction
            bool shouldFaceRight = direction.x >= 0f;
            if (playerSprite.isFacingRight.Value != shouldFaceRight)
            {
                playerSprite.SetFacingDirection(shouldFaceRight);
            }
        }
    }
}
