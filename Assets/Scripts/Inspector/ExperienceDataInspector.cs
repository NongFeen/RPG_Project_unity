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

        for(int i = 1 ; i <= 30; i++)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"Level {i}");
            int expRequired = (int)playerExperience.experienceCurve.Evaluate(i);
            EditorGUILayout.LabelField($"{expRequired} EXP");
            EditorGUILayout.EndHorizontal();    
        }
        EditorGUILayout.EndScrollView();
    }
}