using UnityEngine;
using Unity.Netcode;
public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform playerPos;
    [SerializeField] private PlayerEquipedItem playerEquipedItem;
    [SerializeField] private PlayerStats playerStats;
    private bool isFiring;
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
        isFiring = isPressed;
    }
    private void HandleReload(bool isPressed)
    {
        if (!IsOwner) return;
        playerEquipedItem.activeWeapon?.OnReload();
    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (isFiring)
        {
            Vector2 dir = AimDirection();
            WeaponBehaviour weapon = playerEquipedItem.activeWeapon;
            if (weapon != null && weapon.CanShoot())
            {
                ShootWeapon(weapon, dir);
            }
            else
            {
                if(!weapon.CanShoot() && weapon.currentAmmo ==  0)
                HandleReload(true);
            }
        }
    }
    private void ShootWeapon(WeaponBehaviour weapon, Vector2 dir, ServerRpcParams rpcParams = default)
    {
        print("ShootWeapon");
        weapon.OnShoot(dir);
        ShootWeaponServerRPC(dir, rpcParams);
    }
    [ServerRpc]
    private void ShootWeaponServerRPC(Vector2 direction,ServerRpcParams rpcParams = default)
    {
        WeaponBehaviour weapon = playerEquipedItem.activeWeapon;
        weapon.Shoot(direction, playerStats,rpcParams);
    }
    public Vector2 AimDirection()
    {
        // Get mouse position in world space
        // Vector3 mosPos = inputReader.AimPosition;
        Vector3 mosPos = Camera.main.ScreenToWorldPoint(inputReader.AimPosition);
        // Get direction from player to mouse
        Vector2 dir = mosPos - playerPos.position;

        return dir.normalized;
    }
}   
