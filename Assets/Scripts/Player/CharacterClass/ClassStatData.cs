using UnityEngine;

[CreateAssetMenu(
    fileName = "ClassStatData",
    menuName = "RPG/Class Stat Data"
)]
public class ClassStatData : ScriptableObject
{
    public ClassType classType;

    [Header("Base Stat Curves")]

    public AnimationCurve healthCurve;
    public AnimationCurve defenseCurve;
    public AnimationCurve critRateCurve;
    public AnimationCurve critDamageCurve;


    // Helpers
    public int GetHealth(int level)
        => Mathf.RoundToInt(healthCurve.Evaluate(level));

    public int GetDefense(int level)
        => Mathf.RoundToInt(defenseCurve.Evaluate(level));

    public float GetCritRate(int level)
        => critRateCurve.Evaluate(level);

    public float GetCritDamage(int level)
        => 1.0f + critDamageCurve.Evaluate(level);
}
