using UnityEngine;

public class SteelStrongSkillLogic : SkillLogic
{
    public SteelStrongSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        controller.GetComponent<PlayerStats>().HealServerRpc(definition.magicNumber1);
        controller.GetComponent<PlayerStats>().AddBuffServer(BuffType.SteelStrong,definition.magicNumber2);
        // controller.GetComponent<PlayerStats>().AddBuffServerRpc(BuffType.WellOfBlessing,definition.magicNumber2);
        return true;
    }
}