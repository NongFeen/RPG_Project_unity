using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class ASaltProjectile : ServerProjectile
{
    public override void OnProjectileHit(BaseNPC npc)
    {
        if(npc.GetNPCName().ToLower().Contains("slime"))
        {
            print("Salt is super effective!");
            this.damage *= 1.3f;
        }
        base.OnProjectileHit(npc);
    }
}
