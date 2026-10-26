# Regauntless — The Art Survival Constraint (HARD RULE)

**Regauntless must NEVER depend on another human producing art on a schedule.**

This is the single most important survival constraint on the project, and it is earned the
hard way. Every prior substantial project of Mids' died at the **art layer** — never at
design, never at tech:

- **Nebulous** (space game): died on art. ~$5k and a year hiring graphic artists for
  ships/stations; hit the money cap, gave up. (The *tech* was EVE-starscape competitive; the
  *commissioning* killed it.)
- **Digital CCG** (had a full dev bible): died on art. Tried to get family ("artsy"
  step-mother / sisters) to paint cards; they couldn't produce on a schedule.
- Pattern: **design finishes, engineering finishes, art-on-a-schedule-from-someone-else does
  not.** Three graveyards, same cause.

So the risk model for Regauntless is clear: the design won't kill it, the tech won't kill it
— **art dependency will, if we allow one.** We do not allow one.

## The rule

1. **No art on anyone else's schedule, ever, on the critical path.** The game must be fully
   shippable and *good-looking* with art that requires ZERO scheduled external delivery.
2. **Allowed art sources** (all require no one-else's-deadline):
   - **Non-artist cleverness** — the signature. Lit-from-below (`presentation.md`), systems
     that make cheap assets look expensive. (Mids has *proven* this hits EVE-tier.)
   - **Generated / procedural / generate-and-curate** (the Nebulous *working* method).
   - **Cheap / asset-store / AI / placeholder** drop-ins.
   - **Placeholder-forever-capable:** the game must remain shippable on programmer-art.
3. **The variant system is the art socket** (`CellTypeDefinition`, label-first/sprite-optional).
   Art plugs into clearly-defined, bounded slots on a game that already works without it.
4. **Human art is a BONUS polish-swap on a finished framework — never a dependency.**
   - The nephew (now in a college graphic-design dept, dormant-on-standby) is the one art
     relationship that could work — activated by a **concrete** pitch: *"the framework exists,
     it just needs assets, here's a percentage deal."* Percentage, not up-front money (dodges
     the Nebulous cap). But even he is a bonus: **ship without him; he polishes a real thing.**
   - The game being *real* is what unlocks human art — not the other way around. Build the
     framework; realness activates the dormant artist. Never gate the framework on the artist.

## Why this governs architecture

This is *why* the project leans on: lit-from-below, data-driven variant drop-ins,
generate-and-curate pipelines, and "shippable on placeholder art." Those aren't just nice —
they are the **break-the-death-cycle insurance.** Any proposal that puts "someone must paint
X on a deadline or the game doesn't ship" on the critical path is REJECTED by this rule.

> Paid for three times (two dead projects + ~$5k). It governs.
