using UnityEngine;

[CreateAssetMenu(menuName = "Player/Skill")]
public class SkillDefinition : ScriptableObject
{
    public SkillId skillId;
    public string displayName;
    public float cooldown;
    public GameObject skillPrefab; // NetworkBehaviour prefab
    public Sprite icon;
}

