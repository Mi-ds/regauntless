# Regauntless — Combat Space: Lanes, Projectiles & Collision

How shots travel and what they hit. Everything is **hex-discrete and collidable** — one
uniform rule, legible by design (see `presentation.md`, `combat.md`).

## Positions are discrete: centers and lines

Both heroes and projectiles live on the same discrete lattice as movement:
- A **hex center**, or
- A **line** (edge-midpoint between two adjacent hexes).

Nothing floats at an arbitrary sub-hex point. A projectile marches its lane
center → line → center → line… Because hexes are offset, "the midpoint a hex away is the
centerpoint of the two farther up" — i.e. a line-position aims into the hex ahead; lanes
converge through the honeycomb rather than running as independent parallel columns.

## Projectiles: width (and shape)

A shot travels down its **lane centerline** and occupies the middle **N%** of the lane
width. **N is a per-class/weapon stat = the hero's spatial identity** (Gauntlet-grounded:
the axe Warrior couldn't fit through cracks the Wizard/Elf could).

| Hero | Body-width of shot | Shape / leading edge | Threads cracks? | Lane behavior |
|---|---|---|---|---|
| Elf | ~20% | thin arrow | yes, easily | coexists freely, but thin = whiffs gaps |
| Valkyrie | ~30% | balanced bolt | mostly | mild crowding |
| Wizard | ~50% | teardrop (narrow tip) | yes — tip threads despite bulk | blocks in open lane, threads gaps |
| Warrior | ~60%+ | blunt/wide (axe) | **no** — lane-hog | crowds neighbors, clashes readily |
| +2 TBD | — | — | — | — |

**Percentages are tunable; the relationships + shapes are the fixed design** (fix ratios,
tune absolutes — same pattern as the SRD time-scale dial). Elf < Valk < Wizard < Warrior in
body width; Wizard threads despite bulk, Warrior cannot thread.

**Two widths from one shape** (only matters where relevant):
- **Threading width** = the *leading-edge* profile → how small a gap/crack it enters. Pointy
  (arrow, teardrop) = small → penetrates/threads. Blunt/wide (axe) = large → can't thread.
- **Collision/body width** = the widest part (the N%) → shot-vs-shot and shot-vs-body overlap.

So a teardrop "blocks like 50% but threads like an arrow." Head-on clash behavior falls out
of the leading edge: **pointy presents a small profile → clashes less / punches through;
blunt presents a large profile → clashes/blocks more.** This is *universal in mechanism,
per-class in outcome* — NOT a special-case rule.

## Bodies

Hero/enemy **bodies are ~50–60% of a hex wide** (tunable in that band). Fat targets:
converging shots land; a body in your lane blocks your shot.

## The one collision rule: footprint overlap in a shared lane

**Everything is collidable. Interaction = do the two footprints overlap in a shared lane.**
Boolean, discrete, legible. The same rule covers every case:
- **shot ↔ body** (hit an enemy; blocked by a teammate; blocked by a mob frontline)
- **shot ↔ shot** (opposing shots in a lane overlap → they collide/cancel)
- **shot ↔ terrain** (a "crack" is a lane partly occluded by walls, leaving < 100% open; a
  thin/pointy shot fits, a fat blunt shot is stopped) — terrain is just another occluder.

No special-casing for friendly fire / pass-through: **it all collides.** This does not
*prevent* blocking — it makes blocking **universal, discrete, and predictable**, so it reads
as positioning skill and coordination, not a random shot-eater. (This is the answer to the
"players get annoyed blocking each other" worry: legible blocking = coordination, not chaos —
and it's class-flavored: thin Elves coexist in tight lanes, fat Warriors must take point or
their own lane.)

## Worked case: Elf on a line, flanked by two shooters

An Elf caught **on a line** with two mobs converging fire on him: the line is where enemy
lanes *converge*, so **both shots land on his ~55% body** — and if he fires **forward**, his
thin ~20% arrow **threads the gap between the mobs and hits nothing.** With 12-way decoupled
aim he can at least pivot to trade with *one* flanker instead of whiffing forward. 

**Design reading (intended, not a bug):** the line-position is double-edged — a clever
brake-pivot/threading spot (see `architecture.md`) *and* a trap when flanked. Positioning
carries real risk/reward; standing on a line while flanked is punishing. Legible and fair
because the lanes and bodies are all visible and discrete.

## Relationship to the rest

- **Discrete + legible** → serves the determinism/breakpoints philosophy (`combat.md`) and
  "legible through feel" (`presentation.md`).
- **Aim is 12-way, decoupled** from movement/position (`vision.md`), so a hero can be on a
  line or mid-step and still fire any of 12 directions.
- **Gauntlet-grounded** (axe can't thread, wizard/elf can) — honoring the ancestor, same way
  we follow the SRD for rules (`principles.md`).
