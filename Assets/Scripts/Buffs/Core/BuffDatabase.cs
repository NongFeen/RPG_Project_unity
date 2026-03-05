using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffDatabase", menuName = "System/Buff Database")]
public class BuffDatabase : ScriptableObject
{
    [SerializeField] private List<BuffDefinition> buffDefinitions;
    private Dictionary<BuffType, BuffDefinition> buffDictionary;
    void OnEnable()
    {
        BuildDictionary();
    }
    private void BuildDictionary()
    {
        buffDictionary = new Dictionary<BuffType, BuffDefinition>();

        foreach (BuffDefinition buffDef in buffDefinitions)
        {
            if (buffDef == null) continue;

            if (!buffDictionary.ContainsKey(buffDef.buffType))
                buffDictionary.Add(buffDef.buffType, buffDef);
            else
                Debug.LogWarning($"Duplicate Buff found: {buffDef.buffType}", this);
        }
    }

    public BuffDefinition GetBuffDefinition(BuffType type)
    {
        if (buffDictionary == null)
            BuildDictionary();

        return buffDictionary[type];
    }

}
