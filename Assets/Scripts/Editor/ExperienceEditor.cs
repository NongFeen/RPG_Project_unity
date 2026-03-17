#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(PlayerExperience))]
public class PlayerExperienceEditor : Editor
{
    Vector2 Scrollbar;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        PlayerExperience playerExperience = (PlayerExperience)target;
        
        Scrollbar = EditorGUILayout.BeginScrollView(Scrollbar, GUILayout.Height(300));

        ExperienceData experienceData = null;
        if (GameDatabase.Instance != null)
            experienceData = GameDatabase.Instance.GetExperienceData();

#if UNITY_EDITOR
        if (experienceData == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:ExperienceData");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                experienceData = AssetDatabase.LoadAssetAtPath<ExperienceData>(path);
            }
        }
#endif

        if (experienceData == null || experienceData.experienceCurve == null)
        {
            EditorGUILayout.HelpBox("No experience curve assigned. Add a GameDatabase to the scene or set ExperienceData asset.", MessageType.Warning);
            EditorGUILayout.EndScrollView();
            return;
        }

        AnimationCurve curve = experienceData.experienceCurve;

        for(int i = 1 ; i <= 30; i++)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"Level {i}");
            int expRequired = (int)curve.Evaluate(i);
            EditorGUILayout.LabelField($"{expRequired} EXP");
            EditorGUILayout.EndHorizontal();    
        }
        EditorGUILayout.EndScrollView();
    }
}
#endif