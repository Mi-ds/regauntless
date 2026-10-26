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

## Avatars: shape-first — crosshatched primitives + a pixel-art HAT

The no-illustrator reality is a HARD constraint (Mids is not a painter; scheduled art killed
three prior projects — see art-survival.md). So the avatar direction turns that constraint into
an AESTHETIC, and scopes the only hand-made art down to a slice Mids can actually make and enjoy.

- **Chassis = a crosshatched PRIMITIVE, lit from below.** Hero = a **pyramid** — chosen because a
  pyramid has an inherent FRONT: the apex/edge **points**, so the geometry itself shows FACING for
  free (matters for dual-stick move-vs-aim + camera snap-rotation; a sphere/capsule can't). Enemies
  = other primitives (cube / sphere / and the **V-wedge** collision footprint from combat-model.md
  — the mechanic shape IS the visible shape). Angled faces catch the under-light dramatically → the
  lit-from-below pillar does the shading for free.
- **Surface = a crosshatch SHADER/material**, not hand-painted detail (the Nebulous trick: tiny
  bitmap + parametric shader work = rich read, zero illustration). Gives an "inked tabletop game
  piece" look — on-theme for the tabletop-fantasy / game-show framing.
- **Identity = a PIXEL-ART HAT / prop** — the ONE bespoke, Mids-authored art slice. The chassis is
  identical for everyone; the **hat on top is the entire character read**, parseable by silhouette
  across a crowded horde. Mids can pixel "a decent hat or 15" — which comfortably covers 6 heroes +
  alts/unlocks. The hat system flexes **ascetic → ostentatious**:
  - Wizard = tall pointy hat · Valkyrie = winged helm · Warrior = big bucket helm · Elf = hood ·
    **Monk = do-rag or bald dome** (the *absence* of a topper IS the read — minimalism as character)
    · Cleric = mitre/halo · …
- **Hats = the cosmetic-unlock / live-service currency** (vision.md): cheap to make, infinite to
  collect, pure flavor, zero balance impact — "new hat dropped" is a complete reward, and it's
  content Mids can personally produce forever. (The giant Santa-hat murder-cat boss = just
  big-cat-primitive + a Santa hat. The whimsy is one pixel-hat away, always.)
- **Art-survival-safe:** chassis is parametric (geometry + shader, revertible, Mids' superpower);
  the only hand-made art is bounded pixel hats. NEVER a bespoke-illustration dependency on a
  schedule. Shader-fiddle (glaze/shine/tessellation) to make a tile/avatar read "cold," "inked,"
  etc. is ENCOURAGED (it's parametric play, not paint-grind) — the distinction that matters is
  *shader-tune = good/fast/you; hand-paint-a-texture = the trap*.

Framing: **"inked tabletop game pieces (crosshatched primitives) in funny hats, on a glowing
board."** Legible, whimsical (whimsy = signal), on-theme, and fully Mids-executable.

### Two deliberate fidelity tracks (not a mismatch — the style)

- **Environment / tiles = HI-REZ.** Rich texture maps, detailed walls/floors, the lit-from-below
  stage, crisp status/damage pop-ups — this track carries the production value. SAFE for Mids
  because environment art is **tileable / buyable / generatable / shader-driven** (material work,
  not character illustration); asset packs + shader-fiddle + the Nebulous trick get a lavish set
  with no bespoke-per-thing pipeline. (The reserved pop-up readouts above assume this fidelity —
  a clean pop-up reads as intentional punctuation against a detailed scene.)
- **Characters = SIMPLE.** Kids'-blocks primitives + pixel-art hats (above). The one track kept
  deliberately minimal, because detailed/rigged/animated characters are the pipeline that kills
  projects — and because simple high-contrast actors stay READABLE against the busy hi-rez ground
  (detailed-chars-on-detailed-ground = 40-enemy visual soup).
- **The contrast IS the aesthetic:** **simple painted miniatures on a gorgeously-crafted board.**
  Tabletop/boardgame-honest (the meeple is simple, the board is detailed — intentional, not
  inconsistent), on-theme for the game-show/tabletop framing, and it does double duty as the
  genre's legibility requirement (always pick the heroes/mobs out of the scene).

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
