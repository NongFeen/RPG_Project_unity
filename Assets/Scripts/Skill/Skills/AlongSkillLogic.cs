using UnityEngine;

public class AlongSkillLogic : SkillLogic
{
    public AlongSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        return true;
    }
}