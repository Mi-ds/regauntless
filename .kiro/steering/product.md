# Regauntless

A **tabletop-fantasy game show** — a televised dungeon crawl, built on the **open d20 SRD
3.5 (OGL)** ruleset. Six heroes run the gauntlet for *big money, big prizes*; a host hypes
the crowd; hordes pour from the generators; the Dragon waits at the finale. **Read
`vision.md` first — it's the north star** (and the legal posture: open mechanics, NEVER the
"D&D" trademark). In one line: *open d20 SRD 3.5 rules + Smash TV dual-stick arena combat +
a game-show frame.* Gauntlet (1985) is the tonal great-grandparent, not the blueprint.

The name is a pun — **re-gauntlet** + **relentless** (and the name of the show). Whimsy is
signal. Keep it.

## The Prime Directive (read this before writing a single system)

The game ships in phases:
1. **Auto-idle** — heroes are driven by AI. You watch your little guys stab goblins. (This
   phase is also the shippable **auto-crawler / demo** — the human's game is the
   character-sheet metagame; see `vision.md`.)
2. **Full control** (final form) — a human can take over any hero.

**These are NOT two codebases. They are one action pipeline with swappable input.**

Classic arcade cabinets (Gauntlet, Smash TV) treat every player slot as an identical input
source — the game does not care whether a human, nobody, or a demo-mode AI holds the stick.
We copy that exactly, and extend it to the network:

- Every hero is driven by a `Controller` (abstraction).
- `AIController` (auto-idle), `PlayerController` (local human), and `NetworkController`
  (remote device) all emit the SAME `HeroIntent` (Move + Aim vectors + action flags). They
  are fully interchangeable.
- "Full control conversion" is therefore NOT a rewrite. It is swapping a hero's
  `Controller` at runtime. Take over an idle hero → its `AIController` is replaced with a
  `PlayerController` mid-run. Drop-in-coin feel, for free. A LAN/remote player joining is the
  same swap over the wire.

If any gameplay system ever branches on "is this the player or the AI?", that is a design
smell. The hero doesn't know and doesn't care. It consumes intent. (This seam is also the
multiplayer seam — see `architecture.md`.)

## The Six (roster — PC-first; 6 rhymes with the hexagon)

| Class    | Role         | Flavor                          |
|----------|--------------|---------------------------------|
| Warrior  | melee        | strongest hits, slow            |
| Valkyrie | tank/melee   | best armor, balanced            |
| Wizard   | ranged magic | strong magic, weak shot/armor   |
| Elf      | fast ranged  | fastest move + fire rate, weak  |
| *(TBD)*  | *(e.g. Cleric — support/heal)* | SRD class, candidate |
| *(TBD)*  | *(e.g. Rogue — skirmish/traps)* | SRD class, candidate |

Six, not four (four was a Gauntlet/couch-hardware holdover; PC-first retires it). The two
new slots are SRD classes — candidates Cleric & Rogue, TBD. Distinguished by a **derived**
`HeroStats` (computed from class + level + abilities + feats + gear per the SRD), not
subclasses. Same `Controller` contract drives all of them, so the roster works at any player
count (AI fills unmanned slots). See `vision.md`.

## Platform

- **Unity 6000.0.75f1 (Unity 6.0 LTS)** — pinned in ProjectVersion.txt. Do NOT bump to
  the 6000.4.x tech stream without a reason; LTS is the stable target.
- **2D.** **True top-down** — flat overhead, NO ¾ slant, NO isometric diamond. The world
  is a **hex grid** (floor built of tessellating hex tiles). Controls are **dual-stick,
  decoupled**: 6-way movement / 12-way shooting, both from analog 360° input. See
  `vision.md` for the full control + world model.
- New **Input System** package (not legacy Input Manager) — the Controller abstraction maps
  cleanly onto InputActions, dual-stick decoupled aim is native to it, and it supports N
  players. **PC-first**: multiplayer is host-authoritative with remote devices joining as
  networked Controllers (LAN → internet, phased); couch same-screen co-op is demoted. See
  `architecture.md`.
- Modules installed: WebGL (share a playable link with no install), Windows, Linux, Mac,
  Android, iOS. WebGL is the "show Tyler/John" path.

## Combat & presentation philosophy (settled — see dedicated docs)

- **The Vision** — Regauntless is a tabletop-fantasy game show (open d20 SRD 3.5 rules +
  Smash TV dual-stick arena combat + game-show frame; never branded "D&D"). The north-star
  doc, including the **metagame reframe** (character-sheet is the human's game; AI runs
  combat; demo = auto-crawler), the **six-hero roster**, multiplayer, and
  legal/monetization/live-service posture. See `vision.md`.
- **Follow the SRD, don't reinvent** — the governing design principle (SRD = mechanics
  authority; our job = presentation; carve-outs: real-time translation + Mids' house-rule
  override). See `principles.md`.
- **Core architecture** — two-clock model (continuous twitch + exact authoritative 6-second
  round-tick = EQ-style net-sync heartbeat), SRD ratios sacred with one tunable time-scale
  dial, movement transcribed to hexes, the Controller seam as the multiplayer seam. See
  `architecture.md`.
- **Combat uses real SRD 3.5 (OGL) numbers but doesn't roll them** (except Gremlin Mode).
  Turn-based → real-time: initiative dropped, the round survives only as fire-rate; to-hit
  is geometry + player aim, not a d20; damage is deterministic with legible breakpoints.
  Difficulty = a global die-resolution policy — Easy (Maximize) / Normal (Take 10) /
  Hard (Minimize) / **Gremlin Mode** (roll everything, misses + crits return). One
  `RollPolicy` seam, downstream of the Controller pipeline. See `combat.md`.
- **Combat space (lanes/projectiles/collision)** — positions are hex-discrete (centers +
  lines); projectiles occupy the middle N% of a lane (per-class width + shape = spatial
  identity, Gauntlet-grounded); bodies ~50-60% of a hex; **everything collidable** via one
  footprint-overlap rule → blocking is legible coordination, not chaos. See `combat-space.md`.
- **Survival = ONE shared party pool** (provisioned + mcguffins; drains via zone-hostility
  tick + mitigated enemy damage; binary functional/down; food refills; P2P buy-back-in at
  zero). Health=timer soul, at run level. Dumb-AI = a management decision (swap-to-human /
  bench), diegetic Prime-Directive funnel. See `survival.md`.
- **The coordination engine** — the shared pool *forces* teamwork: AoE heals/buffs multiply
  by clustered members (stack-vs-spread tension), driving a spread⇄converge rally rhythm
  (voice-comms in co-op; formations/commands solo). "Formations" = party-cohesion umbrella
  fed by many SRD sources (Leadership radial / Bard inspire / Paladin aura); light action-game
  command layer (radial + pre-run roles), no RTS micro. See `coordination.md`.
- **Modern tech, 1985 design concepts.** High-res, but graphics are an information channel:
  legible through *feel*, explicit readouts reserved for the rare/surprising. Enemy health =
  discrete sprite states, one per hit-breakpoint. See `presentation.md`.
- **Gremlin Mode leaderboard** is a flagged, backend-dependent stretch goal — local score
  first, hosted later. See `../design-notes/leaderboard.md`.
- **The Announcer / Host** — a game show needs a host; built voice-agnostic + hot-loadable
  (hosts are live-service content packs), with a marquee TTRPG voice as flagged aspirational
  casting (not a dependency). See `../design-notes/announcer.md`.

## Status

Fresh scaffold. No scenes, no scripts yet. **Vision reframed** to a tabletop-fantasy game
show with a character-sheet metagame core (`vision.md`); governing SRD principle
(`principles.md`), two-clock architecture + multiplayer roadmap (`architecture.md`), and
combat + presentation philosophy all settled. **Newly resolved:** six-hero roster, derived
stats, feats/skills first-class (transcribe from SRD), movement = SRD speed → hex glide,
round = exact authoritative tick, movement glide-vs-hop (glide), camera snap-rotation +
camera-relative controls, hex orientation (dead question — camera default only), **and the
big one: the health/survival model** (shared party pool + coordination engine +
leadership/formations — see `survival.md`, `coordination.md`). **Still open:**
run/round/prize structure, the two extra roster classes, deterministic crits, mcguffin
specifics (consumable vs permanent), command-depth (deferred), enemy-AI philosophy (leaning
deterministic/herdable baseline + type spice). Next build milestone: Controller abstraction
+ one AI hero moving (6-way) and shooting (12-way) at a dummy in one hex room.
