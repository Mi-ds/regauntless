# Design Note — Gremlin Mode Leaderboard

**Status: flagged stretch goal. Backend-dependent. OUT OF SCOPE for the current milestone.**

## Why it belongs to Gremlin Mode specifically

The deterministic modes (Easy/Normal/Hard) are *solved puzzles* — everyone who masters the
breakpoints gets the same result, so ranking them is meaningless. **Gremlin Mode has live
dice, so runs differ** — and only then is there something to compete over. "Can you beat the
dev?" literally implies a scoreboard with the dev's name sitting at the top as the benchmark.
The leaderboard *completes the taunt*. 🦊

## The scope warning (read before building)

Everything else in Regauntless is **local, self-contained, offline** — pure client-side game
logic, zero runtime network dependency (we deliberately killed the idea of hitting any
tabletop-ruleset API at runtime). A *hosted* leaderboard breaks that in half. It is effectively a second
project wearing a leaderboard costume:

- **Needs a backend** — server + database + API endpoint + hosting that costs money and has
  to stay up. Infrastructure, not game code. An ongoing operational commitment for a hobby.
- **Needs an anti-cheat posture** — a hosted leaderboard with no validation is a hosted list
  of lies. People *will* fake scores. And in the one mode whose identity is RNG, "prove the
  run was real" is genuinely hard: replay validation, server-authoritative scoring, signed
  runs. This is a real design problem, not a checkbox.
- **Touches every platform's networking** — WebGL CORS, mobile permissions, etc.

## The plan: local-first

1. **Record it here as intent**, explicitly flagged backend-dependent + out of current scope.
   It's a great north-star feature.
2. **Design the score *locally* first.** Gremlin runs produce a score + a **local** high-score
   table from day one. Cheap, offline, no server.
3. **Hosted = a later, deliberate upgrade** — "sync the local score up," with its own session
   to pick the backend, decide the anti-cheat posture, and cost it out. Not a from-scratch
   build, and never a blocker on shipping Gremlin Mode.

This way Gremlin Mode *ships* with a scoreboard (local), and "hosted" is an upgrade, not a
prerequisite — no infrastructure stood up before a single hero walks across a room.
