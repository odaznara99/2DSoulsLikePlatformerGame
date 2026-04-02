using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Level-Up panel where the player can invest souls into stats.
///
/// Setup
/// -----
/// 1. Add a panel hierarchy to your persistent Canvas:
///    - Root panel (assign to <see cref="levelUpPanel"/>).
///    - Header: <see cref="playerLevelText"/>, <see cref="playerSoulsText"/>,
///              <see cref="nextLevelCostText"/>.
///    - Stat rows: instantiated from <see cref="statRowPrefab"/> into
///                 <see cref="statRowContainer"/>.  The prefab must contain a
///                 <see cref="LevelUpStatRowUI"/> component.
///    - <see cref="closeButton"/>.
///
/// 2. Place a <see cref="LevelUpNPC"/> trigger in the scene and assign this
///    component to its <c>levelUpUI</c> field.
///
/// The panel silently pauses the game while open (like the Shop UI).
/// </summary>
public class LevelUpUI : MonoBehaviour
{
    // ── Inspector References ──────────────────────────────────────────────────

    [Header("Panel")]
    [Tooltip("Root panel GameObject. Shown/hidden by this component.")]
    public GameObject levelUpPanel;

    [Header("Header")]
    [Tooltip("Label showing the current overall Player Level.")]
    public TextMeshProUGUI playerLevelText;

    [Tooltip("Label showing the player's current Souls.")]
    public TextMeshProUGUI playerSoulsText;

    [Tooltip("Label showing the soul cost for the next level-up (any stat).")]
    public TextMeshProUGUI nextLevelCostText;

    [Header("Stat Rows")]
    [Tooltip("Parent transform that holds the instantiated stat-row GameObjects.")]
    public Transform statRowContainer;

    [Tooltip("Prefab containing a LevelUpStatRowUI component (one row per stat).")]
    public GameObject statRowPrefab;

    [Header("Close Button")]
    [Tooltip("Button that closes the panel.")]
    public Button closeButton;

    [Header("Audio")]
    [Tooltip("SFX name played on a successful level-up.")]
    public string levelUpSfxName = "";

    [Tooltip("SFX name played when the player cannot afford a level-up.")]
    public string failSfxName = "";

    // ── Private State ─────────────────────────────────────────────────────────

    private readonly List<LevelUpStatRowUI> statRows = new List<LevelUpStatRowUI>();

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void Update()
    {
        if (levelUpPanel == null || !levelUpPanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
            ClosePanel();
    }

    private void OnEnable()
    {
        if (LevelUpManager.Instance != null)
            LevelUpManager.Instance.OnStatLeveledUp += HandleStatLeveledUp;
    }

    private void OnDisable()
    {
        if (LevelUpManager.Instance != null)
            LevelUpManager.Instance.OnStatLeveledUp -= HandleStatLeveledUp;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Opens the level-up panel and silently pauses the game.</summary>
    public void OpenPanel()
    {
        GameManager.Instance?.PauseSilent(true);

        BuildStatRows();
        RefreshHeader();

        if (levelUpPanel != null)
            levelUpPanel.SetActive(true);
    }

    /// <summary>Closes the level-up panel and resumes the game.</summary>
    public void ClosePanel()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        GameManager.Instance?.PauseSilent(false);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private void BuildStatRows()
    {
        // Destroy any existing rows
        foreach (var row in statRows)
        {
            if (row != null) Destroy(row.gameObject);
        }
        statRows.Clear();

        if (statRowPrefab == null || statRowContainer == null) return;

        // One row per stat enum value
        foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
        {
            GameObject go  = Instantiate(statRowPrefab, statRowContainer);
            var        row = go.GetComponent<LevelUpStatRowUI>();
            if (row != null)
            {
                int statLevel = LevelUpManager.Instance?.GetStatLevel(stat) ?? 0;
                int cost      = GetNextLevelCost();
                row.Setup(stat, statLevel, cost, OnLevelUpClicked);
                statRows.Add(row);
            }
        }
    }

    private void RefreshHeader()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        int playerLevel = gm.playerData.PlayerLevel;
        int cost        = GetNextLevelCost();

        if (playerLevelText   != null) playerLevelText.text   = "Level: " + playerLevel;
        if (playerSoulsText   != null) playerSoulsText.text   = "Souls: " + gm.playerData.souls;
        if (nextLevelCostText != null) nextLevelCostText.text = "Next level cost: " + cost;
    }

    private void RefreshAllRows()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        int cost = GetNextLevelCost();
        foreach (var row in statRows)
        {
            if (row == null) continue;
            int statLevel = LevelUpManager.Instance?.GetStatLevel(row.Stat) ?? 0;
            row.Refresh(statLevel, cost, gm.playerData.souls);
        }
    }

    private int GetNextLevelCost()
    {
        var gm = GameManager.Instance;
        if (gm == null || LevelUpManager.Instance == null) return 0;
        return LevelUpManager.Instance.GetSoulCostToLevelUp(gm.playerData.PlayerLevel);
    }

    private void OnLevelUpClicked(StatType stat)
    {
        var lum = LevelUpManager.Instance;
        if (lum == null) return;

        bool success = lum.TryLevelUpStat(stat);

        if (success)
        {
            if (!string.IsNullOrEmpty(levelUpSfxName))
                AudioManager.Instance?.PlaySFX(levelUpSfxName);
        }
        else
        {
            if (!string.IsNullOrEmpty(failSfxName))
                AudioManager.Instance?.PlaySFX(failSfxName);
        }
    }

    private void HandleStatLeveledUp(StatType stat, int newStatLevel, int newPlayerLevel, int remainingSouls)
    {
        RefreshHeader();
        RefreshAllRows();
    }
}
