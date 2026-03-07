    using UnityEngine;
    using Unity.Netcode;
using System.Collections;
using System;
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
        }
    }
    private void ShootWeapon(WeaponBehaviour weapon, Vector2 dir)
    {
        weapon.OnShoot(playerPos,dir);
        Stats stats = playerStats.activeStats.Value;

        float critChance =
            stats.critRate + weapon.bonusStat.critRate;

        float critDamage =
            stats.critDamage + weapon.bonusStat.critDamage;

        float extraDamage =
            stats.extraDamage;

        weapon.OnShoot(playerPos, dir);

        ShootProjectileServerRpc(
            playerPos.position,
            dir,
            critChance,
            critDamage,
            extraDamage
        );
        // ShootProjectileServerRpc(playerPos.position, dir, weapon.bonusStat.critRate, weapon.bonusStat.critDamage);
    }
    [ServerRpc]
    private void ShootProjectileServerRpc(Vector3 firePointPosition, Vector2 dir,float critChance, float critDamageMultiplier, float extraDamage)
    {
        WeaponBehaviour weapon = playerEquipedItem.activeWeapon;
        bool isCrit = false;
        float rollvalue = UnityEngine.Random.Range(0f, 1f);
        // print($"Roll {rollvalue:F2} < CritChance {critChance:F2}");
        if (rollvalue < critChance)
            isCrit = true;
        weapon.SpawnProjectileServer(firePointPosition, dir, isCrit, critDamageMultiplier, extraDamage);
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
