using UnityEngine;

public class SteelStrongBuff : BaseBuff
{
    public SteelStrongBuff(PlayerStats owner, BuffDefinition data,float duration)
        : base(owner, data, duration) { }
    public override void Update()
    {
        //heal player hp every second by 1% of max hp
        if (owner.IsOwner)
        {
            float healAmount = owner.activeStats.Value.health * 0.01f * Time.deltaTime;
            owner.HealServerRpc(healAmount);
        }
        base.Update();
    }
}
