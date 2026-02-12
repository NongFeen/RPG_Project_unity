[System.Serializable]
public class SkillInstance
{
    //store in player as each skill slot
    public SkillDefinition definition;
    public float cooldownRemaining;
    public bool CanUse => cooldownRemaining <= 0f;

    public void Tick(float dt)
    {
        if (cooldownRemaining > 0)
            cooldownRemaining -= dt;
    }

    public void TriggerCooldown()
    {
        cooldownRemaining = definition.cooldown;
    }
}
