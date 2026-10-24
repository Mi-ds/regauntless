# Regauntless — Core Architecture (engine spine)

## Two clocks (the EverQuest model)

Reconciling 3.5's 6-second turn-based round with real-time play is solved the way **EverQuest**
solved it: don't dissolve the round — **keep it as an authoritative tick, and run continuous
twitch play on top.** Two clocks:

1. **Continuous twitch layer** — movement, projectiles, aim, ability activation / GCDs.
   Smooth, per-frame, sub-second. This is where player skill lives.
2. **Authoritative round-tick** — the SRD's **6-second round**, kept as a discrete, *exact*
   server/host heartbeat. Resolves round-based SRD effects (regen, durations, DoT/HoT,
   per-round accounting) **and** is the host-authoritative network-sync pulse. The SRD round
   = the EQ tick = the net-sync heartbeat. One heartbeat, double duty.

### The tick MUST be exact (and why)

The authoritative clock must be **precise and desync-free.** This is not optional polish — it
is what lets frame-perfect execution be *reliably rewarded*, which creates emergent skill
ceilings. **Rationale (EQ bard song twisting):** because EQ's server did perfect, desync-free
tick resolution and song activation sat on a ~0.5s fine clock, an expert bard could *twist*
up to 11 songs live + 1 activating (11+1=12) by timing activations perfectly against the tick.
That ceiling existed **only because the authoritative clock was exact.** A fudged or
desyncing tick would randomly drop a song and kill the mastery. 

This is the **same philosophy as deterministic combat** (`combat.md`): exactness + determinism
= rewarded mastery. The legible-breakpoints decision and the exact-tick decision are the same
value in two costumes. Do not let anyone later water the tick down to "close enough" —
exactness is load-bearing for skill expression.

## SRD ratios sacred; ONE tunable time-scale dial

The SRD defines the **ratios** (relative speeds, attacks-per-round, the round as common
denominator). We obey the ratios absolutely. The **round → real-seconds conversion is a single
global time-scale multiplier**, tuned for playability.

- Everything (movement rate, fire rate, cooldowns, cast times, tick duration) derives through
  that one dial. Tune the **one scalar** until it feels smooth; never hand-fudge individual
  values. Because all rates flow through it, the SRD relationships stay perfectly intact while
  absolute pace becomes playable.
- Default round = 6 seconds (SRD/EQ). The dial scales it. Derived examples (pre-tuning):
  speed-30 = 6 hexes/round = 1 hex/sec; speed-20 = 4; speed-40 = 8.

## Movement = SRD speed, transcribed to hexes

Per `principles.md`, movement is **not** invented — it's SRD transcription:

- 3.5 movement is already grid-and-rate based: speed-30 = 6 squares/round. Read **squares as
  hexes**: speed-30 = **6 hexes/round**. Speed is a *derived* stat (base − armor/load, per SRD).
- **Hexes fix 3.5's diagonal kludge.** The square grid's "every other diagonal = 2 squares"
  (5-10-5) hack exists to fake Euclidean distance on squares; on hexes all 6 neighbors are
  equidistant, so the kludge *vanishes*. Hexes are *more* faithful to the rules' intent, not less.
- **Glide, not hop** (resolves the open "glide-vs-hop" thread): the SRD gives a *rate*
  (hexes/round); real-time makes it a continuous glide at that rate through the time-scale dial.
- Turn-structure-dependent movement (5-ft step, full-round move, charge/run action economy)
  hits the real-time carve-out — translate what maps (run = sprint, difficult terrain = slow
  zone), flag what doesn't.

## Camera snap-rotation & camera-relative input (world stays absolute)

The arena is viewed as a hex board the player can **snap-rotate**, but the **world never
rotates** — only the camera does. This keeps the sim and netcode sane.

- **Q/E snap-rotates the CAMERA** in discrete **60° clicks** (6 hex-native viewing angles;
  60° is the hexagon's self-symmetry so the grid always lines up with itself). It is
  **purely local, cosmetic, client-side** — camera angle is NEVER networked. Two players can
  view the same authoritative world from different angles simultaneously.
- **World positions are absolute.** Heroes, enemies, projectiles never move or re-orient
  because of a camera snap. A camera snap mid-combat changes only *how you see* the board,
  not where anything is.
- **Pointy-top vs flat-top is a dead question** — it's just the camera's default angle; the
  player rotates from there.

### Three distinct things — do not conflate (this was a near-bug)

1. **Hero facing** — a *persistent world-space direction owned by the unit*. Changes only
   when the hero turns (from movement/aim intent). **Camera-independent.** Snapping the
   camera does NOT turn the hero; he keeps his world facing and merely *appears* rotated
   on screen.
2. **Camera angle** — one of 6 snap positions; how you *view* the fixed world. Cosmetic/local.
3. **Raw input** — screen-relative WASD/stick from the player.

### Camera-relativity applies to INPUT ONLY

Movement and aim are **always camera-relative** ("up" = up-on-screen), but that relativity
is an **input transform**, not a property of the hero. The pipeline:

```
raw input (screen-relative)
  → rotate by current camera angle (one of 6 fixed 60° offsets)   ← camera-relativity lives HERE
  → world-space Move/Aim vector
  → HeroIntent (ABSOLUTE world space)
  → hero moves / updates its own persistent world facing / shoots
  → (presentation: snap facing to 6 for body, aim to 12 for shot)
```

The hero's forward-facing is an **output in world space**, never re-derived from the camera.
**Camera-relative resolution happens client-side BEFORE `HeroIntent` is emitted** — so intent
is always absolute world-space direction. The host/sim never knows or cares what camera angle
a (possibly remote) player was using. Player A on angle 2 and Player B on angle 5 both emit
clean world-space intent; the authoritative world agrees. (This is also why camera rotation
is netcode-free.)

Sanity check: hero faces world-north, camera at angle 0. Snap camera +60°. Hero **still faces
world-north**, now just drawn "up-left" on screen. Press W → "up on screen" now means
world-northeast → hero turns to face that. The camera didn't turn him; the *next input,
interpreted through the new camera angle*, did.

## The Controller seam carries multiplayer (Prime Directive, extended)

The Prime Directive's `Controller → HeroIntent → Hero` seam is also the **multiplayer seam.**
Every input source is just a `Controller` emitting the same serializable `HeroIntent`:

- `AIController` (phase 1, auto-crawler), `PlayerController` (local human), and
  `NetworkController` (remote device over the wire) are **interchangeable**. The hero never
  knows which. A phone joining as hero #3 is the *same swap* as AI handing control to a local
  player — drop-in-coin, over TCP/IP.
- **Discipline this requires (enforce from day one):** Controllers emit **intent and NOTHING
  else**; only the hero/sim touches authoritative world state. If a Controller ever reaches
  into game state directly, networked/host-authoritative play breaks. Keep `HeroIntent`
  clean and serializable.

### Multiplayer roadmap (phased; later than the core loop)

1. **Single-player + AI party** — phase 1. All 6 heroes AI-driven (the auto-crawler / demo);
   human can take over any. Zero netcode.
2. **LAN / WiFi co-op** — *first* real multiplayer. The PC is the **host (authoritative)**;
   devices on the same LAN/WiFi (phone / console / other PC — "bring your own device,"
   Jackbox-style) join as `NetworkController`s. No matchmaking, no NAT traversal, trivial
   latency — the cheap, achievable wedge. Host-authoritative tick doubles as net sync.
3. **Internet co-op** — later / funded-stage. Add matchmaking, NAT traversal/relay, lag
   compensation **on top of** the working LAN foundation. Additive, not a rewrite.

Same seam at every phase. Couch same-screen co-op is **demoted** in favor of bring-your-own-
device-over-LAN (PC-first; nobody's playing this with 6 controllers on one couch).

## Core-loop-first (sequencing guardrail)

None of the above netcode/tick-as-netsync machinery gets *built* before the single-player core
loop is proven fun (see `vision.md` sequencing rule). The architecture is *designed for* it —
clean serializable intent, exact authoritative tick — so it's possible later without a rewrite.
Design for it now; build it when the loop earns it.
