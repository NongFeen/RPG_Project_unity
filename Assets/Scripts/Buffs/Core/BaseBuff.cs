using UnityEngine;

public abstract class BaseBuff
{
    public PlayerStats owner;
    public BuffDefinition data;
    public float endTime;
    public float duration;
    public bool isDebuff => data.isDebuff;
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
        Stats modifyStats = baseStats;
        modifyStats += new Stats
        {
            health = data.flatHealth,
            defense = data.flatDefense,
            critRate = data.flatCritRate,
            critDamage = data.flatCritDamage,
        };
        modifyStats.health += Mathf.RoundToInt(modifyStats.health * data.percentHealth);
        modifyStats.defense += Mathf.RoundToInt(modifyStats.defense * data.percentDefense); 
        modifyStats.critRate += modifyStats.critRate * data.percentCritRate;
        modifyStats.critDamage += modifyStats.critDamage * data.percentCritDamage;
        modifyStats.extraDamage += modifyStats.extraDamage * data.percentExtraDamage;
        return modifyStats; // default no stat change
    }
}