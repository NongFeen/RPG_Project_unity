using UnityEngine;

public class LockedInBuff : BaseBuff
{
    public LockedInBuff(PlayerStats owner, BuffDefinition data,float duration)
        : base(owner, data, duration) { }

    public override void Update()
    {
        Debug.Log($"Buff : {data.buffType}, Time Left: {duration}");
        base.Update();
    }
    public override Stats ModifyStats(Stats baseStats)
    {
        Stats result = baseStats;

        result.health += data.flatHealth;
        result.defense += data.flatDefense;

        result.health += Mathf.RoundToInt(baseStats.health * data.percentHealth);
        result.defense += Mathf.RoundToInt(baseStats.defense * data.percentDefense);

        return result;
    }
}