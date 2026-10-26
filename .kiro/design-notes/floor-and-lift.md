# Design Note — The Base Hero Is a Deliberate FLOOR

**A lone base hero struggling (or losing) is not a balance bug — it's the premise.**

The crawl-sim's first finding was that a lone level-1 Fighter (18 Str) *loses* to 3 goblins
on Hard, and that the weapon die barely moves the needle at low tier. That is **working as
designed.** The auto-crawler was never "one basic fighter solos everything." The entire point
of the rogue-lite / party / metagame layer is that you climb OFF the floor through the
**systems around the fight**, not by the base hero being strong enough alone.

## How you overcome difficulty hiccups (the levers — NOT base-stat balance)

- **Find / unlock / buy better weapons & gear** (progression + the earnable-currency economy).
- **Limited-duration consumables** (e.g. a potion adding +1d6 elemental damage) — loadout
  choices, currency sinks.
- **Party buffs** — aura / inspire / concentration spells from other members (the coordination
  layer: paladin aura, bard inspire, cleric — see `coordination.md`).
- **Party composition / varied damage sources** — add a rogue (sneak attack) or caster (spell
  damage) for different damage *types/amounts*. (Also the organic answer to DR/resistance
  walls — bring the damage type that applies.)

## Why the floor is load-bearing

If a lone base hero could faceroll Hard, the whole metagame would be pointless — no reason to
unlock, buff, bring a rogue, or manage loadout. **The base hero's struggle is exactly the gap
that makes all the growth systems matter.** Keep the floor a floor.

## What this means for reading sim / balance data

We are NOT tuning "is the lone fighter balanced." We are measuring **how much each
system/lever lifts you off the floor**:
- How much does a better weapon / elemental potion move win rate?
- How much does adding a rogue or caster help (and against what)?
- How much does an aura/inspire buff swing a fight?

**Metric of success = each progression/party/loadout lever produces a satisfying, measurable
improvement** over the deliberate base floor. The floor is the baseline we measure lift
*against*. This governs how ALL future combat/sim data is interpreted.
