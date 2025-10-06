# LD58

A Unity 2D prototype where you captain a modular space train through an auto-scrolling sector full of random encounters. The focus is on escorting an ever-growing convoy of train cars while balancing risk, repairs, and taxation as difficulty ramps up over time.

## Table of contents
- [Gameplay overview](#gameplay-overview)
- [Controls](#controls)
- [Project structure](#project-structure)
- [Systems breakdown](#systems-breakdown)
- [Unity version & external packages](#unity-version--external-packages)
- [Getting started](#getting-started)
- [Development tips](#development-tips)

## Gameplay overview
- **Objective.** Survive until the encounter timeline is exhausted while keeping at least one locomotive module alive; finishing the schedule dispatches a win signal, while losing the last locomotive triggers a defeat.
- **Modular train.** The player ship is assembled at runtime from module configs (locomotive, cargo, turret). Modules can be added or removed, and cargo modules award score when delivered.
- **Encounter loop.** A timeline of encounters (asteroid fields, pirate raids, traders, tax stations) is either handcrafted or procedurally generated; each encounter spawns prefabs via the pooled factory and scales with the current difficulty value.
- **Scoring & rating.** Selling cargo at tax stations increments the score, which is then converted into an end-of-run rating when the finish UI appears.

## Controls
- **Movement:** Arrow keys or WASD (Unity's default `Horizontal` / `Vertical` axes) steer the leading locomotive.
- **Restart:** Press `R` after the victory/defeat screen to reload the scene.
- **Experimental fire:** `Space` currently only logs a debug message and can be extended with real weapon logic.

## Project structure
```
Assets/
├── _Scripts/             # Gameplay, ship, DI, utility logic
├── _Prefabs/             # Train modules, encounters, pooled prefabs
├── _Sprites/             # 2D art used by ships and encounters
├── Scenes/MainScene      # Primary gameplay scene
├── Audio & Fonts         # Supporting assets
└── Settings/             # ScriptableObject configs (modules, etc.)
```
Key namespaces mirror the folder layout (`_Scripts.Game`, `_Scripts.Ships`, `_Scripts.Static`, etc.) to keep feature areas isolated.

## Systems breakdown
- **Game flow controller.** Tracks elapsed time, difficulty progression, and triggers encounters based on a timeline that can be authored in the inspector or generated procedurally for replayability.
- **Dependency injection.** The scene-level `DiInstaller` wires Zenject bindings for the prefab pool, score service, module registry, random service, and other shared dependencies.
- **Prefab pooling.** Encounters spawn ships, asteroids, and stations through a pooled factory (`IPrefabPool`) to avoid runtime allocation spikes.
- **Signals.** Game completion and asteroid destruction events use `SignalsHub` to loosely couple UI and gameplay responses.
- **AI controllers.** Trader and pirate trains assemble their own ship configurations and run simple steering behaviours to interact with the player or world elements.

## Unity version & external packages
- **Unity Editor:** 6000.0.25f1 (Unity 6).
- **Packages / git submodules:**
  - [Audio-Tools](https://github.com/Nebulate-me/Audio-Tools.git)
  - [Behavior-Tree](https://github.com/Nebulate-me/Behavior-Tree.git)
  - [DI-Tools](https://github.com/Nebulate-me/DI-Tools.git)
  - [SignalsHub](https://github.com/Nebulate-me/SignalsHub.git)
  - [Unity-Utilities](https://github.com/Nebulate-me/Unity-Utilities.git)

## Getting started
1. Install the Unity version listed above (or a compatible 6.x editor).
2. Clone the repository and its dependencies (`git clone --recursive`).
3. Open the project in Unity Hub; the default scene is **`Assets/Scenes/MainScene.unity`**.
4. Press **Play** to spawn the player train and begin the encounter timeline. The DI installer will automatically set up services when the scene loads.

## Development tips
- Module stats and prefabs live under `Assets/Settings/` (`ModuleConfig` assets). Update health, scoring, or prefab references there without touching code.
- Extend encounters by adding new `EncounterType` values and handling them in `GameFlowController.TriggerEncounter` with matching prefabs.
- Ship behaviour is driven by `TrainController` plus specific module scripts (e.g., `CannonModule`). Override or subclass modules to introduce new mechanics while keeping the follow-the-leader movement intact.