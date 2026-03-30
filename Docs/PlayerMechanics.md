# Player Mechanics — Souls of the Hollow Vale

This document summarizes the player's gameplay mechanics and how they integrate with systems in the project. Use this as a reference for design, tuning, and quick navigation to implementation files.

## Overview
The player (the Wanderer) is driven by two core resources: Health and Stamina. Health represents survivability; Stamina is the primary resource for movement, attacks, and special actions. Gameplay emphasizes risk vs. reward: aggressive or mobile play consumes stamina and opens the player to danger, while careful pacing and use of anchors/checkpoints enable progress.

## Core stats
- Health
  - Managed by `PlayerHealth.cs`.
  - Damage from enemies, hazards and some environment events reduces health.
  - Health reaches zero causes player death and respawn behavior handled by `GameManager.cs` / `PlayerPositionRestorer.cs`.

- Stamina
  - Managed by `PlayerStamina.cs`.
  - Consumed by actions (attacks, sprint/dash, heavy moves).
  - Regenerates over time when not performing stamina-costing actions.
  - Stamina UI is displayed by `StaminaUI.cs`.

## Player actions
- Movement & Jumping
  - Standard 2D platformer movement, including horizontal movement and jumping.
  - Movement and jump stamina costs (if any) are implemented in the player controller scripts (`PlayerControllerVersion2.cs` and/or `PlayerController.cs`).

- Attacking
  - Light and (optionally) heavy attacks consume stamina.
  - Attack animations and hit detection are handled in the player controller / combat scripts.

- Plunge Attack *(Unlockable Air Ability)*
  - Activated while **airborne** by pressing **Attack + Down** direction.
  - Slams the player sharply downward, dealing area-of-effect damage and knockback to all enemies within the landing radius.
  - Consumes a large amount of stamina (default 40) as a drawback.
  - Has a dedicated cooldown (default 5 seconds) before it can be used again.
  - Unlocked as a **Passive Ability** (`PlungeAttack`) via a Memory Shard pickup.
  - Tunable parameters in `PlayerControllerVersion2.cs`: `plungeAttackDamage`, `plungeDownForce`, `plungeAttackRadius`, `plungeKnockbackForce`, `plungeStaminaCost`, `plungeCooldown`.
  - Requires a `"PlungeAttack"` trigger wired in the Player Animator for visual feedback.

- Dash / Sprint / Special Mobility
  - Mobility abilities consume a chunk of stamina and have short recovery windows.
  - These are used for traversal and defensive maneuvers; their tuning lives in the controller and stamina scripts.

- Defensive Actions
  - Blocking/parry (if implemented) consumes or depends on stamina; check controller and combat systems for behavior.

## Recovery, Rest & Anchors
- Stamina Regeneration
  - Stamina regens passively when not consuming stamina. Regeneration rates and delays are in `PlayerStamina.cs`.

- Anchors / Checkpoints (bonfire-like)
  - Death returns the player to a safe anchor. Collected persistent progress (memory shards, relics) is retained.
  - Position restoration and respawn logic are implemented in `PlayerPositionRestorer.cs` and `GameManager.cs`.

## Death and consequences
- On death:
  - The player is respawned at the most recent anchor / checkpoint.
  - Memory shards and collected upgrades persist across death, but the world may shift slightly (narrative mechanic).
  - Any death-related UI or flows are handled by `GameManager.cs` and relevant UI scripts.

## Pickups and upgrades
- Memory Shards
  - Collectible narrative items that unlock passive abilities, UI changes, or map shifts.
  - Influence both story and mechanical progression.

- Stamina Relics
  - Rare pickups that temporarily or permanently modify stamina behavior (capacity, regen, special effects).
  - These are useful for traversal puzzles and boss interactions.

- Other pick-ups
  - Health items or vitality shards may exist and are handled by pickup or inventory scripts.
 
- Souls pickup
  - Main currency that can use to level up permanent stats of the character, buy permanent weapon and armor in shops, or spells and even special skills. Will reset to zero upon death and leave the souls to the death place. Can also be pick-up again, but if you did not pick it up before the second death, it will disappear.
 
- Coins pickup
  - Another currency, mainly for consumable items, weapons, armors, but only temporary. All item bought by coins will be lose upon death.

## Enemy & Encounter interactions
- Echo Ambushes
  - Enemies that mimic recent player actions to force adaptation (design mechanic).
  - Consider testing players’ attack patterns vs. mimic behavior.

- Boss interactions
  - Boss health behavior and UI uses `BossHealthUI.cs`. Boss AI scripts (e.g., `BossAI.cs`) drive encounter rules and stamina/health interactions.

## Level integration
- Memory Rifts
  - Optional rooms or zones that trigger flashbacks and may change platform layouts or player abilities while inside.
  - Use these to gate optional content and grant small mechanical changes.

- Environmental hazards
  - Standard platformer hazards that damage health or affect movement/stamina.

## UI
- StaminaUI
  - Display and feedback for stamina state: current value, regen, exhaustion.
  - Implemented in `StaminaUI.cs`.

- Health UI & Boss UI
  - Player health and boss health displays live in `PlayerHealth.cs` (data) and `BossHealthUI.cs` (boss HUD).

- Audio & Settings
  - Tuning and feedback for audio cues tied to stamina/health events use `AudioSettingsUI.cs` and project audio assets.

## Developer notes / where to change things
- Player controller behaviour: `Assets/Scripts/PlayerControllerVersion2.cs` and `Assets/Scripts/PlayerController.cs`.
- Stamina system: `Assets/Scripts/PlayerStamina.cs`.
- Health system: `Assets/Scripts/PlayerHealth.cs`.
- UI: `Assets/Scripts/StaminaUI.cs`, `Assets/Scripts/BossHealthUI.cs`.
- Respawn / anchors: `Assets/Scripts/GameManager.cs`, `Assets/Scripts/PlayerPositionRestorer.cs`.
- Scene loading and transitions: `Assets/Scripts/SceneLoader.cs`.
- Souls pickup: `Assets/Scripts/SoulsPickup.cs` — collectible souls in the world.
- Coins pickup: `Assets/Scripts/CoinsPickup.cs` — collectible coins in the world.
- Dropped souls pickup: `Assets/Scripts/DroppedSoulsPickup.cs` — spawned at death position; configure `GameManager.droppedSoulsPrefab` in the Inspector.
- Currency data & death reset logic: `Assets/Scripts/PlayerData.cs` (`souls`, `coins`, `droppedSouls`), `Assets/Scripts/GameManager.cs` (`NotifyPlayerDeath`, `SpawnDroppedSoulsIfAny`, `ClearDroppedSoulsPickup`).

When tuning gameplay, adjust values in `PlayerStamina.cs` (capacity, regen rate, cooldown/delay), `PlayerHealth.cs` (max health, invulnerability windows), and the controller scripts for stamina costs per action.

## Design recommendations
- Keep stamina costs visible and consistent — players should understand cost vs. reward from the UI and audio cues.
- Use memory shards to pace power growth; avoid letting shards trivialize core traversal early.
- Make anchors meaningful trade-offs: safe progress with structural narrative/upgrade consequences.
- Test echo enemies against common player behaviors to ensure they force adaptation without feeling unfair.

## Quick links
- Design story / narrative context: `Docs/GameStory.md`
- Mechanics implementation: open `Assets/Scripts/PlayerStamina.cs`, `Assets/Scripts/PlayerHealth.cs`, `Assets/Scripts/PlayerControllerVersion2.cs`, `Assets/Scripts/StaminaUI.cs`.
- Currency pickups: `Assets/Scripts/SoulsPickup.cs`, `Assets/Scripts/CoinsPickup.cs`, `Assets/Scripts/DroppedSoulsPickup.cs`.
