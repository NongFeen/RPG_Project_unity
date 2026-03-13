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

        AnimationCurve curve = GameDatabase.Instance.GetExperienceData().experienceCurve;

        if (curve == null)
        {
            EditorGUILayout.HelpBox("No experience curve assigned.", MessageType.Warning);
            EditorGUILayout.EndScrollView();
            return;
        }

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