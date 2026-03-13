using UnityEngine;

public static class RelicGenerator
{
    public static RelicInstance GenerateRelic(RelicRarity rarity)
    {
        RelicInstance relic = new RelicInstance();
        relic.rarity = rarity;

        int statCount = GetStatCount(rarity);

        for (int i = 0; i < statCount; i++)
        {
            RelicAttributeType type = (RelicAttributeType)Random.Range(0, 4);

            float value = RollValue(type);

            relic.attributes.Add(new RelicAttribute
            {
                type = type,
                value = value
            });
        }

        return relic;
    }

    static int GetStatCount(RelicRarity rarity)
    {
        switch (rarity)
        {
            case RelicRarity.Common: return 1;
            case RelicRarity.Rare: return 2;
            case RelicRarity.Epic: return 3;
        }

        return 1;
    }

    static float RollValue(RelicAttributeType type)
    {
        switch (type)
        {
            case RelicAttributeType.CritRate:
                return Random.Range(0.01f, 0.05f);

            case RelicAttributeType.CritDamage:
                return Random.Range(0.02f, 0.1f);

            case RelicAttributeType.Health:
                return Random.Range(0.01f, 0.1f);

            case RelicAttributeType.BonusDamage:
                return Random.Range(0.01f, 0.05f);
        }

        return 0;
    }
}