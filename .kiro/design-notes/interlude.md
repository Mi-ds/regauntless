# Design note — The Interlude (between-runs / between-levels metagame)

*Inherit-safe. A captured design GOAL + candidate systems — deliberately NOT built yet (see the
guardrail at the bottom). Origin: Mids was playing a pixel crawler with an FF13-style branching
skill-web and the between-runs loop was DEAD — "stare at the web, scan for anything affordable I
missed, spend, dive again." Passive, static, lonely. The itch: the interlude should be an ACTIVE
SPACE, not a menu-stare.*

## Why it matters (not optional flavor)
Regauntless's core thesis is **metagame-is-the-game** (vision.md, floor-and-lift.md): for the human,
the character sheet IS the game. That thesis **lives or dies in the interlude.** If "manage your
build between runs" is a passive web-stare, the thesis is hollow. The interlude is where the human
actually *plays* the part of the game that's theirs. So making it active is a first-class goal, not
polish.

## What the interlude should CONTAIN (candidate contents, two kinds)
**Management (quiet, always-available housekeeping — make it feel like play, not a spreadsheet):**
- spend skill points; buy new feats; slot/swap feats
- change equipped loadout (weapon/armor — the .sheet `build.loadout`)
- swap loaded spells (the caster prep step)
- reconcile the sheet generally (the "character maintenance" beat)

**Events (variable, reactive — what makes each interlude DIFFERENT, vs the same static menu):**
- some number of **random events** between levels (offers, risks, encounters, choices with texture
  — the "react to a situation and spend resources on a DECISION, not just a purchase" energy)
- **shopping** (a shop/economy — flagged by Street Rat's Five-Finger Discount, Pirate's gold)
- **stamina-gated mini-games** (forage / train / gamble / craft / scavenge) — see Stamina below
- the **clinic/rest step** (ability-damage recovery +1/level, cures, consumables — combat-model.md,
  Street Rat)

The combo = housekeeping-made-pleasant + variable-events-to-react-to → you're DOING something
between runs, not scanning a web. This is also the natural home for every "flagged future system"
the backgrounds referenced (shop/economy, consumables, stamina, traversal).

## STAMINA (planned) lives HERE, not in combat
Stamina is a **per-hero INTERLUDE resource** — "how much can you DO during the between-floors
interlude" (gates the mini-games/activities). It is **NOT a dungeon/combat/AP resource** (do not
reconcile it with the combat AP-loop — different layer entirely). Referenced by the "Average Normal
Player Character" background (+20% max stamina = ~20% more interlude actions; the null-background's
one gloriously minor gift). Not built yet; exact mini-games TBD — pin only the CATEGORY
(interlude-activity budget), leave the activities unspecced.

## Tone
The interlude is also the **game-show backstage / green-room between rounds** (vision.md framing):
could be diegetic — the announcer, sponsors, next-round modifiers, banking winnings — rather than a
flat menu. Pip-Boy-voiced (presentation.md).

## GUARDRAIL — core-loop-first (this is a rabbit hole)
A rich between-runs metagame is **infinitely expandable and can swallow the project.** Per
vision.md's sequencing rule: the **dungeon crawl must be proven Diablo-replayable FIRST.** The
interlude is captured here as a GOAL with candidate systems; it is built AFTER the core loop, not
instead of it. Today it correctly exists only as **flagged hooks** (stamina, shop, clinic,
recovery) seeded across the data — not a system. Resist building it early.
