using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillDatabase", menuName = "System/Skill Database")]
public class ClassDataBase : ScriptableObject
{
    [SerializeField] private List<ClassSkillData> classSkill;
    private Dictionary<ClassType, ClassSkillData> classSkillDictionary;
    [SerializeField] private List<ClassStatData> classStat;
    private Dictionary<ClassType, ClassStatData> classStatDictionary;   

    void OnEnable()
    {
        BuildDictionary();
    }
    private void BuildDictionary()
    {
        BuildSkillDictionary();
        BuildStatDictionary();
    }

    private void BuildSkillDictionary()
    {
        classSkillDictionary = new Dictionary<ClassType, ClassSkillData>();

        foreach (ClassSkillData classSkillData in classSkill)
        {
            if (classSkillData == null) continue;

            if (!classSkillDictionary.ContainsKey(classSkillData.characterClass))
                classSkillDictionary.Add(classSkillData.characterClass, classSkillData);
            else
                Debug.LogWarning($"Duplicate Class found: {classSkillData.characterClass}", this);
        }
    }
    private void BuildStatDictionary()
    {
        classStatDictionary = new Dictionary<ClassType, ClassStatData>();

        foreach (ClassStatData classStatData in classStat)
        {
            if (classStatData == null) continue;

            if (!classStatDictionary.ContainsKey(classStatData.classType))
                classStatDictionary.Add(classStatData.classType, classStatData);
            else
                Debug.LogWarning($"Duplicate Class found: {classStatData.classType}", this);
        }
    }

    public ClassSkillData GetClassSkillData(ClassType classType)
    {
        if (classSkillDictionary == null)
            BuildDictionary();

        return classSkillDictionary[classType];
    }
    public SkillDefinition GetSkillDefinition(SkillBehaviourType skillType)
    {
        if (classSkillDictionary == null)
            BuildDictionary();

        foreach (var classSkillData in classSkillDictionary.Values)
        {
            if (classSkillData.skillV != null && classSkillData.skillV.skillType == skillType)
                return classSkillData.skillV;

            if (classSkillData.skillQ != null && classSkillData.skillQ.skillType == skillType)
                return classSkillData.skillQ;

            if (classSkillData.skillF != null && classSkillData.skillF.skillType == skillType)
                return classSkillData.skillF;
        }

        Debug.LogWarning($"Skill not found in any class: {skillType}", this);
        return null;
    }
    public ClassStatData GetClassStatData(ClassType classType)
    {
        if (classStatDictionary == null)
            BuildDictionary();

        return classStatDictionary[classType];
    }

}

