# Design note — Run-loss model (= the SRD CR/EL/resource-attrition framework, transposed)

*Inherit-safe. IMPORTANT FRAMING: this is NOT a novel Regauntless invention. It is the **SRD 3.5
CR / Encounter-Level / resource-attrition** system — stated outright in the SRD and playtested for
20+ years — **transposed** to our real-time, shared-pool, unlockable-party rogue-lite. Follow-SRD,
don't reinvent (principles.md): the SRD is the difficulty authority; our job is the transposition.
We re-derived this bottom-up by watching the sim (Grukk vs a Goblin), then recognized it's just
what CR already means.*

## What the SRD already says (the authority)
- **Challenge Rating (CR)** is a calibrated difficulty number: a CR-appropriate encounter is one a
  party of 4 suitable-level PCs can beat while spending **~25% of their resources**. It's a
  **deliberate attrition unit**, not a coin flip.
- **Encounter Level / the "adventuring day"**: you face a *string* of encounters; each drains the
  tank; **the day ends (you must stop/retreat) when resources run dry.** Loss is usually
  **attrition across encounters**, not one heroic death.
- **Over-CR encounters** are *defined* as probably-deadly / likely-unwinnable for the party.
- **DR, saves, special defenses, SR** are *gates*: they decide **who can even hurt/affect** a
  creature — bring the wrong tools and you simply can't crack it.
- CR is a **legible** number — the system tells you, up front, roughly how hard a thing is.

## The transposition (SRD concept → our system)
| SRD | Regauntless |
|---|---|
| "~25% of daily resources per CR-appropriate encounter" | **shared survival pool** drains per room (survival.md); zone-hostility **bleed** is the clock |
| "the adventuring day ends when resources are spent" | **the run ends when the pool bleeds out** |
| over-CR encounter = deadly/unwinnable | **DPS wall / can't-crack-it lock** (incl. the deterministic to-hit wall below) |
| DR / saves / SR gate who can hurt it | **the boss DR / stat / tool check** |
| CR as a legible difficulty number | our **legible deterministic breakpoints** (combat.md) — you can *read* whether a wall is crackable |
| 4 CR-appropriate PCs | **unlockable party slots + classes + feats** — the toolkit you assemble to meet the CR |

## The deterministic-to-hit corollary (what the sim surfaced)
Our deterministic combat (combat.md: no to-hit *roll* in Easy/Normal/Hard) makes the SRD's
"over-CR = can't win" show up as a **hard, legible WALL**: a lone or under-tooled hero whose
`value + mods` can't reach a mob's AC **simply cannot connect** — infinite whiff (the real sim
result: L1 Grukk +4 vs Goblin AC 15 take-10 → never hits; Goblin likewise → deadlock). This is
**intentional and correct** — it's the SRD saying "you need the party / the right tools for this
CR." A **lone** hero stalls; the **party breaks the wall**:
- **Flanking** (+2 SRD) — a 2nd hero flanks → crosses the AC gate (14→16 ≥ 15). Positioning is the crack.
- **Auras / buffs** (Paladin, Bard inspire) — stack to-hit/damage over the wall.
- **Auto-hit magic** — Magic Missile ignores AC entirely → the caster is the answer to a can't-hit wall.
- **More bodies / focus fire** — sheer party action.
- **DR-piercing / the right damage type** — the boss-gate answer.

So AC stays a **real gate** (we did NOT turn it into mitigation); the *answer to a gate is
teamwork*, which is the shared-pool/coordination thesis (survival.md, coordination.md) expressed at
the to-hit layer — and it makes **unlockable party slots/classes genuinely matter** (a 1-slot run
*can't* crack some walls; unlocking the right 2nd hero is a real, legible power gain).

## How runs END (the three loss conditions — all expressions of the above)
1. **DPS wall / lock** — mobs you can't crack (AC/HP/DR beyond your assembled tools). You stall.
2. **Location bleed / timeout** — the pool drains in hostile space while you're stalled or slow;
   a stalemate is a **losing clock** (can't kill it → it bleeds you out). Time is the real killer;
   the wall just makes you spend it.
3. **Boss check** — clear the level, reach the boss, lack the DR-pierce / stats / tools to win. A
   capstone gate.

## Failure is DIAGNOSTIC (the rogue-lite loop)
You rarely die to a dice fluke — you lose because **your build/party wasn't enough for this wall**,
and the deterministic legibility tells you *exactly which* wall ("couldn't reach the AC", "bled out
stalling", "boss DR I couldn't pierce"). → adjust the build / unlock the slot or class that answers
it / try again. The wall **teaches**; the metagame is **acquiring the keys to the walls that stopped
you.** (This is why legible breakpoints + unlockable party are load-bearing, not decoration.)

## The SIM's job (CrawlSim / SimRunner)
Verify the transposition **preserves SRD balance**: that our CR<1 bestiary's walls are
**crackable-with-the-right-party-tools** (a stacked party's flank/aura/auto-hit/DR-pierce actually
clears the AC/HP/DR range), and that the pool-bleed clock turns stalemates into losses on a fair
timeline. If a wall is *uncrackable even with correct tools*, that's a tuning bug (we broke SRD
balance in transposition); if it's crackable only with the right tools, that's working as the SRD
intends. Run real sheets vs real mobs and read the numbers.
