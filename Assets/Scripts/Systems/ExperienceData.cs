using UnityEngine;

[CreateAssetMenu(
    fileName = "ExperienceData",
    menuName = "System/Experience Data"
)]
public class ExperienceData : ScriptableObject
{
    public AnimationCurve experienceCurve;

    public int GetLevelStartExp(int level)
    {
        if (experienceCurve == null)
            return 0;
        return Mathf.RoundToInt(experienceCurve.Evaluate(level));
    }

    public int GetNextLevelExp(int level)
    {
        if (experienceCurve == null)
            return 0;
        return Mathf.RoundToInt(experienceCurve.Evaluate(level + 1));
    }
}
