using Unity.Netcode;
using UnityEngine;

public class DashSkill : SkillBehaviour
{
    [SerializeField] float dashSpeed = 10f;
    public override void Initialize(PlayerStats owner, SkillDefinition def)
    {
        base.Initialize(owner, def);
        dashSpeed = def.magicNumber1;
    }
    public override void ActivateSkill(Vector3 targetPos)
    {
        if (!CanUse) return;

        cooldownRemaining = definition.cooldown;

        Vector2 dashDirection = 
            (targetPos - owner.transform.position).normalized;

        owner.GetComponent<PlayerMovement>().AddExternalVelocity(dashDirection * dashSpeed);

        Debug.Log("Dash Activated");
    }
    
}

