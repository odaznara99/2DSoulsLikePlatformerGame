using UnityEngine;

/// <summary>
/// Represents a single line of dialogue with an optional speaker name.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [Tooltip("Name of the speaker. Leave empty to hide the speaker name box.")]
    public string speakerName;

    [TextArea(2, 5)]
    [Tooltip("The dialogue text to display.")]
    public string text;
}
