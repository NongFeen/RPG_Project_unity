#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerStats))]
public class PlayerStatsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerStats stats = (PlayerStats)target;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Stats visible only in Play Mode", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Runtime Stats", EditorStyles.boldLabel);

        DrawStats("Stable Stats", stats.stableStats.Value);
        DrawStats("Active Stats", stats.activeStats.Value);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Current HP", stats.currentHP.Value.ToString());

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Active Buffs", EditorStyles.boldLabel);

        var buffs = stats.GetActiveBuffs();

        if (buffs.Count == 0)
        {
            EditorGUILayout.LabelField("None");
        }
        else
        {
            foreach (var buff in buffs)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("Buff Type", buff.Key.ToString());
                EditorGUILayout.LabelField("Time Left", buff.Value.duration.ToString("F2"));

                EditorGUILayout.EndVertical();
            }
        }

        Repaint();
    }

    void DrawStats(string title, Stats stat)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Health", stat.health.ToString());
        EditorGUILayout.LabelField("Defense", stat.defense.ToString());
        EditorGUILayout.LabelField("Crit Rate", stat.critRate.ToString());
        EditorGUILayout.LabelField("Crit Damage", stat.critDamage.ToString());
        EditorGUILayout.LabelField("Extra Damage", stat.extraDamage.ToString());

        EditorGUILayout.EndVertical();
    }
}
#endif