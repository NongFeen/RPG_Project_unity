using UnityEngine;

public class DashSkillLogic : SkillLogic
{
    public DashSkillLogic(PlayerSkillController controller, SkillDefinition def)
        : base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        // Vector2 dir = (targetPos - controller.transform.position).normalized;
        controller.TryGetComponent<PlayerMovement>(out var playerMovement);
        Vector2 movedir = playerMovement.GetMoveInput();
        if(movedir == Vector2.zero) return false;
        playerMovement.AddExternalVelocity(movedir * definition.magicNumber1);
        // controller.GetComponent<PlayerMovement>().AddExternalVelocity(dir * definition.magicNumber1);
        return true;
    }
}