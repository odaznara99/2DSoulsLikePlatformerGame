using System.Collections.Generic;

// ── Persistent data classes used by SaveManager ──────────────────────────────
// All classes are marked [System.Serializable] so Unity's JsonUtility can
// read and write them to the local save file.

[System.Serializable]
public class PlayerSaveData
{
    public float  currentHealth;
    public float  maxHealth;
    public int    mana;
    public int    coins;
    public int    xp;
    public int    souls;
    public int    droppedSouls;
    public float  droppedSoulsX;
    public float  droppedSoulsY;
    public float  posX;
    public float  posY;
    public string checkpointSceneName;
    public int    memoryShards;
    public List<int> unlockedPassives = new List<int>(); // PassiveAbility cast to int
    public float  bonusMaxHealth;
    public float  bonusDamageReduction;
    public int    bonusJumpCount;
    public float  bonusMovementSpeed;
    public int    staminaRelicLevel;
    public float  bonusAttackDamage;
    public List<string> purchasedItemIds = new List<string>();
}

/// <summary>
/// Tracks the persisted state of a single named object within a scene
/// (e.g. whether a boss is defeated or a lever/gate is open).
/// </summary>
[System.Serializable]
public class SceneObjectState
{
    public string objectId;

    /// <summary>
    /// Generic flag: true = "activated" (boss defeated, gate open, etc.).
    /// </summary>
    public bool isActivated;
}

/// <summary>
/// Holds the collection of object states for one scene.
/// </summary>
[System.Serializable]
public class SceneSaveData
{
    public string sceneName;
    public List<SceneObjectState> objectStates = new List<SceneObjectState>();
}

/// <summary>
/// Root save-file structure written to disk as JSON.
/// </summary>
[System.Serializable]
public class GameSaveData
{
    /// <summary>True once the player has reached at least one checkpoint.</summary>
    public bool hasCheckpoint;

    /// <summary>Snapshot of the player's state at the last activated checkpoint.</summary>
    public PlayerSaveData checkpointData = new PlayerSaveData();

    /// <summary>Per-scene object states (boss defeats, gate positions, etc.).</summary>
    public List<SceneSaveData> sceneStates = new List<SceneSaveData>();
}
