# **Souls of the Hollow Vale**

![License](https://img.shields.io/badge/license-MIT-green)
![Platform](https://img.shields.io/badge/platform-Unity-blue)

## **Table of Contents**

- [**Game Synopsis**](#game-synopsis)
- [**Game Mechanics**](#game-mechanics)
- [**Technical Overview**](#technical-overview)
- [**Installation**](#installation)
- [**How to Play**](#how-to-play)
- [**Contributing**](#contributing)
- [**License**](#license)
- [**Contact**](#contact)

## **Game Synopsis**

**Souls of the Hollow Vale** is a 2D platformer Soulslike about a nameless wanderer drawn into a ruined valley where memory and sorrow shape the land. The player's journey is one of reclamation: recovering lost shards of identity, confronting echo-spirits of past heroes, and breaking a cycle of decay that feeds on willpower and stamina.

### **Setting**

The Hollow Vale was once a prosperous chain of villages and sky-bridges suspended over an ancient chasm. After the Sundering — an event that fractured reality and tethered memories to the land — the Vale became a place where echoes of the past roam. Architecture folds on itself, gravity quirks on ledges, and relics hum with fragments of those who fell before.

- **Visual tone**: weathered stone, tattered banners, spectral light, moss-grown gears.
- **Atmosphere**: heavy, melancholic, intimate — a quiet that presses on the player between sudden dangers.

### **Protagonist**

The **Wanderer** — a silent, mysterious figure awoken at the Vale's edge with a fractured memory and a single dim relic: the Ember Sigil. Their motivation is to recover identity fragments and either escape the Vale or become its new anchor.

### **Core Antagonist**

The **Hollow Sovereign** — a being formed from the Vale's collected regrets and unfinished wills. Not a singular villain in the cinematic sense; it acts through corrupted champions and bound echoes. Defeating it requires understanding why the Vale clings to sorrow.

### **Story Beats**

1. **Awakening at the Vale's Threshold** — intro platforming sequence teaching stamina and recovery basics.
2. **The Village of Lost Lanterns** — meet Lira, learn about identity shards, retrieve the first shard.
3. **The Sky-Bridge Trial** — vertical gauntlet where environment and memory interact.
4. **The Forgebound Foundry** — mid-act boss that hoards stamina-imbuing relics.
5. **The Echo Hall** — narrative hub where recovered memories rewrite sections of the map.
6. **The Hollow Sovereign's Antechamber** — revelations about cycles of sacrifice and a player choice.
7. **Multiple Endings** — reclaim identity and free the Vale, bind yourself to hold it together, or succumb and become another echo.

### **Themes**

- **Memory vs. Identity**: each recovered shard reshapes who the Wanderer is and how NPCs treat them.
- **Sacrifice and Persistence**: progress requires risk; death is a lesson rather than failure.
- **Quiet grief and small hope**: storytelling through environment, losses, and sparse, meaningful dialogue.

---

## **Game Mechanics**

The player is driven by two core resources: **Health** and **Stamina**. Gameplay emphasizes risk vs. reward — aggressive or mobile play consumes stamina and opens the player to danger, while careful pacing and use of anchors enables progress.

### **Core Stats**

| Stat | Description |
|------|-------------|
| **Health** | Represents survivability. Reaching zero triggers death and respawn. |
| **Stamina** | Primary resource for attacks, dashes, and special actions. Regenerates passively when idle. |

### **Player Actions**

- **Movement & Jumping** — standard 2D platformer movement with horizontal movement and jumping.
- **Light Attack** — fast melee strike; consumes a small amount of stamina.
- **Heavy Attack** — slower, high-damage strike; consumes more stamina.
- **Dash / Sprint** — short burst of speed for traversal and dodging; costs a chunk of stamina.
- **Block / Parry** — defensive stance that reduces incoming damage at the cost of stamina.
- **Plunge Attack** *(Unlockable)* — activated while airborne by pressing **attack button + down**. Slams downward with AoE damage and knockback; costs 40 stamina with a 5-second cooldown. Unlocked via Memory Shard.

### **Pickups & Upgrades**

| Pickup | Effect |
|--------|--------|
| **Memory Shards** | Unlock passive abilities or alter UI/map elements. |
| **Stamina Relics** | Temporarily or permanently modify stamina capacity, regen, or special behaviour. |
| **Health Pickups** | Restore a portion of the player's health. |
| **Souls** | Main currency. Used to buy permanent upgrades and gear from shops. Dropped at the death location on death; lost if not recovered before the next death. |
| **Coins** | Secondary currency for consumables. Lost on death. |

### **Death & Progression**

- On death, the player respawns at the last checkpoint/anchor.
- **Souls** are dropped at the death location and can be recovered; a second death before recovery destroys them.
- Memory shards and purchased upgrades persist across deaths.

### **Enemies**

| Enemy | Behaviour |
|-------|-----------|
| **Bandit** | Patrols, chases the player on sight, attacks at melee range, can jump gaps. Supports knockback and floating health bars. |
| **Boss** | Complex multi-phase AI (`BossAI.cs`) with dedicated health UI and zone-based camera triggers. |

---

## **Technical Overview**

Below is a summary of the major systems and scripts implemented in this project.

### **Player Systems**

| Script | Purpose |
|--------|---------|
| `PlayerControllerVersion2.cs` | Core player movement, jumping, attacking, plunge attack, and input handling. |
| `PlayerHealth.cs` | Health tracking, damage application, invulnerability windows. |
| `PlayerStamina.cs` | Stamina capacity, consumption, regeneration rate and delay. |
| `PlayerData.cs` | Central data store for health, stamina, souls, coins, unlocked passives, and purchased items. |
| `StaminaUI.cs` | HUD element displaying current stamina state. |

### **Save System**

- `SaveManager.cs` — `DontDestroyOnLoad` singleton; reads/writes `gamesave.json` in `Application.persistentDataPath`.
- `SaveData.cs` — serializable data class holding all persistent player and world state.
- `GameManager.SaveCheckpointSnapshot()` — call to persist the current state at a checkpoint.
- `BossAI.cs` and `LeverGateController.cs` use a `persistentId` string (set in the Inspector) to opt in to per-object save state.

### **Currency & Death System**

- **Souls** — main currency tracked in `PlayerData.souls`. On death: value drops to 0, a `DroppedSoulsPickup` prefab is spawned at the death position.
- **Coins** — temporary currency tracked in `PlayerData.coins`. Cleared on death; items bought with coins are also lost.
- Key methods in `GameManager.cs`: `NotifyPlayerDeath()`, `SpawnDroppedSoulsIfAny()`, `ClearDroppedSoulsPickup()`.

### **Shop System**

Fully data-driven shops built on ScriptableObjects.

| Script / Asset | Role |
|----------------|------|
| `ShopItem.cs` | ScriptableObject defining an item (id, name, price, currency, effect). |
| `ShopInventory.cs` | ScriptableObject holding a list of ShopItems for one shop. |
| `ShopNPC.cs` | Trigger component placed on an NPC; opens the shop on player interaction. |
| `ShopUI.cs` | Panel logic — displays items, handles buy actions, updates currency display. |
| `ShopItemSlotUI.cs` | Per-slot prefab component for the item grid. |

**Create a shop**: `Assets → Create → Shop → Shop Item` / `Shop Inventory`.

### **Dialogue System**

- `DialogueLine.cs` / `DialogueData.cs` — ScriptableObject data layer (`Assets → Create → Dialogue`).
- `DialogueManager.cs` — `DontDestroyOnLoad` singleton with typewriter effect (uses `maxVisibleCharacters`).
- `DialogueTrigger.cs` — NPC component that starts a dialogue sequence.
- `GameManager.PauseSilent(bool)` — pauses gameplay without showing the pause screen (used during dialogue and shop interactions).

### **Abilities System**

Unlockable passive abilities are stored in the `PassiveAbility` enum (`PlayerData.cs`).

- Granted by `MemoryShardPickup.ApplyPassive()`.
- Restored from `playerData.unlockedPassives` in `PlayerControllerVersion2.Start()`.
- Persisted automatically via the `unlockedPassives` list in `SaveData`.

Currently implemented passives: `PlungeAttack` (AoE slam), `ExtraJump`, `FasterStaminaRegen`, `IncreaseMaxHealth`, `IncreaseDamage`, `IncreaseDamageReduction`, `IncreaseMovementSpeed`.

### **Enemy System**

| Script | Purpose |
|--------|---------|
| `Bandit.cs` | Melee enemy with Idle / Patrol / Chase / Jump / Attack / Hurt / Dead states. |
| `EnemyHealth.cs` | Shared health component with floating damage text and health bar. |
| `EnemyAttack.cs` | Shared attack helper. |
| `EnemySpawner.cs` / `EnemyManager.cs` | Spawning and registration of enemies. |
| `BossAI.cs` | Complex boss encounter with persistent state via `persistentId`. |
| `BossHealthUI.cs` | Boss-specific HUD health bar. |

### **Level & Scene Systems**

| Script | Purpose |
|--------|---------|
| `SegmentManager.cs` | Spawns modular level segments using `startPoint` / `endPoint` anchors. |
| `SceneLoader.cs` | Handles scene transitions and loading. |
| `CheckpointTrigger.cs` | Activates a checkpoint and saves player state. |
| `PlayerPositionRestorer.cs` | Restores the player to the last saved anchor on respawn. |
| `LeverGateController.cs` | Persistent lever-gate state (uses `persistentId`). |

### **UI & Audio**

| Script | Purpose |
|--------|---------|
| `HUDManager.cs` / `UIScreensManager.cs` | Manages HUD elements and screen overlays. |
| `BossHealthUI.cs` | Boss health bar HUD. |
| `AudioManager.cs` / `CharacterAudio.cs` | Central audio playback and per-character SFX. |
| `AudioSettingsUI.cs` | In-game audio settings panel. |

---

## **Installation**

### **Prerequisites**

- **Unity**: [Download Unity](https://unity.com/download) (Unity 2021 or higher recommended)
- **Git**: [Download Git](https://git-scm.com/downloads)

### **Steps**

1. Clone the repository:

    ```bash
    git clone https://github.com/odaznara99/2DSoulsLikePlatformerGame.git
    ```

2. Open the project in Unity:

    - Open Unity Hub
    - Click **Add**
    - Navigate to the cloned folder and select it

3. Press the **Play** button in Unity to start the game.

### **Building for Android (Optional)**

1. In Unity, go to `File → Build Settings`.
2. Select **Android** as the platform and click **Switch Platform**.
3. Ensure the Android SDK, NDK, and JDK are installed (Unity Hub can manage these).
4. Click **Build and Run**.

---

## **How to Play**

| Action | Input |
|--------|-------|
| Move left / right | `A` / `D` or arrow keys |
| Jump | `Spacebar` |
| Light attack | `J` (or configured attack button) |
| Heavy attack | `K` (or configured heavy button) |
| Dash | `Shift` |
| Block | `L` (or configured block button) |
| Plunge attack *(unlocked)* | Jump, then **attack + down** |
| Interact / talk to NPC | `E` (near an NPC or shop) |

- Defeating enemies drops **Souls**. Spend them at shops to permanently upgrade your character.
- Collect **Memory Shards** to unlock new passive abilities.
- Rest at **Checkpoints** to save your progress.

---

## **Contributing**

We welcome contributions! Please read [CONTRIBUTING.md](./CONTRIBUTING.md) for development standards, branching workflow, coding conventions, and PR guidelines.

---

## **License**

This project is licensed under the MIT License — see the [LICENSE](./LICENSE) file for details.

## **Contact**

If you have any questions, feel free to reach out!

- **GitHub**: [odaznara99](https://github.com/odaznara99)
- **Email**: jayrussell.aranzado@gmail.com
