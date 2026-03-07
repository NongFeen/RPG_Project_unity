using UnityEngine;

public abstract class SkillLogic
{
    protected PlayerSkillController controller;
    protected SkillDefinition definition;
    private float coolDownTimer;

    public SkillLogic(PlayerSkillController controller, SkillDefinition def)
    {
        this.controller = controller;
        this.definition = def;
    }

    public void TryActivate(Vector3 targetPos)
    {
        // Debug.Log(definition.displayName + " CooldownTimer: "+ CooldownRemaining());
        if (!IsReady()) return;
        bool success = Activate(targetPos);
        if (success)
        {
            coolDownTimer = definition.cooldown;
        }
    }
    public abstract bool Activate(Vector3 targetPos);
    public void Tick(float deltaTime)
    {
        coolDownTimer -= deltaTime;

    }
    public bool IsReady()
    {
        return coolDownTimer <=0;
    }
    public float CooldownRemaining()
    {
        return coolDownTimer;
    }
    public float PercentCoolDown()
    {
        return coolDownTimer / definition.cooldown;
    }

    public SkillDefinition GetSkillDefinition()
    {
        return definition;
    }
}