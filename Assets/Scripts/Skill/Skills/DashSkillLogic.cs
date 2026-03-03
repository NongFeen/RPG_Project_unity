using UnityEngine;

public class DashSkillLogic : SkillLogic
{
    public DashSkillLogic(PlayerSkillController controller, SkillDefinition def)
        : base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        Vector2 dir = (targetPos - controller.transform.position).normalized;

        controller.GetComponent<PlayerMovement>().AddExternalVelocity(dir * definition.magicNumber1);
        return true;
    }
}