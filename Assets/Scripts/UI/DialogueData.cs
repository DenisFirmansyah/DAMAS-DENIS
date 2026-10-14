using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Dialogue")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Data")]
    public string speakerName;
    [TextArea] public string dialogue;
}
