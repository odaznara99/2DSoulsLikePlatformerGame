# Enemy Reference & Design — Souls of the Hollow Vale

This document lists currently implemented enemies, summarizes their behaviors and tuning points, and provides suggestions and new enemy ideas to expand combat variety.

## Current enemies (implemented)
### Bandit
- Implementation: `Assets/Scripts/Bandit.cs`
- Behavior summary:
  - States: `Idle`, `Patrol`, `Chase`, `Jump`, `Attack`, `Hurt`, `Dead`.
  - Chases the player when within `followRange`, attacks when inside `attackRange`.
  - Uses simple horizontal chase (preserves vertical velocity) and can jump when wall or gap is detected.
  - Supports knockback, floating damage text, and a floating health bar.
  - Signals: plays SFX on damage/death, spawns blood splash on hit.
- Key tuning fields (in inspector / script):
  - `moveSpeed`, `followRange`, `attackRange`, `attackCooldown`, `attackTiming`
  - `damage`, `maxHealth`, `knockbackResistance`, `knockbackDuration`
  - Jump: `jumpForce`, `jumpHorizontalSpeed`, `jumpCooldown`, sensor `checkDistance`
- Related systems:
  - Player references: `PlayerControllerVersion2` and `PlayerHealth`.
  - Spawning / management: `EnemyManager` (register/unregister), `EnemySpawner.cs`.
  - UI: `FloatingHealthbar`, `FloatingText`, `StaminaUI` (for player feedback).

## Implementation notes (quick)
- Attack timing uses a coroutine `AttackState()` that waits `attackTiming` seconds then applies damage if target remains in `attackRange`.
- Hurt flow reduces `currentHealth`, updates the health bar and triggers `Dead` state if <= 0.
- Patrol/Idle are placeholders — they can be extended for waypoints or timed roaming.
- Collision between enemies is ignored with `Physics2D.IgnoreLayerCollision(6,6)`.

## Suggested improvements (short)
- Decouple direct `GameObject.Find` calls: inject player and world-canvas references from spawner or manager to avoid runtime lookups.
- Replace hard-coded strings for SFX with an enum or audio ID table to avoid runtime errors.
- Move tuning constants to a ScriptableObject `EnemyStats` for easy balancing without prefab modifications.
- Add configurable waypoint patrol (small `PatrolPath` component) and a simple state-entry timer to avoid jittery switching.
- Add stun/interrupt windows on heavy hits and expose a damage reaction curve for knockback.

## Enemy ideas to develop
- Echo Scout
  - Fast, low-health enemy that telegraphs a short dash and records last player action to mirror it after a delay.
  - Purpose: teach players to vary attack patterns.

- Forgebound Grunt
  - Heavier, armored melee unit that reduces incoming knockback and has a slowed charge attack breaking player guard.
  - Drops small stamina relic on defeat.

- Lantern Wisp
  - Flying, fragile enemy that circles lanterns and can revive extinguished light if not dealt with. Emits low light and distracts player during platforming.

- Memory Shade
  - Phase-in enemy that becomes intangible periodically; attacks during the tangible window. Defeating it yields memory shard fragments.

- Tethered Forgemender (mini-boss)
  - Stationary hazard/mini-boss that summons mechanical echoes and repairs armor on nearby forgebound units unless its heat core is exposed (platforming puzzle to reach core).

- Echo Duelist (advanced)
  - Mimics the player's last two actions (jump + most recent attack) and punishes repetitive play. Higher skill ceiling, good for optional encounters.

- Corrupted Child-Echo
  - Low hp but erratic movement and a scream that debuffs player stamina regen in a radius — forces focus-target prioritization.

## Design guidance
- Mix movement archetypes (flying, tethered, leaper, charger) with interaction archetypes (mimic, buffer, debuffer, spawner).
- Use small, readable telegraphs before high-damage moves.
- Let enemies teach mechanics: early-level Echo Scouts encourage variation; later Memory Shades teach timing and windowed DPS.
- Keep tuning variables exposed and document which are safe to modify at runtime.

## Where to look in code
- `Assets/Scripts/Bandit.cs` — current melee enemy.
- `Assets/Scripts/EnemySpawner.cs`, `Assets/Scripts/EnemyManager.cs` — spawning & registration.
- `Assets/Scripts/EnemyHealth.cs`, `Assets/Scripts/EnemyAttack.cs` — shared health/attack helpers.
- `Assets/Scripts/BossAI.cs` — reference for more complex encounter patterns.
