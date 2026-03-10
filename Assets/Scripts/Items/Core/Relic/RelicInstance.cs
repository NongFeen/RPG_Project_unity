using System.Collections.Generic;

[System.Serializable]
public class RelicInstance : ItemInstance
{
    public RelicRarity rarity;

    public List<RelicAttribute> attributes = new List<RelicAttribute>();
    public override bool IsEmpty => attributes.Count == 0;

    public RelicInstance(Item itemData, int stackCount = 1) : base(itemData, stackCount)
    {
        //do notihng
    }
    public RelicInstance() : base(null)
    {
        //do notihng
    }
    public override string ToString()
    {
        string attrString = "";
        foreach (RelicAttribute attr in attributes)
        {
            attrString += attr;
        }
        return $"{rarity} (\n{attrString}";
    }
}