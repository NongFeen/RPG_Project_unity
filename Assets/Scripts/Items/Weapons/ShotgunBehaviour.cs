using UnityEngine;
using Unity.Netcode;

public class ShotGunBehaviour : WeaponBehaviour
{
    [SerializeField]public int pelletCount = 6;
    [SerializeField]public float spreadAngle = 30f;
    [SerializeField]public float randomJitter = 3f;

    public override void Shoot(Vector2 direction,Transform weaponHolder, PlayerStats playerStats, ServerRpcParams rpcParams)
    {
        // Ammo/reload state has already been handled by the owning client.
        // Calculate damage and spawn the requested projectiles on the server.
        float critChance =
            playerStats.activeStats.Value.critRate + weaponInstance.bonusStat.critRate;

        float critDamage =
            playerStats.activeStats.Value.critDamage + weaponInstance.bonusStat.critDamage;

        float percentExtraDamage = playerStats.activeStats.Value.extraDamage;
        float flatExtraDamage = weaponInstance.bonusStat.bonusDamage;
        bool isCrit = Random.value < critChance;
        for (int i = 0; i < pelletCount; i++)
        {
            float t = (pelletCount == 1) ? 0.5f : (float)i / (pelletCount - 1);

            float angle = Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, t);
            angle += Random.Range(-randomJitter, randomJitter);

            Vector2 dir = Quaternion.Euler(0, 0, angle) * direction;

            SpawnProjectileServer(
                weaponHolder.position,
                dir,
                isCrit,
                critDamage,
                percentExtraDamage,
                flatExtraDamage,
                rpcParams
            );
        }
    }
}
