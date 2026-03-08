using UnityEngine;
using Unity.Netcode;

public class PulseRifleBehaviour : WeaponBehaviour
{
    [SerializeField] int  bulletPerBurst = 3;
    [SerializeField] float burstInterval = 0.1f;
    [SerializeField] private PlayerShooting shooter;
    private float burstTimer;
    private int bulletsFiredInBurst;
    public override void Update()
    {
        base.Update();
        if(bulletsFiredInBurst > 0)
        {
            burstTimer += Time.deltaTime;
            if(burstTimer >= burstInterval)
            {
                burstTimer = 0;
                bulletsFiredInBurst--;
                Vector2 dir = shooter.AimDirection();
            }
        }
        else
        {
            lastShootTime = Time.time;
        }
    }
}