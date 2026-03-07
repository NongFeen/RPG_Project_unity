using UnityEngine;

public class ChadAuraSkillLogic : SkillLogic
{
    public ChadAuraSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        controller.SpawnProjectileServerRpc(0,controller.transform.position,Vector3.zero,definition.skillType);
        return true;
    }
}