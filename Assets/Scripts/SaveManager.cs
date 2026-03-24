using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Singleton that persists game progress (player checkpoint and per-scene object
/// states) to a JSON file in <see cref="Application.persistentDataPath"/>.
///
/// Usage
/// -----
/// • Add this component to a persistent GameObject in the first/bootstrap scene
///   (alongside GameManager and SceneLoader).
/// • Call <see cref="SaveCheckpoint"/> whenever the player activates a checkpoint.
/// • Call <see cref="SetObjectState"/> when a boss dies or a lever is toggled.
/// • Call <see cref="LoadSavedGame"/> (e.g. from a "Continue" button) to restore
///   the last checkpoint and navigate to the saved scene.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string SaveFileName = "gamesave.json";

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private GameSaveData currentSave = new GameSaveData();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFromDisk();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Returns true if a save file with at least one checkpoint exists.</summary>
    public bool HasSave() => currentSave.hasCheckpoint;

    /// <summary>
    /// Persists <paramref name="checkpointSnapshot"/> (and all currently known
    /// scene states) to the save file.  Call this from
    /// <see cref="GameManager.SaveCheckpointSnapshot"/>.
    /// </summary>
    public void SaveCheckpoint(PlayerData checkpointSnapshot)
    {
        currentSave.hasCheckpoint = true;
        currentSave.checkpointData = ToSaveData(checkpointSnapshot);
        WriteToDisk();
        Debug.Log("[SaveManager] Checkpoint saved → " + SaveFilePath);
    }

    /// <summary>
    /// Fills <paramref name="restoredData"/> with the data from the last saved
    /// checkpoint.  Returns false (and leaves the out parameter null) when no
    /// save exists.
    /// </summary>
    public bool TryLoadCheckpoint(out PlayerData restoredData)
    {
        restoredData = null;
        if (!currentSave.hasCheckpoint) return false;
        restoredData = FromSaveData(currentSave.checkpointData);
        return true;
    }

    /// <summary>
    /// Loads the saved checkpoint into <see cref="GameManager.playerData"/>,
    /// restores the checkpoint snapshot, and navigates to the saved scene.
    /// Returns false if no save exists.
    /// </summary>
    public bool LoadSavedGame()
    {
        if (!TryLoadCheckpoint(out PlayerData saved)) return false;

        GameManager.Instance.playerData.RestoreFrom(saved);
        GameManager.Instance.RestoreCheckpointSnapshot(saved);

        string scene = saved.checkpointSceneName;
        if (string.IsNullOrEmpty(scene)) return false;

        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene(scene);
        return true;
    }

    // ── Scene Object States ───────────────────────────────────────────────────

    /// <summary>
    /// Returns the persisted activation state for an object in a scene.
    /// Returns false when no state has been saved for that object.
    /// </summary>
    public bool GetObjectState(string sceneName, string objectId)
    {
        SceneSaveData sd = FindSceneData(sceneName);
        if (sd == null) return false;
        SceneObjectState obj = sd.objectStates.Find(o => o.objectId == objectId);
        return obj != null && obj.isActivated;
    }

    /// <summary>
    /// Persists the activation state for an object in a scene and immediately
    /// writes the save file.
    /// </summary>
    public void SetObjectState(string sceneName, string objectId, bool value)
    {
        SceneSaveData sd = GetOrCreateSceneData(sceneName);
        SceneObjectState obj = sd.objectStates.Find(o => o.objectId == objectId);
        if (obj == null)
        {
            obj = new SceneObjectState { objectId = objectId };
            sd.objectStates.Add(obj);
        }
        obj.isActivated = value;
        WriteToDisk();
    }

    /// <summary>Erases all save data from memory and deletes the save file.</summary>
    public void DeleteSave()
    {
        currentSave = new GameSaveData();
        if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
        Debug.Log("[SaveManager] Save data deleted.");
    }

    // ── File I/O ──────────────────────────────────────────────────────────────

    private void LoadFromDisk()
    {
        if (!File.Exists(SaveFilePath)) return;
        try
        {
            string json = File.ReadAllText(SaveFilePath);
            GameSaveData loaded = JsonUtility.FromJson<GameSaveData>(json);
            if (loaded != null) currentSave = loaded;
            Debug.Log("[SaveManager] Save loaded from " + SaveFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SaveManager] Failed to load save file: " + e.Message);
            currentSave = new GameSaveData();
        }
    }

    private void WriteToDisk()
    {
        try
        {
            string json = JsonUtility.ToJson(currentSave, true);
            File.WriteAllText(SaveFilePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SaveManager] Failed to write save file: " + e.Message);
        }
    }

    // ── Conversion Helpers ────────────────────────────────────────────────────

    private PlayerSaveData ToSaveData(PlayerData data)
    {
        var s = new PlayerSaveData
        {
            currentHealth        = data.currentHealth,
            maxHealth            = data.maxHealth,
            mana                 = data.mana,
            coins                = data.coins,
            xp                   = data.xp,
            souls                = data.souls,
            droppedSouls         = data.droppedSouls,
            droppedSoulsX        = data.droppedSoulsPosition.x,
            droppedSoulsY        = data.droppedSoulsPosition.y,
            posX                 = data.position.x,
            posY                 = data.position.y,
            checkpointSceneName  = data.checkpointSceneName,
            memoryShards         = data.memoryShards,
            bonusMaxHealth       = data.bonusMaxHealth,
            bonusDamageReduction = data.bonusDamageReduction,
            bonusJumpCount       = data.bonusJumpCount,
            bonusMovementSpeed   = data.bonusMovementSpeed,
            staminaRelicLevel    = data.staminaRelicLevel,
        };
        s.unlockedPassives = data.unlockedPassives.ConvertAll(p => (int)p);
        return s;
    }

    private PlayerData FromSaveData(PlayerSaveData s)
    {
        var data = new PlayerData
        {
            currentHealth        = s.currentHealth,
            maxHealth            = s.maxHealth,
            mana                 = s.mana,
            coins                = s.coins,
            xp                   = s.xp,
            souls                = s.souls,
            droppedSouls         = s.droppedSouls,
            droppedSoulsPosition = new Vector2(s.droppedSoulsX, s.droppedSoulsY),
            position             = new Vector2(s.posX, s.posY),
            checkpointSceneName  = s.checkpointSceneName,
            memoryShards         = s.memoryShards,
            bonusMaxHealth       = s.bonusMaxHealth,
            bonusDamageReduction = s.bonusDamageReduction,
            bonusJumpCount       = s.bonusJumpCount,
            bonusMovementSpeed   = s.bonusMovementSpeed,
            staminaRelicLevel    = s.staminaRelicLevel,
        };
        data.unlockedPassives = s.unlockedPassives.ConvertAll(i => (PassiveAbility)i);
        return data;
    }

    private SceneSaveData FindSceneData(string sceneName)
        => currentSave.sceneStates.Find(s => s.sceneName == sceneName);

    private SceneSaveData GetOrCreateSceneData(string sceneName)
    {
        SceneSaveData sd = FindSceneData(sceneName);
        if (sd == null)
        {
            sd = new SceneSaveData { sceneName = sceneName };
            currentSave.sceneStates.Add(sd);
        }
        return sd;
    }
}
