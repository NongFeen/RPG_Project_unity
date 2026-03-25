using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.InputSystem;
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
    [SerializeField] private float reloadSpinSpeed = 1000f;
    [SerializeField] private float facingFlipDeadzone = 0.05f;
    private float reloadSpinAngle;
    private Mouse virtualMouse;
    private void Update()
    {
        if (playerEquippedItem.activeWeapon == null) return;
        if (IsOwner)
        {
            Vector2 aimPos = GetScreenAimPosition();
            Vector2 aimWorldPos = Camera.main.ScreenToWorldPoint(aimPos);
            Vector2 direction = aimWorldPos - (Vector2)weaponDisplayRoot.position;

            // Rotate weapon
            Vector2 rotatedDir = new Vector2(-direction.y, direction.x);
            bool isReloading = playerEquippedItem.activeWeapon.isReloading;
            if (isReloading)
            {
                reloadSpinAngle += reloadSpinSpeed * Time.deltaTime;
                if (reloadSpinAngle >= 360f) reloadSpinAngle -= 360f;
                Quaternion spin = Quaternion.AngleAxis(reloadSpinAngle, Vector3.forward);
                weaponDisplayRoot.up = spin * rotatedDir;
            }
            else
            {
                reloadSpinAngle = 0f;
                weaponDisplayRoot.up = rotatedDir;
            }

            bool shouldFaceRight = playerSprite.isFacingRight.Value;
            Vector2 PlayerDirection = aimWorldPos - (Vector2)playerSprite.transform.position;
            if (Mathf.Abs(PlayerDirection.x) > facingFlipDeadzone)
            {
                shouldFaceRight = PlayerDirection.x > 0f;
            }
            if (playerSprite.isFacingRight.Value != shouldFaceRight)
            {
                playerSprite.SetFacingDirection(shouldFaceRight);
            }
        }
    }

    private Vector2 GetScreenAimPosition()
    {
        if (inputReader != null && inputReader.activeGameDevice == InputReader.GameDevice.GamePad)
        {
            var mouse = GetVirtualMouse();
            if (mouse != null && mouse.added)
            {
                return mouse.position.value;
            }
        }

        return inputReader != null ? inputReader.AimPosition : Vector2.zero;
    }

    private Mouse GetVirtualMouse()
    {
        if (virtualMouse != null && virtualMouse.added)
        {
            return virtualMouse;
        }

        virtualMouse = InputSystem.GetDevice<Mouse>("VirtualMouse");
        if (virtualMouse != null)
        {
            return virtualMouse;
        }

        foreach (var device in InputSystem.devices)
        {
            if (device is Mouse mouse)
            {
                if (mouse.layout == "VirtualMouse" || mouse.displayName == "VirtualMouse" || mouse.name == "VirtualMouse")
                {
                    virtualMouse = mouse;
                    return virtualMouse;
                }
            }
        }

        return null;
    }
}
