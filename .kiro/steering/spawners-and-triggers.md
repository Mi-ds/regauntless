# Regauntless — Spawners & Trigger Fields

*Inherit-safe (intent + rationale, for a maintainer or a level-gen subagent with no memory of the
conversation that produced this). Worked out by dismantling a naive "5 spawner types" idea down to
TWO primitives.*

The naive version was "spawner types: generator / ambush / boss / pack / ambient." That's wrong —
it conflated **how creatures ENTER a level** with **how they BEHAVE once out**, and invented engine
types for what are really parameter presets. The real factoring is **two primitives**:

## 1. The Emitter Spawner (ONE primitive, parameterized)

The only thing that actually spawns creatures. A **set prop** in the level. Fundamentals:
- **location** — the hex it sits on, OR bonds into (free-standing *pile of bones* in a cell vs
  *bones protruding from a wall* fused to the cell — SAME spawner, just a placement/mounting
  variation, not two objects).
- **spawnDirection** — the way it emits; seeds the initial vector of the extruded mob line (feeds
  the collision-slide-drain movement model, combat-model.md — mobs come out facing this way then
  pile/slide/drain). Wall-bonded ones naturally point away from the wall.
- **mob** — pointer to the creature(s) it produces (a bestiary key).
- **burstCount** — N spawned INSTANTLY on activation (0 = none).
- **rate** — steady drip after the burst (0 = burst-only then inert).
- **count / cap** — total it will ever produce (∞ = endless Gauntlet generator; finite = exhausts
  into a dead prop).
- **hp** — it's DESTRUCTIBLE (killing it stops the stream; priority target — combat-model.md).
- **skinKey** — stable NAME for its visual (bone_pile / wall_bones / badger_den / …). Art bound
  SEPARATELY via a definition registry (mirrors CellTypeDefinition: label-first, sprite-later;
  missing art = a labeled gray stub, never a blocker — art-survival.md). NEVER a raw graphic path
  in gameplay data.

**The "modes" I wanted are just presets of these params — zero special-casing:**
| Preset | burstCount | rate | count |
|---|---|---|---|
| Gauntlet generator | 0 | steady | ∞ |
| Burst-then-drip | 5 | steady | ∞ |
| Hand-placed clump / ambush pop | N | 0 | N |
| Solitary | 1 | 0 | 1 |

Editor exposes these as **preset buttons** that stamp a pre-filled Emitter Spawner; the author then
tweaks any field. Presets are starting points, NOT subclasses.

**Behavior is NOT a spawner property.** "Pack" is a per-creature BEHAVIORAL flag (pack-tactics,
combat-model.md — the LoS-gated multi-line-around-a-corner stage), not a spawn mode. The emitter
drips mobs; what they do once out (archetype, pack-tactics, sensing) lives in the bestiary entry.
The spawner doesn't know or care.

## 2. The Trigger Field / Event layer (a separate scripting system, sits ABOVE the emitter)

A **zone that runs a script when a condition is met.** This is the authored-set-piece engine
(levels.md). "Ambush," "boss lair," "guarded room" are NOT engine types — they're authored
*scripts* built from these parts:

- **zone** — a dragged floor-region (bounds the author paints; the editor FLOODS the covered hexes
  as you drag — the "show where it fills to" feedback moment). Rides the same cell/zone layer as
  web-floor / water / Hole / circumstance-tiles.
- **trigger** — the condition that fires it. Open enum, extensible: **entry / exit** (first two),
  then on-death-of-X, on-timer, on-interact, on-prior-event-complete (chaining), etc. Add
  condition-types as needed; the architecture just says "a condition."
- **script** — an ORDERED list of effects + an advance/completion condition. Effect kinds include:
  **place/activate emitter-spawner(s)** (deploys type-1 generators into the zone — the two systems
  meet HERE), **play a line** (announcer/dialogue), **mark a lair** (boss-territory dressing:
  gold hoard / alien hatch-eggs / roc nest — the zone's visual), **grant a reward**, **gate/open
  geometry**, **chain the next event**, …

**Worked example (shows every part + the chaining):**
> Player ENTERS the zone → miniboss **says a line** → script **places 3 emitter-spawners** inside
> the field → **completion condition = all 3 spawners destroyed** → event ADVANCES (open the door /
> reward / next wave).

Note the simplification this proves: the field doesn't invent spawning — it **places normal
emitter-spawners**, which then behave by the normal rules (pile/slide/drain/pack). One emitter,
reused.

## Authoring loop (the editor flow)
1. **Drop a trigger field.**
2. **Drag its bounds** (zone floods visibly as you drag — reuses the chunk-painter floor-paint gesture).
3. **Assign a trigger method** (entry/exit/…).
4. **Define the script** (ordered effects + completion); "place emitter-spawner(s)" is one effect.

→ `field = { zone, trigger, script }`. Three things to fill, powerful engine underneath. This is
what makes authored set-pieces authorable IN-EDITOR by Mids (and later by a level-gen subagent
reading the same structure) rather than hand-coded.

## What to store where
- **Emitter spawner** = a placed level object: `{ location, spawnDirection, mob, burstCount, rate,
  count, hp, skinKey }`.
- **Spawner-definition registry** (new, mirrors CellTypeDefinition): `skinKey → prefab/sprite (or
  stub)`. Art-bound-later.
- **Trigger field** = a placed level object: `{ zone, trigger, script[] }`.
- **Bestiary** holds CREATURE data + behavior only. It may carry *suggested* spawner-param defaults
  per creature (derived from the SRD `organization` line we transcribed — e.g. centipede → high
  rate/big count; badger → solitary preset) as HINTS the editor presets can read. It does NOT
  store spawner "types."
