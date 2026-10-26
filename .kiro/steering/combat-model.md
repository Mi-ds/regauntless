# Regauntless — Combat Model (derived from the SRD-monster walkthrough)

The core real-time-ish combat, worked out by walking through low-CR SRD monsters and mapping
each to Gauntlet behavior. This is the model both the 2D game and the non-graphical sim target.
(Spatial player skills — dodging, sightline play — are REAL in the 2D game but ABSTRACTED to
probabilities in the numbers-sim; see note at bottom.)

## The actor turn = an Action-Point loop running a priority list

Every actor (mob now; AI/human hero later — same framework, Prime Directive) takes a turn by
draining an AP budget through a prioritized action list:

```
while AP > 0:
    sense (e.g. check line of sight)        // FREE — sensing costs no AP
    do the first valid action in the list   // costs AP
    deduct that action's AP cost
    // loop re-senses (free) each iteration → "move THEN shoot" in one turn emerges naturally
```

- **Sensing (LoS check, etc.) is FREE. Only actions cost AP.**
- **Behavior = a per-archetype ordered action list** ({condition, AP cost, effect}); the AP loop
  is the universal engine. Archetypes are DATA filling the same skeleton.
- **Fire rate emerges from AP** (more AP / cheaper actions = more attacks per turn) — not a
  separate stat.

### The default action list (mob)
1. **check line of sight** — free.
2. **if LoS && has-shot → FIRE** (a basic shot; near-universal capability).
3. **else (no LoS) → MOVE** (to gain a sightline / close on the target).
4. **else → overflow / support action** (archetype special, e.g. a lobber's lob).

## NO RANGE — line of sight is the only gate

**There is no shot range. If you can SEE it (clear LoS) and you have a shot, you can hit it at
ANY distance** — the shot travels its lane until it collides. So:
- The spatial game is **sightlines, not spacing.** Walls / corners / blocking bodies are what
  stop shots (by breaking LoS / blocking the lane), NOT distance.
- "Move" exists to **gain a sightline (an angle)** and to **close for contact** — never to "get
  in range."

## Contact damage = automatic, symmetric, a consequence of positioning

When bodies meet (enemy reaches you, OR you charge into one), contact resolves automatically —
it is not usually a *chosen* action, it's what happens when units occupy adjacency:
- **Ghost / Detonator:** on contact with a NON-monster object (a shot, a player) it
  **self-destructs — always, one hit — dealing X× damage** (level scales the DAMAGE, not
  durability). It ignores monster-vs-monster collisions, so ghost swarms flow through each other
  and only resolve against the player's stuff. Your shots intercept it (a shot = a moving ~1-HP
  object; shot-meets-ghost = mutual destruction).
- **Brawler:** real HP; contact = a multi-round melee exchange (visible melee anims); "run over"
  = winning it. Survivable, unlike the detonator.
- **Symmetric & costly:** charging into things hurts YOU too (you take the contact/exchange
  damage). Charging a ghost = cheap (1 exchange); a brawler = costly (several).

## Enemy archetypes (Gauntlet-grounded)
- **Ghost / Detonator** — homes on the player, contact-detonates for level-scaled burst, dies on
  contact with non-monster objects. *Counter: kill before it reaches you.*
- **Lobber** — holds range / kites, throws arcing AREA attacks. *Counter: dodge the zone, or
  close and kill. Baitable (see below).*
- **Brawler** — closes, survivable multi-round melee. *Counter: out-fight at contact, or shoot down.*
- (More to be cataloged as we walk further SRD monsters.)

## Ranged-delivery taxonomy (one primitive, several delivery physics)

An attack/spell picks a DELIVERY TYPE. Both mob attacks and hero/spell attacks reuse these:

| Type | e.g. | Shape | Speed | Gate | Blocked by | Faction |
|---|---|---|---|---|---|---|
| **Flat shot** | player shot, goblin javelin, kobold sling | single lane | fast | **LoS** | bodies + walls (lane) | **faction-filtered** |
| **Lob / burst** | fireball | area (circle/zone) @ a landing hex | slow (~⅓ shot), **ARCS** | targets a hex, **no LoS needed** | nothing (arcs over cover) — **dodge the zone** | **faction-BLIND** |
| **Cone** | cone of cold | cone from caster | ~instant | direction from self | shape fills area | (TBD) |
| **LoS bolt** | magic missile, acid arrow | single target | fast, flies **overhead** | **needs LoS** | walls (LoS), not bodies | (TBD) |

v1 focus: **flat shot + lob** (Gauntlet-grounded). Cone + LoS-bolt arrive with the caster/spell layer.

### Faction rules (the key asymmetry)
- **Flat shots are FACTION-FILTERED:** they **despawn (no damage) on a same-faction body**, deal
  **damage only to the enemy faction**. So friendlies BLOCK and WASTE your flat shots (positioning
  tension — "move, Valk!" survives) but there's **no friendly-fire harm** (can't kill your own team).
  → **Mob flat-shot firing discipline:** a mob HOLDS fire if a friendly/spawner blocks its lane
    (the shot would be wasted). So flat-shooters jockey for clear lanes / hold; **crowding a
    faction's own units suppresses its ranged output.**
- **Lobs / area effects are FACTION-BLIND:** splash hits EVERYONE in the zone (including the
  emitter's own allies and spawners), and **arcs over walls/cover.** Lobbers have **no firing
  discipline** → this enables the emergent exploit below.

## Spawners ("generators")
- **Destructible, priority targets** — killing a spawner stops its spawn stream.
- **Mobs from a spawner do NOT path individually.** They **follow the same path in LINES and pile
  up in lines** (single-file-ish columns), not spreading out with clever pathfinding. This is why
  chokepoints/corners work as kill-zones (the column funnels into one sightline).

## Mob movement — collision-slide-drain, NO pathfinding (the Gauntlet rule)

Mobs have **zero navigation AI**. Movement is three dumb rules; everything else is emergent:

1. **Desired vector** = straight toward the target (the player/party). No pathing, no steering,
   no corner-rounding, no A*. Just a direction.
2. **Actual move = desired vector, collide-and-slide** against walls AND against other bodies:
   - Head-on into a wall → the into-wall component is cancelled → **stop / pile up.**
   - Hit at an ANGLE → into-wall component cancelled, along-wall component carries → **slide
     along the wall.** (The wall does the "thinking" — it's collision math, not intelligence.)
   - Can't enter an occupied hex → **wait** until the body ahead vacates.
3. **Jaggies self-fill:** a body that slides into a notch/gap fills it and sits there, which
   **smooths the wall-line for the mobs behind it.** The first bodies become terrain; the rest
   flow over the smoothed path. **The pile IS the pathfinding** — emergent, free.

### The drain
A pile isn't stuck — it **drains.** The front body (the one with a clear path) moves, **vacating
a hex**; the next advances into the gap, vacating its hex; and so on. The swarm pours toward the
player **first-in-first-out at the rate the front empties.** Geometry is the throttle, not the
mob's brain: fast mobs drain quicker (close the vacated gap sooner); tight corridors drain slow
no matter what. (Exceptions: the **badger burrows THROUGH** walls instead of sliding/piling; a
**ghost/detonator ignores monster collisions** so swarms flow through it.)

## Sensing — what gives a mob a live vector (shared, live, one-round grace)

Movement only happens when a mob HAS a target. Two drivers:
- **Scent mobs (badger, dog):** vector is **always live** — they know the player from spawn,
  move immediately, no waiting on sightlines.
- **LoS mobs (kobold, goblin):** vector lives **only while the player is seen.**

Awareness is **shared across the copies, LIVE, with hysteresis** — and crucially it's **NOT
latching**:
- **Gain LoS** (ANY copy sees the party) → vector goes live **immediately** (shared: since
  they're all copies of one definition, one sighting lights the mob type — but see "why shared is
  fine" below).
- **Lose LoS** (NOBODY sees the party) → vector **persists through the end of the NEXT round**,
  THEN goes dead (freeze / pile). One round of **grace/momentum**, not an instant switch: a
  mob mid-charge finishes its lunge / rounds the corner instead of comically freezing; and you
  **can't cheese the pulse with one frame of cover** — you must stay broken a full round+ to reset.
- No memory, no last-known-position search. "Dead vector" = just stop. It's the current vector
  held one beat longer, nothing smarter.

**Why shared awareness is fine (no hivemind problem):** even when all copies flip live at once,
they **can't all move** — they're piled, blocking each other. Only the front (clear path) moves;
the rest drain behind it as hexes vacate. **The geometry meters the "wave of awakening," not an
alert-propagation rule.** So we don't need per-instance sensing or a "bark" ripple; the pile-drain
produces the same visual (front charges, rest pour after) for free. Simplest wins: shared + live.

## Wave rhythm (emergent — Gauntlet's pulse, not scripted spawn timers)

The loop that falls out of (spawn-pressure + collision-slide-drain + live-non-latching awareness):
1. Party in a safe pocket, **no LoS** → all vectors dead → mobs **pile inert** at spawner/corners.
2. Spawner keeps pumping → pile grows and **overflows** down corridors by pure spawn-pressure
   (bodies shoving bodies), creeping toward the party.
3. Leading edge **rounds a corner into LoS** → vectors live → the built-up mass **drains onto the
   party at once = a WAVE.**
4. Party kills the wave; the moment the last visible mob drops & no LoS remains → (after the
   one-round grace) vectors die → the trickle **freezes / re-piles.**
5. Pressure rebuilds behind cover → **next wave.**

Waves are **emergent, not spawn-timer scripts.** The party can play tempo: sit & break LoS to
**stall and let pressure build** (riskier — bigger wave), or **push the corner** to trigger early
on their terms. "Sit doing nothing → waves crash in" is the player-facing read.

## Emergent mixing — why it's "more than static" (the payoff of refusing AI)

Because movement is just vector + collision + speed, a **mixed spawn** behaves like loose fluid
dynamics, free:
- **Speed-sorting:** fast mobs (dog, 8 hexes) surge to the front; slow mobs clog the tail. The
  leading edge's composition differs from the back and shifts as it drains.
- **Accidental-LoS leaks:** a scent mob (always-live vector) shoving through a pile of blind LoS
  mobs can **physically displace one around a corner into sightline** → that mob's vector goes
  live "by accident" → cascades the drain early. One creature's awareness **leaks** into another's
  via collision — nobody scripted "trigger the swarm"; a badger bumped a goblin into view.
- **Cross-push churn:** bodies shoving bodies → the pile never sits stable; it repacks, re-fills
  jaggies differently each run. A ghost cutting through (ignores monster collision) channels the mass.
- **Result: the corridor is a SYSTEM, not a spawn-list.** Same spawner + geometry, different
  *mess* run-to-run depending on the mix and spawn order. **Spawn-rate and mob-mix are the real
  level-design levers** — you tune the RECIPE, not script the behavior. This is the justification
  for the whole brainless model: legible emergent chaos is what we BUY by refusing pathing/alert-AI.

## Spawners ("generators") — movement note
Spawner-fed columns use the same collision-slide-drain; the "single-file lines + pile-up" above is
just that rule applied at the spawn mouth (bodies extrude and shove the pile outward).

## Emergent tactics (fall out of the rules — not scripted)
- **Corner kill-zone:** establish a SIGHTLINE down a corridor/chokepoint; because shots have no
  range and spawned mobs come single-file, you mow the incoming line (cf. "the Elf charges ahead,
  eats a few ghosts, then holds an angle to block the line of ghosts around the corner").
- **Bait the lobber:** lure a lobber, position behind a wall/corner; its arcing, faction-BLIND
  splash (aimed at your position, ignoring cover) lands on its OWN spawner/allies → turn enemy
  area-fire into a weapon against them. (Emergent from: lob arcs over cover + faction-blind splash
  + lobber targets the player + spawners are destructible.)

## Sim boundary (what the non-graphical tester CAN'T do)
The numbers-sim validates **systems**: AP economy, LoS-gated shooting, breakpoints (shots-to-kill
vs HP), fire-rate-vs-movement, archetype behavior, faction interactions, spawner attrition. It
CANNOT simulate **spatial player skill** — dodging a lob's splash zone, breaking LoS, threading a
sightline, holding a corner angle. Those resolve by positioning in the real 2D game; in the sim
they must be **abstracted to probabilities/assumptions** (e.g. "assumed dodge rate"), and flagged
as such. The sim measures the lift of systems; it does not measure twitch/positioning skill.
