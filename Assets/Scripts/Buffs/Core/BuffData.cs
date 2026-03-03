using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff Data")]
public class BuffData : ScriptableObject
{
    public BuffType buffType;

    [Header("Duration")]
    public float duration = 5f;

    [Header("Flat Modifiers")]
    public int flatHealth;
    public int flatDefense;
    public float flatCritRate;
    public float flatCritDamage;
    public float flatExtraDamage;

    [Header("Percent Modifiers (0.2 = +20%)")]
    public float percentHealth;
    public float percentDefense;
    public float percentCritRate;
    public float percentCritDamage;
    public float percentExtraDamage;

    [Header("Visual")]
    public GameObject particlePrefab;
}