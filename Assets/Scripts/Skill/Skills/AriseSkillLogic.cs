using UnityEngine;

public class AriseSkillLogic : SkillLogic
{
    public AriseSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        return true;
    }
}