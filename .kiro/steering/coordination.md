# Regauntless — The Coordination Engine

The shared party pool (`survival.md`) is not just a health model — it's a **coordination
engine.** It *economically forces* teamwork and makes players rally and talk. This is the
core co-op fantasy of the game.

## Multiplied AoE healing/buffs → stack-vs-spread tension

Because survival is ONE shared pool, an **AoE heal multiplies by the number of party members
in its radius** — each hero it touches pours its healing into the single pool. A Cleric's
channel (SRD: *channel positive energy* = burst AoE heal — transcribes perfectly) with 5
heroes clustered dumps ~5× into the pool from one cast.

This creates a genuine, delicious tension:
- **Stack up** → multiply heals/buffs, refill the pool fast.
- **...but** stacking = shared firing lanes (blocking, `combat-space.md`), worse lane/corner
  coverage, and **multiplied exposure to ENEMY AoE** (one enemy fireball on the clump drains
  5× mitigated damage). Heal-efficiency and AoE-vulnerability are the *same* formation.
- **Spread** → hold lanes, cover cracks, suppress generators, dodge enemy AoE — but lose heal
  multiplication and let the pool trickle down.

Symmetric for enemies (enemy healers/AoE → priority targets). Per Option B, each hero's
exposure is mitigated by *their own* stats, so composition × formation × enemy-AoE all
interact through the one pool. Deep, emergent, legible.

## The rhythm: the pool makes players RALLY (and talk)

Multiplied heals + a draining pool force a **spread ⇄ converge pulse**: scatter to fight and
cover, **collapse to heal/buff**, scatter again. Every convergence is a coordination moment:

- **Co-op:** voice-comms rallying — *"Cleric's channeling, COLLAPSE ON ME!"* / *"fall back
  for the group buff!"* The mechanic *creates the conversation* (the holy grail of co-op
  design — the "everyone, NOW" moment, mechanically enforced, not vibe-hoped).
- **Solo + AI party:** the same coordination, issued via **formations/commands** (below)
  instead of voice. Co-op talks; solo dials. Same outcome.

**Zone hostility (−N/tick) sets the tempo** of how often you MUST regroup → the run
*breathes*, and content composes the social rhythm by tuning one number. Game-show-narratable:
*"The team's scattered and the pool's draining — can they rally in time?!"*

This is the cooperative twin of `blocking-banter.md`: the same legible-grid-social philosophy
produces both the arguing ("move left, Valk!") and the rallying ("collapse for the heal!").

## Party cohesion ("formations") — an umbrella fed by MANY SRD sources

"Formation" = the game's umbrella for party-cohesion-around-a-leader. We do NOT invent an
unlock tree — 3.5 already has many "the party coheres around this character" features, and
they ALL feed the one system, each with its own flavor/stringency (follow-the-SRD):

| Source | SRD mechanism | Cohesion style |
|---|---|---|
| **Leadership feat** | followers/cohort; direct command | **stringent** — you *order* the shape (the configurable radial, below) |
| **Bard** | Inspire / bardic music buff | **loose** — cohere to catch the buff |
| **Paladin** | aura (courage/resolve, radius benefit) | **magnetic** — cluster for the carrot |
| *(others TBD — marshal auras, etc.)* | — | various |

Consequences:
- **Multiple classes can be the coordination hub**, each a different fantasy (Leadership-Fighter
  = tight drill-team; Bard = loose inspired mob clumping to the music; Paladin = magnetic huddle
  in the aura). Same coordination *need* (the pool), different class answers.
- All cohesion carrots (auras, inspire, AoE heals) share the **stack-for-benefit vs.
  spread-for-safety/lanes** tension — many class-flavored dials pulling the party together
  against the enemy-AoE/blocking push keeping it apart.
- **Party composition determines coordination STYLE** — a coach-chair/build decision.
- **Feat/feature-sourced, NOT player-slot-sourced** (Prime Directive safe): an AI with
  Leadership can lead; a human taking over a follower doesn't change who commands. Cohesion
  runs off *features*, never off *who is human*.

## The command layer — sized for an ACTION game, not an RTS

Two light layers only. **No real-time RTS micro** (it would drown the fire-hold-brake-pivot
twitch game). Depth lives in the *metagame loadout*, not in APM.

1. **Formations = a configurable RADIAL menu.**
   - **Pre-run (coach chair):** the leader *assigns* which formations populate the wheel;
     Leadership rank/feats (and bard/paladin equivalents) gate slot count / options / adherence
     fidelity.
   - **In-run:** pop the radial, flick to a formation — coarse, fast, a *stance switch*, not
     micro. This is the one live coordination verb: flick "tight" to rally for the Cleric's
     channel, flick "spread" to hold lanes. Formations can be situational (tight/rally,
     spread/screen, column/breach — herd the horde down a lane, box/guard a vulnerable member).
   - Solo-coordination = this flick (the leader's substitute for voice "ON ME!").

2. **Roles = per-AI behavioral profiles, assigned PRE-RUN** (metagame): aggressive/skirmisher,
   defensive/anchor, healer-priority, focus-fire, kiter, etc. Varies each follower's logic.
   Orthogonal to formation (role = *how a hero behaves*; formation = *where the party stands*).
   Good hero AI also must not block allies' lanes (`../design-notes/blocking-banter.md`).

**Deliberately deferred:** a much deeper command vocabulary is possible and flagged as a
future depth axis — but OUT OF SCOPE for the action-core. Dream flagged, core protected
(same discipline as the live-service tier in `vision.md`).
