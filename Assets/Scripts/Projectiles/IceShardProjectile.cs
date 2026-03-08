using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class IceShardProjectile : ServerProjectile
{   
    [SerializeField] private float slowMultiplier = 0.95f;
    public override void Update()
    {
        speed *= slowMultiplier; //slow down over time
        base.Update();
    }
}
