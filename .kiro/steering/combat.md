# Regauntless — Combat Resolution & Dice

**We use real d20 SRD 3.5 (OGL) numbers. We do not roll them (except in one mode).**

Governing law: **follow the SRD, don't reinvent** (see `principles.md`) — with the dev's
house-rule override, and the one real-time carve-out below. The turn-based → real-time
translation and the two-clock (tick) model live in `architecture.md`.

3.5's class/feat/stat design is the good stuff — far richer and more modular than 5e for a
game whose goal is "add more as we go." So the data model is honest 3.5: BAB, AC, saves,
ability scores, HD, weapon dice, feats-as-data. What we change is not the *numbers*, it's
the *turn structure* — because the game is real-time and 3.5 is turn-based.

## What dies in the conversion (turn-based → real-time)

- **Initiative: gone.** Initiative only exists to answer "who acts first when everyone acts
  at once?" — a problem *created* by turn-based play. Real-time deletes the question:
  everyone acts continuously on their own clock. Same fate for **attacks of opportunity**
  and **readied/initiative actions** — artifacts of the turn structure.
- **The round survives only as a *rate*.** "BAB +6/+1 = two attacks per 6-second round"
  isn't about order, it's about *how often* you act. That becomes a **fire-rate cooldown**:
  hold pri-fire, shots come out at a cadence derived from the 3.5 iterative-attack economy.
  Higher BAB → more iteratives → faster cadence. We keep the rate, drop the queue.

## To-hit: geometry, not a d20 (in normal play)

**The player is the to-hit roll.** Shots are player-aimed (12-way snapped from analog 360°,
see `vision.md`); lining up the shot *is* the accuracy skill. A visually-connecting shot that
a hidden d20 turns into a "miss" tells the player their aim doesn't matter — unacceptable.

So for Easy/Normal/Hard: **collision is the hit.** The projectile touching the enemy is the
"yes." No to-hit roll. (Gremlin Mode is the exception — see below.)

## Damage: deterministic. Legible breakpoints are the whole game.

A Gauntlet player wants to *know*: "that enemy, that configuration, takes exactly 1 Warrior
shot / 2 Valkyrie shots." That predictability is the skill expression — you read a room, you
know its cost in hits, you plan. **Variance would rob the player of mastery**, not add
texture. So combat is deterministic: fixed damage, fixed HP, integer breakpoints that fall
out of the real 3.5 math.

## The dice policy: one abstraction, four modes

Combat is written **once** against "what value does this die expression yield?" A single
`RollPolicy` / `IDiceResolver` seam answers that. Each mode is just a different
implementation — the rest of combat (BAB, AC, damage, saves) never changes.

| Mode   | Name                                              | Die resolution                          |
|--------|---------------------------------------------------|-----------------------------------------|
| Easy   | Easy                                              | **Maximize** — every die at its max     |
| Normal | Normal                                            | **Take 10** — see below                 |
| Hard   | Hard                                              | **Minimize** — every die at its min     |
| Chaos  | *"You ran the Gauntlet, but can you beat the dev?"* / **Gremlin Mode** 🦊 | **Roll everything, for real** |

These are 3.5's own vocabulary: minimize / take-10 / maximize are where on the die the
fixed value sits. "Easy = Maximize" because maxed player damage crushes breakpoints in the
player's favor (fewer hits to kill). It's a **global** rule, not a player/enemy split.

### Normal mode = "Take 10", resolved per-die

- **d20 rolls** (to-hit comparisons, saves, checks): use a flat **10**.
  `10 + BAB + mod` vs AC, `10 + saveBonus` vs DC. Deterministic comparison, no roll.
- **Damage dice**: each die → its average → **floored**, then summed, **per die** (not
  floor-of-the-total), then add flat mods:

  `damage = Σ floor(avg(each die)) + mods`

  | Die  | avg  | floored | Example      |
  |------|------|---------|--------------|
  | 1d4  | 2.5  | 2       |              |
  | 1d6  | 3.5  | 3       | 2d6 = 3+3 = **6** (not 7) |
  | 1d8  | 4.5  | 4       | 2d8 = **8**  |
  | 1d12 | 6.5  | 6       |              |

### Gremlin Mode (Chaos) — opt-in expert mode

The one mode where the dice gremlin gets a vote again. **Everything rolls for real:** d20
to-hit vs AC, damage dice, saves, nat-20 crits — full 3.5 RNG, warts and spice.

- **To-hit comes back.** Shots can miss. This is the point — it's the "real 3.5, cope" mode.
- Still **one code path**: the Gremlin `RollPolicy` returns real rolls where the others
  return fixed values. The resolver still just asks "does this hit / how much?"
- **Miss feedback:** the projectile still connects geometrically (honor the player's aim),
  but a failed roll pops a **`-miss-`** floating text where the damage number would go.
  No ghost arrows, no magic deflection — the shot landed, the *roll* failed, say so.
- **Gremlin Mode is the only mode where a leaderboard makes sense** — it's the only mode
  with variance to compete over. See `.kiro/design-notes/leaderboard.md`.

## Difficulty modes — the knob is global die-resolution

`enum RollPolicy { Easy, Normal, Hard, Gremlin }` (Gremlin gets a `// 🦊`). The policy sits
**downstream** of the Controller → HeroIntent → Hero pipeline and never touches it. The
Prime Directive stays clean: a Controller emits "attack"; the resolver and its policy decide
what that attack does.

## The circumstance pile — ±5 modifiers, additive, dev-authored

3.5-as-played: a justified "advantage/disadvantage" was a flat **±5** on the roll (= 25% of a
d20), and **every distinct reason stacks** into one additive pile. We keep the math, change only
the *source of reasons*.

- **One pile, additive.** A check's total = `base + Σ(all active ±5 circumstance sources)`.
  Buffs (+5 each) and debuffs (−5 each) live in the **same pile** and sum — no "highest-only,"
  no "doesn't stack." Count the reasons × 5.
- **±5 is the unit, always.** One reason = one ±5 (= 25% swing on a d20). Stackable without an
  enumerated cap. Legible + deterministic → drops straight into the RollPolicy comparison
  (`10 + base + Σ±5` vs DC in Normal, etc.); the sim computes it trivially, the player eyeballs it.
- **Sources are DEV-IMPOSED TRIGGERS, not live-DM improv.** No table to negotiate "the guard
  likes me, +5." Instead the **level/content author pre-binds fiction → ±5** via authored
  tile/zone/state triggers. Examples:
  - **Knee-high water tile** → −5 to movement/saves while standing in it (sloshing, bad footing).
  - High ground / shrine tile → +5 to the relevant roll. Darkness / smoke / mud / ice → each a
    placed trigger with its defined ±5.
- **Terrain carries circumstance modifiers.** This rides the SAME cell/hazard-tile layer as
  water-strips-latchers, vent-fumigate, Web slow-floor, `Hole` (see combat-model.md + the levels
  cell system). A tile isn't just walkable/not — it can carry ±5s applied to whoever stands on it.
  → **the geometry itself tilts your rolls**; authored positional trade-offs (stand in the water
  to shed leeches, but eat −5 saves while you're there). The map is a participant in the d20 math.
- (A future social/announcer/crowd-favor layer *could* grant some "reasoned" +5s — a hook, not now.)

- **Crits — RESOLVED (see "Crits" section below).** They exist in Gremlin Mode via nat-20 AND in
  deterministic modes as a **forced/triggered effect** (not RNG). No longer open.
- **Where BAB/AC live in deterministic play** beyond fire-rate (BAB) and the to-hit
  comparison (AC): AC-as-damage-mitigation is a candidate but not locked.
- **Low-level 3.5 swinginess** (6-HP level-1 death) is a difficulty/starting-level knob, not
  an architecture decision.

## Crits — triggered in deterministic modes, random only in Gremlin

- **Gremlin Mode:** crits happen the real 3.5 way — a natural 20 threatens, confirm, ×multiplier.
  RNG, as intended for the "real dice" mode.
- **Deterministic modes (Easy/Normal/Hard):** there's no to-hit roll, so **no *random* crits** —
  but crits still **exist as a FORCED/TRIGGERED effect.** Certain effects *cause* a crit on a
  specific hit: a spell (True Strike → force a crit on your next attack), a feat, a sneak-attack
  condition (positional), etc. So a crit is something the game *grants*, not something the dice
  roll.
- **What a (forced) crit DOES:** apply the weapon's **crit multiplier to the deterministic damage**
  of that hit (×2 weapon → double the take-10/max/min damage; ×3 → triple; etc.). Legible and
  breakpoint-friendly — a forced crit is just a predictable bigger number, not a variance spike.
- **First trigger on the books:** True Strike (data/spells.json) — reinterpreted from SRD's "+20
  to the next attack roll" (meaningless with geometry-as-hit) to "force a crit on the next
  attack." Future crit-triggers (feats, rogue-style positional sneak conditions) plug in the same way.

## Resolved elsewhere (no longer open)

- **Feats + skill/class progression** — first-class, transcribed straight from SRD (not
  deferred). The character-sheet metagame IS the game for the human. See `vision.md`,
  `principles.md`.
- **Movement** — SRD speed → hexes/round → continuous glide via the time-scale dial; hexes
  fix the 3.5 diagonal kludge. See `architecture.md`.
- **The round** — kept as an exact authoritative 6-second tick (EQ two-clock model), doubling
  as the net-sync heartbeat. See `architecture.md`.
