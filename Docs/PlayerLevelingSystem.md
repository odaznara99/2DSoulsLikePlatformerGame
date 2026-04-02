# Player Leveling System — Souls of the Hollow Vale

## Overview

The leveling system is directly inspired by **Dark Souls**, where progression is measured not by a single experience-point bar, but by independently leveling individual character stats using the soul currency earned from slaying enemies. Every individual stat level-up counts as one Player Level increase.

> **Example:** You are Player Level 1. You spend souls to raise Strength by 1. You are now Player Level 2. You then raise Vitality by 1. You are now Player Level 3.

---

## The Currency: Souls

Souls are the game's primary currency. They are:

- Dropped by defeated enemies and found in the world.
- **Lost on death** — dropped at the death location as a pick-up.
- **Recoverable once** — if you reach your dropped souls before dying again, you regain them. A second death before pick-up permanently destroys them.
- Spent at a **Level-Up NPC** (e.g. a Bonfire Keeper or Firekeeper) to increase stats.

---

## Player Level

The **Player Level** is the sum of all individual stat levels (not including base levels). Starting at level 1 means no stats have been leveled yet. Each stat investment increases Player Level by 1.

```
PlayerLevel = 1 + vitality + attunement + endurance + strength
                + dexterity + resistance + intelligence + faith
```

---

## Soul Cost to Level Up

The soul cost to reach the next Player Level follows a polynomial curve, inspired by the Dark Souls 1 formula. Cost increases as the player level grows, making early levels cheap and accessible while high-level investments require substantial soul stockpiles.

### Formula

```
Cost(L) = max(100, round(0.02 × L³ + 3.06 × L² + 105.6 × L − 895))
```

Where **L** is the **next** Player Level (current + 1).

### Example costs

| Current Player Level | Next Level Cost (Souls) |
|----------------------|------------------------|
| 1                    | ~108                   |
| 5                    | ~553                   |
| 10                   | ~1,173                 |
| 20                   | ~3,324                 |
| 30                   | ~7,455                 |
| 50                   | ~21,440                |
| 100                  | ~96,275                |

> ⚠ All stat levels within a single character share the same Player Level cost. Raising Strength from 10→11 when your Player Level is 40 costs the same as raising Vitality from 8→9 at that same Player Level 40. This incentivizes diversified builds.

---

## Stats

Each stat starts at **0** (uninvested). Stats are increased one level at a time by spending souls. The table below summarises each stat, its category, and its in-game effect per level.

| Stat           | Code Field      | Category       | Effect per Level Up                                                  |
|----------------|-----------------|----------------|----------------------------------------------------------------------|
| **Vitality**   | `vitality`      | Survival       | +15 maximum HP                                                       |
| **Attunement** | `attunement`    | Magic          | +1 spell/attunement slot (future system); currently +10 max mana     |
| **Endurance**  | `endurance`     | Stamina        | +1 Stamina Level (increased capacity and regeneration rate)           |
| **Strength**   | `strength`      | Physical       | +5 attack damage                                                      |
| **Dexterity**  | `dexterity`     | Physical       | +3 attack damage, +0.1 movement speed                                |
| **Resistance** | `resistance`    | Defense        | +0.03 shield damage reduction (max 0.99)                              |
| **Intelligence**| `intelligence` | Magic          | +10 max mana; future: scales magic spell damage                       |
| **Faith**      | `faith`         | Faith / Miracles | Future: scales miracle strength; currently grants minor HP regen bonus |

### Stat soft-cap notes

Like Dark Souls, certain stats become less efficient beyond a **soft cap**:

- **Vitality, Strength, Dexterity**: soft cap at **40**, hard cap at **99** (effects per level halve above 40).
- **Endurance**: soft cap at **40** (each level above 40 adds only +0.5 stamina capacity instead of the full increase).
- **Resistance, Intelligence, Faith, Attunement**: no soft cap currently, but hard-capped at **99**.

> Soft caps are enforced inside `LevelUpManager.ApplyStat()`. Tune the values in the `LevelUpManager` Inspector-serialised fields.

---

## Level Up Interaction

Players can only level up stats by interacting with a **Level-Up NPC** (Bonfire Keeper). This mirrors the Dark Souls design where leveling is only possible at a bonfire.

### How it works

1. Player walks into the NPC trigger zone — an interaction prompt appears.
2. Player presses **E** (or equivalent on gamepad/touch).
3. The **Level Up UI** opens (game is silently paused).
4. The panel shows:
   - Current Player Level
   - Current Souls count
   - Each stat row: name, current level, and cost to level up next (+1)
   - A **"Level Up"** button per stat (greyed out if insufficient souls)
5. Player selects a stat and confirms. Souls are deducted, the stat increases, Player Level increases by 1, effects are applied immediately.
6. UI refreshes to show updated souls and levels.
7. Player closes the panel with **E** or the Close button.

---

## Effect Application

Stat increases take effect **immediately** on the live player components:

| Stat          | Target Component              | Property Modified                              |
|---------------|-------------------------------|------------------------------------------------|
| Vitality      | `PlayerHealth`                | `maxHealth` (+15, then `RefreshHealthUI()`)    |
| Attunement    | `PlayerData`                  | `mana` (+10)                                   |
| Endurance     | `PlayerStamina`               | `LevelUp(1, false)`                            |
| Strength      | `PlayerControllerVersion2`    | `attackDamage` (+5)                            |
| Dexterity     | `PlayerControllerVersion2`    | `attackDamage` (+3), `movementSpeed` (+0.1)    |
| Resistance    | `PlayerHealth`                | `shieldDamageReduction` (+0.03, clamped ≤0.99) |
| Intelligence  | `PlayerData`                  | `mana` (+10)                                   |
| Faith         | *(planned)*                   | *(reserved)*                                   |

All bonuses are **persisted** in `PlayerData` so they survive scene reloads, checkpoint restores, and save/load cycles.

---

## Data Fields (PlayerData)

```csharp
// ── Stat Levels (leveled up using souls at a Level-Up NPC) ──────────────────
public int vitality   = 0;   // governs max HP
public int attunement = 0;   // governs spell slots / mana
public int endurance  = 0;   // governs stamina level
public int strength   = 0;   // governs physical attack damage
public int dexterity  = 0;   // governs finesse damage + movement speed
public int resistance = 0;   // governs damage reduction
public int intelligence = 0; // governs magic power / mana
public int faith      = 0;   // governs faith / miracle power

// Derived: PlayerLevel = 1 + sum of all stat levels above
public int PlayerLevel => 1 + vitality + attunement + endurance
                            + strength + dexterity + resistance
                            + intelligence + faith;
```

---

## Implementation Files

| File | Purpose |
|------|---------|
| `Assets/Scripts/PlayerData.cs`     | Holds stat-level fields and `PlayerLevel` computed property |
| `Assets/Scripts/SaveData.cs`       | Serialises stat levels in `PlayerSaveData` |
| `Assets/Scripts/SaveManager.cs`    | Maps stat levels in `ToSaveData` / `FromSaveData` |
| `Assets/Scripts/LevelUpManager.cs` | Soul-cost formula, stat level-up logic, events |
| `Assets/Scripts/LevelUpUI.cs`      | UI panel — stat rows, souls display, level-up buttons |
| `Assets/Scripts/LevelUpNPC.cs`     | Trigger component — opens `LevelUpUI` on interaction |

---

## Design Notes (Dark Souls Reference)

- **No class system yet**: all characters start from the same stat base of 0. A class system (e.g., Knight starts with higher Strength; Pyromancer starts with higher Intelligence) can be added later by initialising some stat values on new game.
- **Respeccing**: Dark Souls 2 introduced Soul Vessels to reset stats. This can be added in a future update.
- **Soul memory vs. soft humanity**: For simplicity this game tracks only current souls. A "total souls spent" metric can be added later for matchmaking or NPC interactions.
- **Covenant / multiplayer considerations**: Out of scope for now.

---

## Quick Developer Reference

- **Tuning soul costs**: Adjust the polynomial coefficients in `LevelUpManager.GetSoulCostToLevelUp(int playerLevel)`.
- **Tuning stat effects**: Edit `LevelUpManager.ApplyStat(StatType stat, ...)` — each case branch controls the per-level bonuses.
- **Adding a new stat**: Add the enum value to `StatType` in `LevelUpManager.cs`, add the int field to `PlayerData`, add to `PlayerSaveData`, update `ToSaveData`/`FromSaveData`, and add a case in `ApplyStat`.
- **Level-Up NPC setup**: Add `LevelUpNPC.cs` + trigger `Collider2D` to any NPC GameObject and assign the `LevelUpUI` reference in the Inspector.
