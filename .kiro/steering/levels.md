# Regauntless — Level Generation

**Two layers: a fractal/grown MAZE (procedural topology) furnished with hand-authored
SET-PIECE CHUNKS (crafted content).** Procedural shape, authored moments. This is the
Spelunky/Isaac model (generated arrangement of hand-made rooms), arrived at from "grow it
like a crystal + slot in themed segments like a guard room."

Deterministic: a level = **seed + ruleset**. Same seed → same level, every time (fits the
determinism philosophy in `combat.md`, and the game-show "tonight's dungeon, seed #X" /
live-service framing in `vision.md`).

---

## Layer 1 — The fractal maze (procedural, grown from a seed)

The connective geography — corridors, junctions, the maze-like topology. **Grows like a
crystal** from a single origin:

```
GENERATE(seed, rules):
  rng ← seed
  bloom the party-spawn rosette at origin         # the crystal nucleus (see below)
  while components remain in the budget:
    pick a growth site on the existing crystal     # frontier-biased (grows outward) — TBD knob
    pick a random hex vector (one of 6) + bounded random distance (min..max)
    carve a corridor along it
      └─ if it OVERLAPS existing geography → FUSE into an intersection/junction
         (overlaps are not errors; they enrich connectivity → loops & junctions)
    place the next node; consume from the component budget
    once ≥ ~80% of the budget is consumed → enable 'exit' as a placeable component
  ensure exactly one exit exists (force in the tail if needed)
```

- **Crystal growth / accretion** — structure accretes outward from the seed; overlaps fuse
  into intersections (better connectivity than a pure tree).
- **Budget-driven content** — "how many total nodes/spawn-points, how dense" = a count +
  density in the ruleset; placement randomized from the seed.
- **Exit gated to the end** — the exit is just another component, weighted to 0 until ~80%
  of the budget is consumed, so it lands far from spawn (you traverse the whole crystal).
  Not special-cased placement — uniform component model.

**Open knobs (TBD, tune later):** growth-site selection (frontier-biased vs. any-hex vs.
most-recent — shapes how bushy/snakey the maze feels); exit crisp-at-80% vs.
eligible-after-80%; corridor distance bounds.

The maze emits a **graph of typed slots** (spawn / arena / trap / puzzle / swarm / junction /
prize / exit ...) connected by corridors. It does NOT author the inside of rooms — it decides
*where* things go and *how they connect*. Layer 2 fills the slots.

---

## The TWO-GRID model (walls derived from floor) — THE REAL, VERIFIED CONSTRUCTION

> This is the construction that actually works (confirmed in the chunk painter after much
> iteration). Mids' original two-sentence description was correct the whole time; earlier
> drafts in this doc (edge-midpoints, (q+r) parity, "15° bridge") were the fox mis-implementing
> it and are SUPERSEDED by what follows.

**Floor layer** = the base: a full, normally-bloomed, tightly-packed flat-to-flat hex grid.
The solid walkable ground. (In the painter: `HexCoord.Range(radius)`, flat-top.)

**Wall layer** = derived from the floor by this exact recipe:
1. **Copy** the floor footprint.
2. **Move the copy half a cell straight N or S** — PURE VERTICAL, zero horizontal. (For
   flat-top the N–S cell spacing is `√3·size`, so half a cell = `(√3/2)·size`.) The move goes
   **OPPOSITE the bloom arc** (see step 3): bloom NE/N/NW → move **South**.
3. **Half-bloom the copy on 3 adjacent sides** — grow one hex outward on `{facing, facing+1,
   facing+2}` (a 180° arc). Default facing = the **NE/N/NW** arc. (`Bloom.HalfBloom`, tested.)
4. **Rotate each wall hex 30°** in place (crosswise; 30° = the point-to-point orientation; by
   hex symmetry 90°/30°/"15° via the trinary" describe the same crosswise look — orientation
   is a 0/15/30 trinary for future-proofing).

That's the whole thing. The point-to-point wall lattice, floor enclosed, **emerges** from
copy + half-cell-vertical-move(opposite the bloom) + half-bloom + 30°. Do NOT try to
manufacture the lattice per-edge — apply the transforms and let it emerge.

### Hard-won lessons (so we never redo the 37 tries)

- **Hex offsets are a SINGLE pure direction, never an additive x+y diagonal.** A diagonal
  "half left + half down" lands off-lattice and looks funky. Half a cell N/S is pure vertical.
- **A generative recipe is applied, not reverse-engineered.** "Every other" / "point-to-point"
  were *descriptions of the emergent result*, not per-cell rules to compute.
- **Geometry is verified by EYE in-editor, not reasoned blind** (division of labor:
  Mids owns spatial/geometric; fox owns the symbolic/coordinate math + implementation).

### Cracks (revisit later, eyes-on)

Where/whether shots thread between walls (cracks) falls out of this wall geometry + projectile
width (combat-space.md). Exact crack behavior is a verify-in-editor item once walls collide
with projectiles. The "window" in a guard room is an open gap in the wall run, not a token.

### For the painter

Floor + wall layers are independently viewable (visibility toggles + draw-order "Front"
control). Walls auto-derive from the floor via the recipe above; the author paints the floor
footprint (and, later, edits walls/overrides).

---

## Layer 2 — Set-piece chunks (hand-authored, "fox created")

Small, crafted hex segments with *intent* — e.g. a **guard room**: a little room with a guard
spawn inside, a **door**, and a **window**. Dev/fox-mapped by hand, not generated. The
generator **slots a chunk into each maze node** by picking (from the seed) from that node
type's chunk-pool. Add chunks → infinitely more level variety for free (the live-service
content engine).

### Chunk anatomy

```
CHUNK:
  theme/type:  guard-room          # which pool it belongs to (spawn/arena/trap/puzzle/swarm/prize/...)
  layout:      <hand-drawn hex geography: walls, floor, cracks>
  openings:                        # how the maze stitches to it
    - door   @ <edge hex>          # full passage: corridor attaches, bodies + shots pass
    - window @ <edge hex>          # NOT a full passage: a 'crack' — shots pass, bodies don't
                                   #   (ties to lane threading / shoot-through-the-crack, combat-space.md)
  contents:
    - guard-spawn @ <hex>          # generators, enemies, pickups/food, props
```

- **Openings:** `door` = a full passage (open floor on the chunk boundary; bodies + shots
  pass; the maze corridor stitches here). A `window`/crack is NOT a separate token — it's just
  an open **tip-to-tip wall junction** (see the two-grid model above): shots thread it,
  bodies don't. Author cracks by leaving wall junctions un-bridged.
- **Connection points are how Layer 1 stitches to Layer 2** — the generator attaches a maze
  corridor to a chunk's `door`.
- **Contents = the authored intent** — the guard spawn, the trap, the swarm generators, the
  prize.

### The first chunk is already designed

The **party-spawn rosette**: center hex + its 6 neighbors (one hex per hero), from the
6-roster. The spawn chunk's shape is literally the party (center may be the leader hub —
ties to formations/coordination, `coordination.md`). This is chunk #0.

---

## Two file formats (both human-readable TEXT, git-friendly — never a bitstream)

1. **Chunk files** — hand-authored. Chunks are small + fixed, so the layout is best as
   **readable ASCII-hex-art** (you want to SEE a chunk), plus declared openings + contents.
   The authoring surface — lets both Mids and the fox make content WITHOUT the Unity editor
   (escapes the manual scene-dragging that is slow/painful).
2. **Generator ruleset** — tiny: `seed`, component budget (counts + density), per-type
   chunk-pool weights, corridor distance bounds, hostility (zone drain, `survival.md`),
   camera default. A level is a seed + this ruleset.

Runtime layer (SEPARATE from the notation): `HexCoord` (axial+cube) and a loader that
executes recipe → hex map. The notation is a contract designed for authoring/legibility; the
runtime conforms to it, not vice versa.

---

## Build order (dependency)

```
chunk FORMAT (+ 1-2 hand-authored chunks, incl. the spawn rosette)   ← authoring surface first
   └─► chunk LOADER / parser (pure, testable)
        └─► HexCoord primitive (axial+cube: neighbors/distance/ring/line/rotate — pure, tested)
             └─► the crystal GENERATOR (seed+rules → maze graph of slots; pure, seed-deterministic)
                  └─► SLOTTING (pick chunks per node by seed, stitch doors to corridors)
                       └─► render to a Unity hex Tilemap (editor layer)
```

Chunks-first because you can't test slotting/generation without chunks to slot. Everything
down to "render to Tilemap" is **pure and headless-testable** (same seed → same output;
connected; exactly one exit; exit in the late/outer region; spawn present; chunk openings
all stitched) — the right kind of foundational work, and legitimate scoped-subagent territory
with a deterministic test harness.

---

## Why this is the right foundation

- **Authored quality + procedural scale** — crafted moments (the guard room *is* designed)
  inside infinite generated topologies. Not soulless-random, not tedious-handmade.
- **Deterministic** — seed reproduces the level (shareable, testable, "beat seed #1234").
- **Content-as-recipe / live-service** — add chunks + tweak rulesets → endless fresh dungeons
  without hand-building each (the Fortnite/Diablo content engine, `vision.md`).
- **Escapes manual scene authoring** — levels become text, not drag-the-objects-in-Unity.

## Spawners & trigger fields
See `spawners-and-triggers.md` — the TWO primitives: (1) one parameterized Emitter Spawner (location/direction/mob + burst/rate/count; presets reproduce generator/burst/clump/solitary), and (2) a Trigger Field/event scripting layer (zone + trigger condition + ordered script; "ambush"/"boss lair" are authored scripts, not engine types). The authored set-piece chunks above ARE trigger-field scripts.
