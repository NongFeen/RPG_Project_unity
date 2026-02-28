using UnityEngine;

[CreateAssetMenu(menuName = "Player/Skill")]
public class SkillDefinition : ScriptableObject
{
    public SkillBehaviourType skillType;
    [SerializeField]private string behaviourClassName;
    public string displayName;
    public float cooldown;
    public Sprite icon;
    public GameObject projectilePrefab;
    public float magicNumber1;
    public float magicNumber2;
    public float magicNumber3;

    public System.Type GetBehaviourType()
    {
        return System.Type.GetType(behaviourClassName);
    }
}

