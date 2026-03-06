using UnityEngine;

public class LockedInBuff : BaseBuff
{
    public LockedInBuff(PlayerStats owner, BuffDefinition data,float duration)
        : base(owner, data, duration) { }

    public override void Update()
    {
        base.Update();
    }
}