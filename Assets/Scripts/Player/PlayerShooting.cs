using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] public Transform weaponPos;
    [SerializeField] private PlayerEquipedItem playerEquipedItem;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerAiming playerAiming;
    private bool isFiring;
    private Mouse virtualMouse;
    public void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }
    private void Start()
    {
        if (!IsOwner) return;
        inputReader.PrimaryFireEvents += HandlePrimaryFire;
        inputReader.ReloadWeaponEvents += HandleReload;
    }
    public override void OnDestroy()
    {
        if (!IsOwner) return;
        inputReader.PrimaryFireEvents -= HandlePrimaryFire;
        inputReader.ReloadWeaponEvents -= HandleReload;

        base.OnDestroy();
    }
    private void HandlePrimaryFire(bool isPressed)
    {
        if (!IsOwner) return;
        if (playerStats != null && playerStats.IsGhost) return;
        isFiring = isPressed;
    }
    private void HandleReload(bool isPressed)
    {
        if (!IsOwner) return;
        if (playerStats != null && playerStats.IsGhost) return;
        playerEquipedItem.activeWeapon?.OnReload();
    }
    private void Update()
    {
        if (!IsOwner) return;
        if (playerStats != null && playerStats.IsGhost) return;
        if (isFiring )
        {
            WeaponBehaviour weapon = playerEquipedItem.activeWeapon;
            if( weapon == null) return;
            if (weapon != null && weapon.CanShoot())
            {
                ShootWeapon(weapon, playerAiming.aimDirection.Value);
            }
            else
            {
                if(!weapon.CanShoot() && weapon.currentAmmo == 0)
                {
                    HandleReload(true);
                }
                else return; //no weapon equip
            }
        }
    }
    private void ShootWeapon(WeaponBehaviour weapon, Vector2 dir, ServerRpcParams rpcParams = default)
    {
        // print("ShootWeapon");
        weapon.OnShoot(dir);
        ShootWeaponServerRPC(dir, rpcParams);
    }
    [ServerRpc]
    private void ShootWeaponServerRPC(Vector2 direction,ServerRpcParams rpcParams = default)
    {
        WeaponBehaviour weapon = playerEquipedItem.activeWeapon;
        weapon.Shoot(direction,weaponPos, playerStats,rpcParams);
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
    public Vector2 GetAimDirection()
    {
        return playerAiming.aimDirection.Value;
    }
}   
