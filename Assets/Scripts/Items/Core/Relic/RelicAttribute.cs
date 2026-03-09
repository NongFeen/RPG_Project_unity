[System.Serializable]
public struct RelicAttribute
{
    public RelicAttributeType type;
    public float value;
    public override string ToString()    {
        return $"{type}: {value:F2}%";
    }
}