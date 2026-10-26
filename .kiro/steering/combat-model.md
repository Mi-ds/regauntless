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
