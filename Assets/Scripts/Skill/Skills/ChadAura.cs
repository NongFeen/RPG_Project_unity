using UnityEngine;

public class ChadAuraSkillLogic : SkillLogic
{
    public ChadAuraSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        // controller.GetComponent<PlayerStats>().AddBuff(BuffType.ChadAura, definition.magicNumber1);
        return true;
    }
}