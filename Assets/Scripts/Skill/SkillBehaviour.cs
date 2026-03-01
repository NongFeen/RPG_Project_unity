using Unity.Netcode;
using UnityEngine;

public abstract class SkillBehaviour 
{
    public SkillDefinition definition;
    protected PlayerStats owner;
    public float cooldownRemaining;
    public bool CanUse => cooldownRemaining <= 0f;

    public virtual void Initialize(PlayerStats owner,SkillDefinition def)
    {
        this.owner = owner;
        definition = def;
    }

    public virtual void ActivateSkill(Vector3 targetPos)
    {
        if(CanUse)
        {
            cooldownRemaining = definition.cooldown;
        }
    }

    public virtual void Tick(float time)
    {
        if(cooldownRemaining > 0)
        {
            cooldownRemaining -= time;
        }
    }
    
}
