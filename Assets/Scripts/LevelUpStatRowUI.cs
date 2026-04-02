using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single stat row inside the Level-Up UI panel.
/// Wire up references in the Inspector, then call <see cref="Setup"/> from
/// <see cref="LevelUpUI"/> after instantiating the row prefab.
///
/// Recommended prefab layout
/// -------------------------
/// Row (horizontal layout)
///  ├─ StatNameText    (TextMeshProUGUI)
///  ├─ StatLevelText   (TextMeshProUGUI)
///  ├─ CostText        (TextMeshProUGUI)
///  └─ LevelUpButton   (Button)
///      └─ ButtonText  (TextMeshProUGUI — optional, shows "+1")
/// </summary>
public class LevelUpStatRowUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Label showing the stat name (e.g. 'Vitality').")]
    public TextMeshProUGUI statNameText;

    [Tooltip("Label showing the current level of this stat (e.g. '0').")]
    public TextMeshProUGUI statLevelText;

    [Tooltip("Label showing the soul cost to raise this stat by 1.")]
    public TextMeshProUGUI costText;

    [Tooltip("Button the player clicks to spend souls and raise this stat.")]
    public Button levelUpButton;

    // ── Runtime state ─────────────────────────────────────────────────────────

    private StatType stat;
    private Action<StatType> onLevelUpCallback;

    /// <summary>The stat this row represents.</summary>
    public StatType Stat => stat;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Initialises this row with stat data and the level-up callback.
    /// Called by <see cref="LevelUpUI"/> after instantiating the prefab.
    /// </summary>
    /// <param name="statType">Stat this row represents.</param>
    /// <param name="currentLevel">Current level of the stat.</param>
    /// <param name="cost">Soul cost to raise this stat right now.</param>
    /// <param name="onLevelUp">Invoked when the player presses the button.</param>
    public void Setup(StatType statType, int currentLevel, int cost, Action<StatType> onLevelUp)
    {
        stat              = statType;
        onLevelUpCallback = onLevelUp;

        if (statNameText != null)
            statNameText.text = statType.ToString();

        if (levelUpButton != null)
        {
            levelUpButton.onClick.RemoveAllListeners();
            levelUpButton.onClick.AddListener(() => onLevelUpCallback?.Invoke(stat));
        }

        UpdateDisplay(currentLevel, cost, GameManager.Instance?.playerData.souls ?? 0);
    }

    /// <summary>
    /// Refreshes the display values after a level-up.  Called by
    /// <see cref="LevelUpUI"/> whenever the player's souls or the stat level
    /// changes.
    /// </summary>
    public void Refresh(int currentLevel, int cost, int playerSouls)
    {
        UpdateDisplay(currentLevel, cost, playerSouls);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private void UpdateDisplay(int currentLevel, int cost, int playerSouls)
    {
        if (statLevelText != null)
            statLevelText.text = currentLevel.ToString();

        if (costText != null)
            costText.text = cost + " Souls";

        // Grey out the button when the player cannot afford this level-up
        if (levelUpButton != null)
            levelUpButton.interactable = playerSouls >= cost;
    }
}
