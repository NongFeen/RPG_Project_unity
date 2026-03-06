using UnityEngine;

public abstract class BaseBuff
{
    protected PlayerStats owner;
    protected BuffDefinition data;
    protected float endTime;
    public float duration;
    public BuffType Type => data.buffType;
    public bool IsExpired => duration <= 0;

    public BaseBuff(PlayerStats owner, BuffDefinition data,float duration = 0)
    {
        this.owner = owner;
        this.data = data;
        this.duration = duration;
        endTime = Time.time + duration;


        OnApply();
    }

    protected virtual void OnApply() { }

    public virtual void Update()
    {
        duration -= Time.deltaTime;
    }

    protected virtual void OnRemove() { }

    public void Remove()
    {
        OnRemove();
    }

    public virtual Stats ModifyStats(Stats baseStats)
    {
        baseStats += new Stats
        {
            health = data.flatHealth,
            defense = data.flatDefense,
            critRate = data.flatCritRate,
            critDamage = data.flatCritDamage,
            extraDamage = data.flatExtraDamage
        };
        baseStats.health += Mathf.RoundToInt(baseStats.health * data.percentHealth);
        baseStats.defense += Mathf.RoundToInt(baseStats.defense * data.percentDefense); 
        baseStats.critRate += baseStats.critRate * data.percentCritRate;
        baseStats.critDamage += baseStats.critDamage * data.percentCritDamage;
        baseStats.extraDamage += baseStats.extraDamage * data.percentExtraDamage;
        return baseStats; // default no stat change
    }
}