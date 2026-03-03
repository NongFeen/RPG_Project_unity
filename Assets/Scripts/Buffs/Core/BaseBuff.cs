using UnityEngine;

public abstract class BaseBuff
{
    protected PlayerStats owner;
    protected BuffData data;
    protected float endTime;

    public BuffType Type => data.buffType;
    public bool IsExpired => Time.time >= endTime;

    public BaseBuff(PlayerStats owner, BuffData data)
    {
        this.owner = owner;
        this.data = data;
        endTime = Time.time + data.duration;

        OnApply();
    }

    protected virtual void OnApply() { }

    public virtual void Update() { }

    protected virtual void OnRemove() { }

    public void Remove()
    {
        OnRemove();
    }

    public virtual Stats ModifyStats(Stats baseStats)
    {
        return baseStats; // default no stat change
    }
}