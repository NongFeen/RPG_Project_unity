using UnityEngine;

public class ProjectileSkillLogic : SkillLogic
{
    public ProjectileSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override void Activate(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - controller.transform.position).normalized;

        controller.SpawnProjectileServerRpc(definition.magicNumber1,dir,definition.skillType);
    }
}