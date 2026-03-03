using UnityEngine;

public class AlongSkillLogic : SkillLogic
{
    public AlongSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        // controller.GetComponent<PlayerStats>().AddBuff(BuffType.ChadAura, definition.magicNumber1);
        // spawn a minion Projectile follow player
        return true;
    }
}