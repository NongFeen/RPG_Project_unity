using UnityEngine;

public class DineAndDashSkillLogic : SkillLogic
{
    public DineAndDashSkillLogic(PlayerSkillController controller, SkillDefinition def)
        : base(controller, def) { }

    public override void Activate(Vector3 targetPos)
    {
        Vector2 dir = (targetPos - controller.transform.position).normalized;

        controller.GetComponent<PlayerMovement>().AddExternalVelocity(dir * definition.magicNumber1);

        controller.SpawnProjectileServerRpc(definition.magicNumber1,dir,definition.skillType);
    }
}