using UnityEngine;

public class ABigGuy : BaseBuff
{
    public ABigGuy(PlayerStats owner, BuffDefinition data,float duration)
        : base(owner, data, duration) { }
    public override void Update()
    {
        //just display for summoned minion, no actual buff effect
        base.Update();
    }
}