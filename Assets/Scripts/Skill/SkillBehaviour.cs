using Unity.Netcode;
using UnityEngine;

public abstract class SkillBehaviour : NetworkBehaviour
{
    protected PlayerStats owner;

    public virtual void Initialize(PlayerStats owner)
    {
        this.owner = owner;
    }

    public abstract void ActivateSkill(Vector3 targetPos);
}
