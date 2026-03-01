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
        Debug.Log(definition.displayName + " CooldownTimer: "+ CooldownRemaining());
        if (!IsReady()) return;

        coolDownTimer = definition.cooldown;
        Activate(targetPos);
    }
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
    public abstract void Activate(Vector3 targetPos);

    public SkillDefinition GetSkillDefinition()
    {
        return definition;
    }
}