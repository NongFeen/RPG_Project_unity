using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ClassStatData))]
public class ClassStatDataEditor : Editor
{
    private Vector2 scroll;
    private int WITDH = 70;
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stat Preview By Level", EditorStyles.boldLabel);

        ClassStatData data = (ClassStatData)target;

        // Check missing curves
        if (data.healthCurve == null ||
            data.defenseCurve == null ||
            data.critRateCurve == null ||
            data.critDamageCurve == null)
        {
            EditorGUILayout.HelpBox("One or more curves are missing!", MessageType.Warning);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(300));

        // Table Header
        EditorGUILayout.BeginHorizontal("box");

        EditorGUILayout.LabelField("Lvl", GUILayout.Width(WITDH));
        EditorGUILayout.LabelField("HP", GUILayout.Width(WITDH));
        EditorGUILayout.LabelField("DEF", GUILayout.Width(WITDH));
        EditorGUILayout.LabelField("CRate", GUILayout.Width(WITDH));
        EditorGUILayout.LabelField("CDMG", GUILayout.Width(WITDH));

        EditorGUILayout.EndHorizontal();

        // Preview Levels
        for (int i = 1; i <= 30; i++)
        {
            int hp = Mathf.RoundToInt(data.healthCurve.Evaluate(i));
            int def = Mathf.RoundToInt(data.defenseCurve.Evaluate(i));

            // Convert to %
            float critRate = data.critRateCurve.Evaluate(i) * 100f;
            float critDmg = data.critDamageCurve.Evaluate(i) * 100f;

            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(WITDH));
            EditorGUILayout.LabelField(hp.ToString(), GUILayout.Width(WITDH));
            EditorGUILayout.LabelField(def.ToString(), GUILayout.Width(WITDH));
            EditorGUILayout.LabelField($"{critRate:0.##}%", GUILayout.Width(WITDH));
            EditorGUILayout.LabelField($"{critDmg:0.##}%", GUILayout.Width(WITDH));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}
