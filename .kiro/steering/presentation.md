# Regauntless — Presentation Philosophy

**Modern tech, 1985 design concepts.**

Gauntlet is the *design* north star, not the *fidelity* ceiling. We are NOT doing 1980s
pixel art. High-res, animated, shader-driven objects — but every visual choice serves a
1985 design *concept*: readable at a glance, information-bearing, no clutter.

## Lit from below (the signature rendering trick)

Levels are **lit from beneath the floor.** This is the distinctive look-and-feel pillar.

- **The floor is an opaque tile surface** mapped from the chunk footprint — and it doubles as
  the **light-occlusion mask.** Under it (below the play plane in Unity space) runs a hidden
  **lighting stage**: tessellated lights, color washes, animated "cool graphical shit."
- **Opaque floor OCCLUDES** the under-light; it reaches the play space only by bouncing off a
  **faked "ceiling rig" derived from the same floor footprint** — giving soft ambient reflection
  for free. All ambiance/lighting is authored by doing cool stuff *under* the floor, never by
  lighting the play space directly.
- **Holes (Cell type `Hole`) are APERTURES, not voids:** where there's no floor, the under-light
  **radiates straight up through the gap** — a bright **light well / shaft**, shaped by whatever
  source glows beneath. Holes are non-walkable (pits) — a traversal obstacle *and* an atmospheric
  beam. (`Cell.OccludesUnderLight` is false only for Hole/None.)
- **The painted floor footprint does TRIPLE duty:** gameplay shape + light-occlusion mask +
  (via the derived ceiling) reflection surface. One authored thing, three payoffs — so the chunk
  paint editor is also authoring the lighting stencil.

> **Back-pocket (parked for the lighting pass):** build the under-floor stage as a
> **camera-synced shading/mask layer**, NOT a bounded textured box. Cheap textures used as
> occluding shading masks can fake rich atmosphere (cf. Mids' Nebulous skyboxes), AND — crucially
> — a camera-synced mask layer **rotates infinitely and seamlessly** with no seam/reload hitch.
> This matters directly: when the player **snap-rotates the camera** (architecture.md), the
> under-floor light stage must rotate WITH it; a bounded box would seam, a synced mask layer
> won't. Pre-solved technique; revisit when we actually wire lighting + rotation.
- Fits "modern tech, 1985 concepts": keeps the flat, legible top-down *read* while adding rich
  modern atmosphere from beneath. (VERIFY-BY-EYE: this is a look that only the human-in-editor
  can judge — URP 2D lights / under-floor stage / faked reflection. Fox builds, Mids judges.)

## Graphics are an information channel, not decoration

We have the resolution to put real, legible meaning into the visuals. Use it. But —

## Legible through *feel*, not readouts

Normal players don't do arithmetic mid-horde. They don't care about numbers *as numbers* —
they care about the *information* the numbers represent, delivered as **feel**:

- A skeleton that reliably dies in one Warrior hit teaches its breakpoint through **rhythm**
  (swing → pop → dead), not a floating `3`. The **determinism** of combat is what makes that
  rhythm learnable — rolled damage would stutter it and the player would never internalize
  the breakpoint.
- Floating damage numbers on every trash mob are **clutter** — on a screen full of enemies
  they obscure the actual game. Don't.

**Rule: reserve explicit readouts (numbers, text) for the rare and the surprising.** A
`-miss-` (Gremlin Mode), a crit, a boss — rare enough to *earn* a readout. A skeleton dying
on schedule earns a good *thunk*, not a digit.

## Enemy health = discrete visual states, one per breakpoint

The 1985 concept: the enemy's *look* tells you its mechanical state — a diegetic health bar
baked into the sprite. Because combat is **deterministic**, the visual vocabulary and the
mechanical vocabulary have the **same count**: an enemy has exactly as many health states as
it has hit-breakpoints. This also makes the art authorable — you know precisely how many
states to draw: one per hit it can take.

**States scale with hit count:**

| Enemy        | Breakpoints | Visual states                                  |
|--------------|-------------|------------------------------------------------|
| 1-hit trash  | 1           | `Healthy` → dead                               |
| mid enemy    | 2           | `Healthy` → `Ragged` → dead                    |
| tougher      | 3           | `Healthy` → `Hurt` → `Ragged` → dead           |
| elite/miniboss | many      | full gradient; rare enough to *earn* an explicit readout if wanted |

## Modern execution of the 1985 concept

States are **not** hard flipbook swaps. We have the fidelity to make transitions juicy:
limbs break off, cracks spread, the thing hunches, bits flake, a shader desaturates it.
Wireframed/animated/deformable objects, not two-frame sprites. The **concept** is 1985
(discrete readable states); the **execution** is modern (animated, shader-driven, satisfying).

## Why this converges

- "Players don't care about numbers" → HP info lives in the **sprite state**, not a gauge.
- "Legible through feel" → the ragged sprite *is* the legibility; you feel "almost dead"
  without reading anything.
- Determinism (from `combat.md`) → defines how many states each enemy needs, and makes the
  feel-based mastery loop actually learnable.

Whimsy is signal (see `product.md`). Legible whimsy doubly so.
