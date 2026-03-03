using UnityEngine;

public class BuffInstance
{
    private BuffData data;
    private PlayerStats owner;
    private float endTime;
    private GameObject particleInstance;

    public BuffType Type => data.buffType;
    public bool IsExpired => Time.time >= endTime;

    public BuffInstance(BuffData data, PlayerStats owner)
    {
        this.data = data;
        this.owner = owner;
        endTime = Time.time + data.duration;

        OnApply();
    }

    private void OnApply()
    {
        if (data.particlePrefab != null)
        {
            particleInstance = GameObject.Instantiate(
                data.particlePrefab,
                owner.transform
            );
        }
    }

    public void Update()
    {
        // ถ้าพี่อยากทำ tick / animate / logic เพิ่ม
    }

    public void OnRemove()
    {
        if (particleInstance != null)
            GameObject.Destroy(particleInstance);
    }

    public Stats ApplyModifier(Stats baseStats)
    {
        Stats result = baseStats;

        result.health += data.flatHealth;
        result.defense += data.flatDefense;
        result.critRate += data.flatCritRate;
        result.critDamage += data.flatCritDamage;
        result.extraDamage += data.flatExtraDamage;

        result.health += Mathf.RoundToInt(baseStats.health * data.percentHealth);
        result.defense += Mathf.RoundToInt(baseStats.defense * data.percentDefense);
        result.critRate += baseStats.critRate * data.percentCritRate;
        result.critDamage += baseStats.critDamage * data.percentCritDamage;
        result.extraDamage += baseStats.extraDamage * data.percentExtraDamage;

        return result;
    }
}