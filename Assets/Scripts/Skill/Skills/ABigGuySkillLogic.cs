using UnityEngine;

public class ABigGuySkillLogic : SkillLogic
{
    public ABigGuySkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        return true;
    }
}