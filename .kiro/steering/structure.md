# Regauntless — Structure & Conventions

## Unity project layout

```
Assets/
  Scripts/
    Controllers/   AIController, PlayerController, IController, HeroIntent
    Core/          game loop, spawn director, room/level, services
    Heroes/        Hero behaviour, HeroStats, the four archetypes
    Enemies/       enemy behaviour, spawner ("generator" in Gauntlet terms)
    Combat/        damage, projectiles, melee, health
  Scenes/          .unity scenes
  Prefabs/         hero/enemy/projectile prefabs
  Art/Sprites/     character + enemy sprites
  Art/Tiles/       tilemap tiles for the dungeon floor/walls
  Settings/        render pipeline / input action assets
ProjectSettings/   pinned to 6000.0.75f1
Packages/          manifest.json — 2D tilemap, Input System, uGUI
```

## The intent contract (the whole ballgame)

A `Controller` produces a per-frame (or per-tick) `HeroIntent`:
- `Vector2 Move` — desired movement direction (analog 360°, magnitude 0..1)
- `Vector2 Aim` — desired aim/shoot direction (analog 360°), **decoupled from Move**
- `bool Attack` — fire this frame
- (later) `bool UseItem`, etc.

**Dual-stick, decoupled (the Smash TV core verb):** `Move` and `Aim` are independent —
strafe one way, shoot another. Input is analog 360°; the **presentation snap** (sprite
facing) is applied downstream at different resolutions: **6-way for movement** (hex edges),
**12-way for shooting** (hex edges + vertices). See `vision.md`.

The `Hero` consumes `HeroIntent` and does movement + combat. The Hero NEVER asks who
produced the intent. `AIController` computes intent from world state (nearest enemy,
pathing). `PlayerController` computes intent from the Input System (two sticks). Swapping
them is how auto-idle becomes full-control.

## Code conventions

- C# standard Unity style: PascalCase types/methods/public fields, camelCase locals,
  `_camelCase` private fields. One MonoBehaviour per file, file named after the class.
- Keep gameplay logic testable where cheap — pure intent/stat math shouldn't need the
  engine running. (test-framework package is included.)
- Favor composition on prefabs over deep inheritance. The four heroes differ by a
  `HeroStats` data object, not four subclasses, unless behaviour truly diverges.
- Whimsy in names is welcome and in-character for this project. Enemy spawners are
  "generators" (Gauntlet term). Keep the arcade flavor.

## Workflow notes

- This is Mids' silly/fun side project — low stakes, high vibes. Not the stonks fleet.
- Build/run happens in the Unity editor (6000.0.75f1). I can write scripts, scenes (as
  YAML), prefabs, and settings as files; Mids opens the editor to see pixels move.
- Git: initialized here, Library/ ignored. Commit only when Mids asks.
