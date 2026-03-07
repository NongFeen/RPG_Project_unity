using UnityEngine;

[CreateAssetMenu(menuName = "Buff/Buff Data")]
public class BuffDefinition : ScriptableObject
{
    public BuffType buffType;

    [Header("Duration")]
    public float duration = 5f;
    public bool isDebuff = false;

    [Header("Flat Modifiers")]
    public int flatHealth;
    public int flatDefense;
    public float flatCritRate;
    public float flatCritDamage;

    [Header("Percent Modifiers (0.2 = +20%)")]
    public float percentHealth;
    public float percentDefense;
    public float percentCritRate;
    public float percentCritDamage;
    public float percentExtraDamage;

    [Header("Visual")]
    public GameObject particlePrefab;
    public Sprite buffIcon;
}