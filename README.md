# Regauntless 🛡️

A top-down ¾-perspective dungeon battler in the spirit of **Gauntlet** (1985).
Four heroes, hordes from the generators, march to the exit.

> *re-gauntlet + relentless* — run the gauntlet, again, fearlessly.

## Phases
1. **Auto-idle** — AI-driven heroes. Watch your little guys stab goblins.
2. **Full control** — humans take over any hero. Same pipeline, swapped input.

The one rule: AI and player are interchangeable `Controller`s feeding one action
pipeline. Full control is a controller swap, not a rewrite. See `.kiro/steering/`.

## Stack
- Unity **6000.0.75f1** (Unity 6.0 LTS), 2D, new Input System
- Targets: WebGL, Windows, Linux, Mac, Android, iOS

## Open in editor
Open this folder via Unity Hub (it will match 6000.0.75f1).
First time: Unity regenerates `Library/` (gitignored) — give it a minute.
