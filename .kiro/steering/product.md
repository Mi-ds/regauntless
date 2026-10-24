# Regauntless

A top-down ¾-perspective dungeon battler. **Gauntlet (1985 arcade / Neo-Geo era) is the
north star.** Four heroes, hordes of enemies pouring from spawners, march to the exit while
the dungeon tries to eat you. *"Elf needs food, badly."*

The name is a pun — **re-gauntlet** + **relentless**. Whimsy is signal. Keep it.

## The Prime Directive (read this before writing a single system)

The game ships in phases:
1. **Auto-idle** — heroes are driven by AI. You watch your little guys stab goblins.
2. **Full control** (final form) — a human can take over any hero.

**These are NOT two codebases. They are one action pipeline with swappable input.**

In the original Gauntlet cabinet, all four player slots are identical input sources — the
game does not care whether a human, nobody, or a demo-mode AI is holding the joystick.
We copy that exactly:

- Every hero is driven by a `Controller` (abstraction).
- `AIController` (auto-idle) and `PlayerController` (human) both emit the SAME hero
  intent — a movement vector + action flags. They are interchangeable.
- "Full control conversion" is therefore NOT a rewrite. It is swapping a hero's
  `Controller` at runtime. Press Start on an idle hero → its `AIController` is replaced
  with a `PlayerController` mid-run. Drop-in-coin feel, for free.

If any gameplay system ever branches on "is this the player or the AI?", that is a design
smell. The hero doesn't know and doesn't care. It consumes intent.

## The Four (Gauntlet roster → starting archetypes)

| Class    | Role        | Flavor                          |
|----------|-------------|---------------------------------|
| Warrior  | melee       | strongest hits, slow            |
| Valkyrie | tank/melee  | best armor, balanced            |
| Wizard   | ranged magic| strong magic, weak shot/armor   |
| Elf      | fast ranged | fastest move + fire rate, weak  |

Distinguished by stats: move speed, attack range, fire rate, armor. Same Controller
contract drives all of them.

## Platform

- **Unity 6000.0.75f1 (Unity 6.0 LTS)** — pinned in ProjectVersion.txt. Do NOT bump to
  the 6000.4.x tech stream without a reason; LTS is the stable target.
- **2D.** ¾ top-down perspective (authentic Gauntlet), NOT true isometric diamond.
- New **Input System** package (not legacy Input Manager) — because the Controller
  abstraction maps cleanly onto InputActions, and it supports N local players (couch co-op
  = the whole point of a Gauntlet clone).
- Modules installed: WebGL (share a playable link with no install), Windows, Linux, Mac,
  Android, iOS. WebGL is the "show Tyler/John" path.

## Status

Fresh scaffold. No scenes, no scripts yet. Next: Controller abstraction + one AI Warrior
walking to and whacking a dummy enemy in one room.
