using UnityEngine;
using Unity.Netcode;

public class PulseRifleBehaviour : WeaponBehaviour
{
    [SerializeField] int bulletPerBurst = 3;
    [SerializeField] float burstInterval = 0.1f;

    // Server burst state
    private float burstTimer;
    private int bulletsLeft;
    private float burstCritDamage;
    private float burstDamageMultiplier;
    private float burstFlatExtraDamage;
    private float currentCritChance;
    private ServerRpcParams burstRpc;

    // Client burst state
    private int clientBulletsLeft;
    private float clientBurstTimer;

    public override void Update()
    {
        base.Update();

        if (!NetworkManager.Singleton.IsServer && clientBulletsLeft > 0)
        {
            clientBurstTimer += Time.deltaTime;
            if (clientBurstTimer >= burstInterval)
            {
                clientBurstTimer = 0;
                clientBulletsLeft--;
                ConsumeAmmo();
                lastShootTime = Time.time + RpmToSecondsPerShot();
            }
        }

        if (NetworkManager.Singleton.IsServer && bulletsLeft > 0)
        {
            burstTimer += Time.deltaTime;
            if (burstTimer >= burstInterval)
            {
                burstTimer = 0;
                bulletsLeft--;

                Vector2 dir = playerShooting.GetAimDirection();

                base.SpawnProjectileServer(
                    playerShooting.weaponPos.transform.position,
                    dir,
                    UnityEngine.Random.value < currentCritChance,
                    burstCritDamage,
                    burstDamageMultiplier,
                    burstFlatExtraDamage,
                    burstRpc
                );
                ConsumeAmmo();
                lastShootTime = Time.time + RpmToSecondsPerShot();
            }
        }
    }

    public override void OnShoot(Vector2 direction)
    {
        if (!CanShoot()) return;
        lastShootTime = Time.time + RpmToSecondsPerShot(); // lock firing during burst
        ConsumeAmmo(); // shot 1
        clientBulletsLeft = bulletPerBurst - 1;
        clientBurstTimer = 0;
    }

    public override void Shoot(Vector2 direction, Transform weaponHolder, PlayerStats playerStats, ServerRpcParams rpcParams)
    {
        base.Shoot(direction, weaponHolder, playerStats, rpcParams);
        currentCritChance = playerStats.activeStats.Value.critRate + weaponInstance.bonusStat.critRate;
    }

    public override void SpawnProjectileServer(
        Vector3 firePointPosition,
        Vector2 direction,
        bool isCrit,
        float critDamageMultiplier,
        float damageMultiplier,
        float flatExtraDamage,
        ServerRpcParams rpcParams)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        base.SpawnProjectileServer(firePointPosition, direction, isCrit, critDamageMultiplier, damageMultiplier, flatExtraDamage, rpcParams);

        burstCritDamage = critDamageMultiplier;
        burstDamageMultiplier = damageMultiplier;
        burstFlatExtraDamage = flatExtraDamage;
        burstRpc = rpcParams;
        burstTimer = 0;
        bulletsLeft = bulletPerBurst - 1;
    }
}