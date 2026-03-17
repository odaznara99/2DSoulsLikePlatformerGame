# Stages & Levels — Souls of the Hollow Vale

This document records the game's current stage/level structure, implementation notes, recommended new stages to build, and coding best practices for level/scene work.

## Current stages (design + implementation notes)
The project uses two complementary approaches to level content:
- Hand-authored scenes (story beats / hubs / boss rooms)
- Modular, segment-based streaming for longer traversal sequences (see `SegmentManager.cs`)

Design-level stage list (from `Docs/GameStory.md`) — treat these as canonical stage designs to implement/test:
1. Awakening at the Vale’s Threshold — intro tutor (stamina + recovery).
2. The Village of Lost Lanterns — early hub, NPCs (Lira), first memory shard.
3. The Sky‑Bridge Trial — vertical gauntlet; memory-interacting geometry.
4. The Forgebound Foundry — mid-act combat arena + mid-boss.
5. The Echo Hall — narrative hub where map tiles can change.
6. The Hollow Sovereign’s Antechamber — final chamber with choice-driven ending.
7. Lantern Bridge (example scene) — scripted encounter with lantern interactions.

Implementation notes (where to look)
- Modular segment streaming: `Assets/Scripts/SegmentManager.cs` — spawns `segmentPrefabs` using `startPoint`/`endPoint`. Useful for long, repeatable traversal (bridge, sky-bridge, corridor).
- Scene / level flow: `Assets/Scripts/SceneLoader.cs` and `Assets/Scripts/TestSceneManager.cs` (test harness).
- Spawners & placement: `Assets/Scripts/EnemySpawner.cs`, `Assets/Scripts/Segment` (segment prefab components).
- Respawn / anchors: `PlayerPositionRestorer.cs`, `GameManager.cs`.
- Recommend: open Unity Build Settings to enumerate exact scenes included in builds and add a small README in each Scene folder documenting purpose and linked prefabs.

If you need a quick inventory of actual scene files included in the current project, I can list Build Settings or scan Assets/Scenes for you.

## Suggested stages to develop (short list + mechanics)
- Memory Rift Chambers
  - Optional puzzle rooms that alter memory-linked geometry and reward shards.
- Lantern Bridge (full implementation)
  - Multi-stage encounter: restore lanterns → song fragments → shadow-echo duel.
- Forgebound Foundry variants
  - Conveyor/platform hazards, heat-core exposes, mini-forgebound groups with staggered spawns.
- Sky‑Bridge Sequels
  - Procedural segments + wind mechanics; falling-risk traversal that rewards precision.
- Echo Hall Extensions
  - Rooms that replay player actions (record/replay echo) to change puzzle/enemy layout.
- Small hubs
  - Village courtyard (shops/upgrade NPC), Echo shrine (choice-based upgrades).
- Optional Challenge Arenas
  - Timed rooms that modify stamina rules (low regen, stamina-relay relics) — good for testing and achievements.
- Environmental transition levels
  - E.g., "Moss Gears" — platforming with moving gears that alter gravity/quasi-rotation.

## Level-design tips & heuristics
- Use telegraphs for high-damage obstacles/enemies (audio + particle + animation).
- Place save/anchor points to create meaningful risk: distance between anchors should balance frustration vs. reward.
- Use Memory Shards to gate progression — avoid letting early shards trivialize traversal.
- Build segments modularly: each `Segment` prefab should be self-contained (entry/exit anchors, expected camera bounds, spawn points).
- Keep scene lighting and post-processing consistent across connected segments to avoid visual pops.

## Coding best practices for levels & scenes
Follow project CONTRIBUTING.md and general Unity/C# best practices:

- Scene & asset organization
  - One clear purpose per scene (e.g., Hub, Combat Arena, SegmentSet).
  - Add a short `README.md` next to complex scene prefabs explaining expected uses and required child objects (startPoint, endPoint, spawn anchors).

- Avoid runtime Find calls
  - Do not rely on `GameObject.Find` or string lookups in `Start()`/`Update()`. Inject references from spawners/managers or use serialized fields. (See `Bandit.cs` for candidates to refactor.)

- Use ScriptableObjects for tuning
  - Store per-enemy, per-segment, and per-stage constants in `ScriptableObject` assets for live balancing and prefab reuse.

- Decouple systems
  - Keep Scene/Segment loading separate from gameplay logic. Let `SceneLoader` and `SegmentManager` handle placement; let enemy/encounter scripts focus on behavior.

- Pooling & performance
  - Pool frequently spawned objects (enemies, projectiles, VFX) to reduce GC and instantiation spikes, especially during segment transitions.

- Scene loading & state
  - Centralize persistent state (player upgrades, memory shards, anchors) in `GameManager` or a dedicated `PersistentState` object that survives scene loads.

- Naming & structure
  - Match naming conventions in CONTRIBUTING.md (PascalCase for classes, clear prefab names, use folders per feature).
  - Add comments to custom components describing required child objects (e.g., `Segment.startPoint`, `Segment.endPoint`).

- Tests & playtesting
  - Keep small, focused test scenes (see `TestSceneManager.cs`) for iteration.
  - Run automated unit tests where feasible (pure logic, spawn rules, state machines).

- Source control & PR workflow
  - Follow the repo workflow: feature branches off `master` (e.g., `feature/sky-bridge`), small PRs with one logical change, and include playtesting notes and screenshots for level work.

## Quick checklist before merging a new stage
- [ ] Scene added to Build Settings and documented.
- [ ] Prefabs and segments referenced via serialized fields (no runtime Find).
- [ ] All tuning exposed in ScriptableObjects or inspector.
- [ ] Enemy spawn points use `EnemySpawner` and pooling.
- [ ] Anchors / respawn tested; player state persists correctly.
- [ ] Playtest notes and screenshots included in PR.
---