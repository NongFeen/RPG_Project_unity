using UnityEngine;

public class WellBlessingSkillLogic : SkillLogic
{
    public WellBlessingSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        controller.SpawnProjectileServerRpc(0,controller.transform.position,Vector3.zero,definition.skillType);
        controller.GetComponent<PlayerStats>().HealServerRpc(definition.magicNumber1);
        return true;
    }
}