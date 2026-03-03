using UnityEngine;

public class StatBuff : BaseBuff
{
    public StatBuff(PlayerStats owner, BuffData data)
        : base(owner, data) { }

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