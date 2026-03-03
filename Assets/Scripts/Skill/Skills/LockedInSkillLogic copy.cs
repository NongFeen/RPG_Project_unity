using UnityEngine;

public class LockedInSkillLogic : SkillLogic
{
    public LockedInSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        controller.GetComponent<PlayerStats>().AddBuff(BuffType.LockedIn, this.definition.magicNumber1);
        return true;
    }
}