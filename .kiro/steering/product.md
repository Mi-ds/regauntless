# Regauntless

A **D&D-themed game show** — a televised dungeon crawl. Four heroes run the gauntlet for
*big money, big prizes*; a host hypes the crowd; hordes pour from the generators; the
Dragon waits at the finale. **Read `vision.md` first — it's the north star.** In one line:
*D&D 3.5 rules + Smash TV dual-stick arena combat + a game-show frame.* Gauntlet (1985) is
the tonal great-grandparent, not the blueprint.

The name is a pun — **re-gauntlet** + **relentless** (and the name of the show). Whimsy is
signal. Keep it.

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
- **2D.** **True top-down** — flat overhead, NO ¾ slant, NO isometric diamond. The world
  is a **hex grid** (floor built of tessellating hex tiles). Controls are **dual-stick,
  decoupled**: 6-way movement / 12-way shooting, both from analog 360° input. See
  `vision.md` for the full control + world model.
- New **Input System** package (not legacy Input Manager) — because the Controller
  abstraction maps cleanly onto InputActions, it supports N local players (couch co-op), and
  dual-stick decoupled aim is native to it.
- Modules installed: WebGL (share a playable link with no install), Windows, Linux, Mac,
  Android, iOS. WebGL is the "show Tyler/John" path.

## Combat & presentation philosophy (settled — see dedicated docs)

- **The Vision** — Regauntless is a D&D game show (3.5 rules + Smash TV dual-stick arena
  combat + game-show frame). The north-star doc. See `vision.md`.
- **Combat uses real SRD 3.5 (OGL) numbers but doesn't roll them** (except Gremlin Mode).
  Turn-based → real-time: initiative dropped, the round survives only as fire-rate; to-hit
  is geometry + player aim, not a d20; damage is deterministic with legible breakpoints.
  Difficulty = a global die-resolution policy — Easy (Maximize) / Normal (Take 10) /
  Hard (Minimize) / **Gremlin Mode** (roll everything, misses + crits return). One
  `RollPolicy` seam, downstream of the Controller pipeline. See `combat.md`.
- **Modern tech, 1985 design concepts.** High-res, but graphics are an information channel:
  legible through *feel*, explicit readouts reserved for the rare/surprising. Enemy health =
  discrete sprite states, one per hit-breakpoint. See `presentation.md`.
- **Gremlin Mode leaderboard** is a flagged, backend-dependent stretch goal — local score
  first, hosted later. See `../design-notes/leaderboard.md`.
- **The Announcer / Host** — a D&D game show needs a host; built voice-agnostic, with Matt
  Mercer as flagged aspirational casting (not a dependency). See `../design-notes/announcer.md`.

## Status

Fresh scaffold. No scenes, no scripts yet. **Vision reframed** to a D&D game show
(`vision.md`); combat + presentation philosophy settled. Open design threads: run/round
structure, prize rounds, hex orientation, movement glide-vs-hop. Next build milestone:
Controller abstraction + one AI hero moving (6-way) and shooting (12-way) at a dummy in one
room.
