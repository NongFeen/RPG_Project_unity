using UnityEngine;

public class ChadAuraBuff : BaseBuff
{
    public ChadAuraBuff(PlayerStats owner, BuffDefinition data,float duration)
        : base(owner, data, duration) { }
    public override void Update()
    {
        if (owner.IsOwner)
        {
            float healAmount = owner.activeStats.Value.health * 0.05f * Time.deltaTime;
            owner.HealServerRpc(healAmount);
        }
        base.Update();
    }
}
