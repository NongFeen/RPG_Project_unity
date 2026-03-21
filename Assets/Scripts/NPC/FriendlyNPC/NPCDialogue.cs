using UnityEngine;

[CreateAssetMenu(fileName ="NewNPCDialogue",menuName ="NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public string[] dialogueLine;
    public float textSpeed = 0.05f;
    public float autoProgressDeley = 1.5f;

}