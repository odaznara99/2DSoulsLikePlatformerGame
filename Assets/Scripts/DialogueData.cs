using UnityEngine;

/// <summary>
/// ScriptableObject that holds a sequence of dialogue lines for a conversation.
/// Create via: Assets > Create > Dialogue > Dialogue Data
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Tooltip("The sequence of lines in this dialogue conversation.")]
    public DialogueLine[] lines;
}
