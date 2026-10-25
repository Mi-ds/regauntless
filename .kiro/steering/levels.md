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

## The TWO-GRID / two-layer model (walls vs floor) — IMPORTANT

> **Correction:** earlier drafts assumed ONE packed hex grid with cracks "emerging from
> diagonal wall placement." That was wrong (it came from picturing walls as a tightly-packed
> flat-to-flat grid). The real model is TWO interlocking grids, authored as two toggleable
> **layers**:

- **FLOOR layer** = a **tightly-packed, flat-to-flat (edge-to-edge) hex grid.** The solid
  walkable ground — hexes tessellate seamlessly into a continuous floor.
- **WALL layer** = a **spaced-out, tip-to-tip (point-to-point) hex grid.** Wall hexes touch
  only at their **vertices/points**, NOT edges — so there are natural **gaps between
  point-touching walls.** This is the grid the author mostly draws on to shape a level/chunk.

The author **toggles which layer** the brush paints (floor-grid vs wall-grid), same canvas,
overlaid; the chunk is the composite. (Classic tilemap-layers approach.)

### Cracks are emergent — from the tip-to-tip WALL grid

Because wall hexes touch **point-to-point**, every junction where two walls meet at a vertex
leaves a **pinch-gap = a crack**: a shot can thread the point-contact (subject to projectile
width, combat-space.md), a body cannot. **Cracks are the default state of point-touching
walls** — you don't author them, they're the geometry of the spaced wall grid.

### Sealing a crack: the 15°-rotated bridge hex

To make a junction **solid** (no crack), drop a **bridging wall hex centered on the contact
point, rotated ~15°, and rasterized** — it seats into the gap and fills the point-junction
into continuous solid wall. So **crack-vs-solid is authored by presence/absence of the bridge
hex**: leave the tip-to-tip junction open → crack; place the rotated bridge → solid wall.
(Exact bridge angle is a render/rasterization detail to verify in-editor — 15° is Mids'
description; confirm by eye. This is a "show me" geometry item — built with eyes-on, not blind.)

### Why this matters for the painter

The chunk painter must author on the **tip-to-tip wall grid** (the spaced one) for walls, with
a toggle to the **packed floor grid** for floor — NOT the single packed grid an earlier painter
draft used. The "window" opening in a guard room is just a crack (an open tip-to-tip junction)
rather than a special token.

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
