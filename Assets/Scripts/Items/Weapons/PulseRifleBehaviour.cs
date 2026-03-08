using UnityEngine;
using Unity.Netcode;

public class PulseRifleBehaviour : WeaponBehaviour
{
    [SerializeField] int bulletPerBurst = 3;
    [SerializeField] float burstInterval = 0.1f;
    [SerializeField] private PlayerShooting shooter;
    private float burstTimer;
    private int bulletsLeft;
    private float burstCritDamage;
    private float burstDamageMultiplier;
    private float burstFlatExtraDamage;
    private float CurrentCritChance;
    private ServerRpcParams burstRpc;

    public override void Update()
    {
        base.Update();

        if (!NetworkManager.Singleton.IsServer) return;

        if (bulletsLeft > 0)
        {
            burstTimer += Time.deltaTime;

            if (burstTimer >= burstInterval)
            {
                burstTimer = 0;
                bulletsLeft--;

                Vector2 dir = shooter.AimDirection();

                base.SpawnProjectileServer(
                    shooter.transform.position,
                    dir,
                    UnityEngine.Random.value < CurrentCritChance,
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
    public override void Shoot(Vector2 direction, PlayerStats playerStats, ServerRpcParams rpcParams)
    {
        base.Shoot(direction, playerStats, rpcParams);
        shooter = playerStats.GetComponent<PlayerShooting>();
        CurrentCritChance = playerStats.activeStats.Value.critRate + weaponInstance.bonusStat.critRate;
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

        base.SpawnProjectileServer(
            firePointPosition,
            direction,
            isCrit,
            critDamageMultiplier,
            damageMultiplier,
            flatExtraDamage,
            rpcParams
        );
        
        burstCritDamage = critDamageMultiplier;
        burstDamageMultiplier = damageMultiplier;
        burstFlatExtraDamage = flatExtraDamage;
        burstRpc = rpcParams;

        burstTimer = 0;
        bulletsLeft = bulletPerBurst - 1;
    }
}