using System;
using Unity.Netcode;
using UnityEngine;

public class WeaponBehaviour : NetworkBehaviour, IWeapon
{
    public WeaponInstance weaponInstance;
    [SerializeField] private GameObject visualRoot;
    // public int bonusDamage;
    // public int critRate;
    // public int critDamage;
    public int currentAmmo;
    public int maxAmmo;
    public float fireRate;
    public event Action<int, int> OnAmmoChange;
    private float lastShootTime = 0;
    // public bool isReloading = false;
    [SerializeField] public bool isReloading = false;
    private float reloadTime;
    private float reloadTimer;
    public WeaponStat bonusStat;
    public void SetDefault(WeaponInstance weapon)
    {
        this.weaponInstance = weapon;
        this.bonusStat = weapon.bonusStat;
        this.maxAmmo = weapon.maxAmmo;
        this.currentAmmo = maxAmmo;
        this.fireRate = weapon.fireRate;
        visualRoot.TryGetComponent<SpriteRenderer>(out var sprite);
        sprite.sprite = weapon.weaponData.image;
    }
    void Update()
    {
        if (isReloading)
        {
            float progress = reloadTimer / reloadTime;

            // if (progress >= 0.7f)
            // {
                print($"{progress} -> {reloadTimer}/{reloadTime} " );
            // }
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= reloadTime)
                FinishReload();
        }
    }
    public void OnDrawWeapon()
    {
        Show();
    }
    public void OnReload()
    {
        print("try Reload");
        if (isReloading || currentAmmo == maxAmmo) return;
        // if(IsOwner)
        isReloading = true;
        reloadTime = weaponInstance.weaponData.baseReloadSpeed;
        reloadTimer = 0f;
        
    }
    private void FinishReload()
    {
        isReloading = false;
        int oldValue = currentAmmo;
        currentAmmo = maxAmmo;
        OnAmmoChange?.Invoke(oldValue, currentAmmo);
    }
    public void ConsumeAmmo()
    {
        // print("use ammo");
        int oldValue = currentAmmo;
        currentAmmo = Mathf.Clamp(currentAmmo - 1, 0, maxAmmo);
        OnAmmoChange?.Invoke(oldValue, currentAmmo);
    }
    public void OnShoot(Transform firePoint, Vector2 direction)
    {
        if (!CanShoot()) return;
        lastShootTime = Time.time;
        if (weaponInstance == null) return;
        if (weaponInstance.weaponData.serverProjectilePrefab != null && firePoint != null)
        {
            ConsumeAmmo();
        }
    }
    public void SpawnProjectileServer(Vector3 firePointPosition, Vector2 direction, bool isCrit, float critDamageMultiplier, float extraDamage, ServerRpcParams rpcParams = default)
    {
        // This check is the authoritative gate to ensure this is only done on the server.
        if (!NetworkManager.Singleton.IsServer) return;
        ulong senderId = rpcParams.Receive.SenderClientId;
        GameObject prefabToUse = weaponInstance.weaponData.serverProjectilePrefab;
        prefabToUse.TryGetComponent<NetworkObject>(out NetworkObject netObjToUse);
        if (prefabToUse == null) return;
        
        // Calculate rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        // Instead of UnityEngine.Object.Instantiate(), use the NetworkManager's instantiation method.
        // The NetworkManager automatically uses the custom pool handler we registered
        // in the NetworkObjectPool script's OnNetworkSpawn() method.
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(netObjToUse, senderId,false,false,false,firePointPosition,rot);
        GameObject proj = netObj.gameObject;
        
        if (proj.TryGetComponent<ServerProjectile>(out ServerProjectile serverProjectile))
        {
            // Pass the direction and damage to the projectile's logic
            serverProjectile.OnSpawn(direction, (weaponInstance.weaponData.baseDamage + bonusStat.bonusDamage)*extraDamage, isCrit, critDamageMultiplier);
        }
    }
    public bool CanShoot()
    {
        if (currentAmmo <= 0 || isReloading) return false;
        return Time.time >= lastShootTime + RpmToSecondsPerShot();
    }
    private float RpmToSecondsPerShot()
    {
        return 60f / this.fireRate;
    }
    public void OnSpecialReload()
    {
        throw new System.NotImplementedException();
    }

    public void OnSpecialShoot()
    {
        throw new System.NotImplementedException();
    }

    public void OnStowWeapon()
    {

        if (isReloading)
        {
            float progress = reloadTimer / reloadTime;
            //hiting quick reload
            if (progress >= 0.7f)
            {
                FinishReload();
            }
            else //cancel reload
            {
                isReloading = false;
            }
        }
        Hide();
    }
    public void Show()
    {
        visualRoot.SetActive(true);
    }
    public void Hide()
    {
        visualRoot.SetActive(false);
    }
}
