using UnityEngine;

public class WellofBlessing : BaseBuff
{
    public WellofBlessing(PlayerStats owner, BuffDefinition data,float duration)
        : base(owner, data, duration) { }
    public override void Update()
    {
        // if player not standing in well remove buff
        base.Update();
    }
}