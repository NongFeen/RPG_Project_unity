using UnityEngine;
[CreateAssetMenu(menuName = "Player/ClassDefinition")]
public class ClassSkillData : ScriptableObject
{
    public ClassType characterClass;
    public SkillDefinition skillV;
    public SkillDefinition skillQ;
    public SkillDefinition skillF;
}