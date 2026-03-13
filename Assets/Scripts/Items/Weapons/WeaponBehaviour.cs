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
    public float lastShootTime = 0;
    // public bool isReloading = false;
    [SerializeField] public bool isReloading = false;
    private float reloadTime;
    private float reloadTimer;
    public WeaponStat bonusStat;
    public virtual void SetDefault(WeaponInstance weapon)
    {
        this.weaponInstance = weapon;
        this.bonusStat = weapon.bonusStat;
        this.maxAmmo = weapon.maxAmmo;
        this.currentAmmo = maxAmmo;
        this.fireRate = weapon.fireRate;
        visualRoot.TryGetComponent<SpriteRenderer>(out var sprite);
        sprite.sprite = weapon.weaponData.image;
    }
    public virtual void Update()
    {
        if (isReloading)
        {
            float progress = reloadTimer / reloadTime;

            // if (progress >= 0.7f)
            // {
                // print($"{progress} -> {reloadTimer}/{reloadTime} " );
            // }
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= reloadTime)
                FinishReload();
        }
    }
    public virtual void OnDrawWeapon()
    {
        Show();
    }
    public virtual void OnStowWeapon()
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
    public virtual void OnReload()
    {
        // print("try Reload");
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
    public virtual void ConsumeAmmo()
    {
        // print("use ammo");
        int oldValue = currentAmmo;
        currentAmmo = Mathf.Clamp(currentAmmo - 1, 0, maxAmmo);
        OnAmmoChange?.Invoke(oldValue, currentAmmo);
    }
    public virtual void OnShoot(Vector2 direction)
    {
        // print("Onshoot");
        if (!CanShoot()) return;
        lastShootTime = Time.time;
        if (weaponInstance == null) return;
        if (weaponInstance.weaponData.serverProjectilePrefab != null )
        {
            ConsumeAmmo();
        }
    }
    public virtual void Shoot(Vector2 direction,Transform weaponHolder,PlayerStats playerStats, ServerRpcParams rpcParams)
    {
        //This is only do in server
        OnShoot(direction);
        //calcuilate crit and damage to and send to server
        float critChance =
            playerStats.activeStats.Value.critRate + weaponInstance.bonusStat.critRate;

        float critDamage =
            playerStats.activeStats.Value.critDamage + weaponInstance.bonusStat.critDamage;

        float percentExtraDamage = playerStats.activeStats.Value.extraDamage;
        float flatExtraDamage = weaponInstance.bonusStat.bonusDamage;
        SpawnProjectileServer(weaponHolder.position,direction,UnityEngine.Random.value < critChance,critDamage, percentExtraDamage, flatExtraDamage, rpcParams);
    }
    public virtual void SpawnProjectileServer(Vector3 firePointPosition, Vector2 direction, bool isCrit, float critDamageMultiplier, 
        float damageMultiplier, float flatExtraDamage, ServerRpcParams rpcParams)
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
            serverProjectile.OnSpawn(direction, (weaponInstance.weaponData.baseDamage + flatExtraDamage)*damageMultiplier, isCrit, critDamageMultiplier);
        }
    }
    public virtual bool CanShoot()
    {
        if (currentAmmo <= 0 || isReloading) return false;
        return Time.time >= lastShootTime + RpmToSecondsPerShot();
    }
    public float RpmToSecondsPerShot()
    {
        return 60f / this.fireRate;
    }
    public virtual void OnSpecialReload()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnSpecialShoot()
    {
        throw new System.NotImplementedException();
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
