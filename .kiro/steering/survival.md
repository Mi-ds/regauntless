# Regauntless — Survival: The Shared Party Pool

**The answer to the "biggest baddest" question — what is health/the timer?** Gauntlet fused
health, the drain, and the clock into one per-hero number because it was a coin-op,
single-avatar arcade game. We de-conflate the *sins* but KEEP the *soul* ("health is the
timer") by moving it from per-hero to **run/team level.**

## The pool

Survival is **one shared party resource — the pool.** Not per-hero HP bars.

- **Provisioned pre-run** (coach/metagame layer): `pool = team baseline + mcguffin bonuses`.
  Mcguffins = coupons/artifacts/mods, earnable-OR-bought, spent to pad a run's survivability.
  A **PvE currency sink, NOT pay-to-win** (leaderboard-banned; everyone can earn them). This
  is the "feed quarters to brute-force" player buying a fatter cushion — pay-to-**play**.
- **Drains from two sources:**
  1. **Zone hostility tick** — a persistent per-location condition (e.g. **−4 per round-tick**).
     Rides the EQ authoritative 6-second tick (`architecture.md`). This is the *clock* — the
     anti-turtle, forward-pressure, game-show-countdown heir to the Gauntlet drain, but at
     run-level so it doesn't punish precious builds (it depletes the *expedition's supplies*,
     not your lovingly-built hero).
  2. **Enemy damage**, reduced per-hit by the **struck hero's char-sheet mitigation/evasion**
     (AC / armor / saves / feats). **Option B:** stats tune *how expensive a hit is*; there is
     **no individual HP buffer** — a hit that lands drains the pool (mitigated), full stop.
- **Food / pickups refill the pool** mid-run.
- **Binary party state:** fully functional until `pool == 0`, then **entirely down** — run over.
  No per-hero mid-run death. The **team is the unit of survival.**
- **Pool hits 0 → P2P "buy-back-in"** moment (earnable-or-bought currency; PvE-only;
  Gremlin-leaderboard-banned). The game-show "will they spend their winnings to stay in?!"

## The "idiot Wizard" case → it's a MANAGEMENT problem, not a combat bug

A squishy hero standing in arrows drains the shared pool (fast, because bad mitigation). One
careless/dumb-AI hero *can* degrade a run. The fix is **not** in combat — it's from the
coach chair, and the game *surfaces* it as a stakes decision:

- **Swap the AI for a live human** on that slot (Prime Directive controller swap, mid-run) —
  "the robot's too dumb, I'll take the Wizard." This makes **"take control" a diegetic
  solution**, and the phase-1 (auto) → phase-2 (full control) funnel's in-fiction reason.
- **Close the slot** — run 5, not 6. An empty slot is safer than a liability slot. Party
  size is a *choice*.

Legible, attributable, coachable (cf. `../design-notes/blocking-banter.md`): "pool's
draining — it's the Wizard AI eating arrows — take him or bench him."

## Why this keeps per-hero identity (it must)

The player controls *a character* — micro-stats (STR/DEX/CON), feats, class all matter,
because they set **per-hit mitigation/evasion** and everything combat. Heroes are distinct,
SRD-derived, not interchangeable sponges. The pool is shared; the *cost of defending it* is
per-hero and skill/stat/positioning-driven.

## What this fuses / replaces

- Replaces the rejected per-hero Gauntlet HP-drain (punished builds, fought our economy).
- **The pool IS the clock** — no separate show-clock needed; the drain-tick provides urgency,
  the zero-point is the fail state. Gauntlet's "health = timer" soul, reclaimed at run level.
- Zone hostility (−N/tick) is a **content-authorable tempo/difficulty dial** — and the
  metronome of the spread/converge coordination rhythm (see `coordination.md`).
