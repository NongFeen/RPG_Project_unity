using UnityEngine;

public class ABigGuySkillLogic : SkillLogic
{
    public ABigGuySkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        controller.SpawnProjectileServerRpc(definition.magicNumber1,controller.transform.position,Vector2.zero,definition.skillType);
        return true;
    }
}