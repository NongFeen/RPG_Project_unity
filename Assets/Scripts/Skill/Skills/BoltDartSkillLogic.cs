using UnityEngine;

public class BoltDartSkillLogic : SkillLogic
{
    public BoltDartSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - controller.transform.position).normalized;

        controller.SpawnProjectileServerRpc(definition.magicNumber1,controller.transform.position,dir,definition.skillType);
        return true;
    }
}