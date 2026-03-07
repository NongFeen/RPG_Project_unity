using UnityEngine;

public class Along : BaseBuff
{
    public Along(PlayerStats owner, BuffDefinition data,float duration)
        : base(owner, data, duration) { }
    public override void Update()
    {
        base.Update();
    }
}