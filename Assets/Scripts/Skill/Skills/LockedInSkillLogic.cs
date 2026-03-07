using UnityEngine;

public class LockedInSkillLogic : SkillLogic
{
    public LockedInSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    private readonly float duration = 15f;
    public override bool Activate(Vector3 targetPos)
    {
        controller.GetComponent<PlayerStats>().AddBuffServer(BuffType.LockedIn, duration);
        return true;
    }
}