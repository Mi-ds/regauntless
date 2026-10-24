# Regauntless — Combat Resolution & Dice

**We use real SRD 3.5 (OGL) numbers. We do not roll them (except in one mode).**

3.5's class/feat/stat design is the good stuff — far richer and more modular than 5e for a
game whose goal is "add more as we go." So the data model is honest 3.5: BAB, AC, saves,
ability scores, HD, weapon dice, feats-as-data. What we change is not the *numbers*, it's
the *turn structure* — because Gauntlet is real-time and 3.5 is turn-based.

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

In Gauntlet **the player is the to-hit roll.** Shots are 8-directional and player-aimed;
lining up the shot *is* the accuracy skill. A visually-connecting arrow that a hidden d20
turns into a "miss" is **anti-Gauntlet** — it tells the player their aim doesn't matter.

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

## Later / unresolved (do not over-build yet)

- **Crits** exist naturally in Gremlin Mode (nat 20). In deterministic modes they currently
  don't trigger — revisit if we want a deterministic crit rule.
- **Where BAB/AC live in deterministic play** beyond fire-rate (BAB) and the to-hit
  comparison (AC): AC-as-damage-mitigation is a candidate but not locked.
- **Low-level 3.5 swinginess** (6-HP level-1 death) is a difficulty/starting-level knob, not
  an architecture decision.
