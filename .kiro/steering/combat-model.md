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
- **Latcher / Clinger** — fast rusher that JUMPS ON a hero and rides, draining; hand-placed in
  clumps (not spawner-fed). *Counter: an ALLY must clean it off (you can't self-clear once your
  hands are full). See the Latcher section.*
- (More to be cataloged as we walk further SRD monsters.)

## Status effects (per-avatar flags; SRD condition → our FEEL)

SRD on-hit conditions (poison, disease, paralysis, fear, …) rarely map 1:1 — our heroes are
**statless on the field** (HeroStats = move/fire/armor; the character sheet is the metagame). So we
**define the FEEL**, house-rule style (SRD is authority on *when* it triggers — the save/trigger —
we own *what it does* to a Regauntless avatar).

**General shape** — every on-hit status is per-avatar and non-pool, described by:
```
{ baseEffect      : applied when the flag goes OFF -> ON (clean target)
  overflowEffect  : applied when re-hit while ALREADY ON (can't "turn it more on")
  selfCure        : feel-tuned duration; REFRESHED on each re-apply; clears itself }
```
- **Per-avatar, NOT the shared pool.** The survival pool is the collective health-clock; status
  sticks to the individual hero who got hit (the Warrior is poisoned; the Elf isn't). Pool =
  shared survival; status = personal, positional consequence.
- **Non-damage** (poison at least): status DEGRADES function, it doesn't chip HP. It kills you
  *indirectly* (broken tempo → caught by the swarm), which is scarier than a tick.
- **Binary flag + overflow, NOT an additive accumulator** (house style — no stacking counters).
  First hit sets the flag; further hits spill into the overflow effect.

**Poison (first defined status):**
- **baseEffect:** −10 ft / −2 hexes movement speed while poisoned.
- **overflowEffect:** re-hit while already poisoned → **micro-stun** (brief AP/tick loss). A swarm
  (centipedes) can chain-stun a poisoned hero — that's the swarm threat, not a deeper slow.
- **selfCure:** **~40s default** (FEEL knob; stored as rounds/ticks internally so it survives
  tick-rate changes and the numbers-sim can use it). Each new bite **refreshes** the timer →
  lasts through the fight + a little spillover, then clears. "While you fight these, maybe the
  next room too."

(Future statuses reuse the same shape; we DEFINE each one's feel as we meet the SRD creature that
carries it.)

## Latcher / Attach archetype (the "get it off me!" enemy)

Built from the SRD **Attach** ability (weasel/leech/tick/stirge). These are NOT spawner chaff —
they're **hand-placed in the dungeon, in clumps of 3-5, a few times per map** (ambient ambush
pockets). Each is a **fast rusher that jumps onto a hero and RIDES** — persistent state that
travels with the hero (first "the enemy is *on* you" mechanic; contact that doesn't resolve by
position each round but sticks). Individually trivial (a weasel is 2 hp, ~1 dmg); the MECHANIC
carries it, not the stats.

### Capacity = hands (the ladder is anatomical, not a counter)
A hero has **hands** (default 2 — a per-hero STAT so classes/feats/forms can vary it). You can
*manage* (grapple/fend) up to your hand-count of latchers; **any beyond that bite freely.** The
effect per attached latcher is a **fixed lookup by count** (no accumulator math — just a table):

| # attached | effect ADDED | why (anatomy) |
|---|---|---|
| 1 | **Distract** — defensive debuff: you take MORE incoming damage (SRD "loses Dex to AC" → you're tangled/exposed) | one hand occupied |
| 2 | **+ Slow** — movement suffers | both hands full wrestling |
| 3 | **+ DoT** — real, uncontested bite damage | no hands left → it eats you |
| 4 | **+ Slow** — harder to walk | hanging off a leg |
| 5 | **+ Slow** — nearly mired | other leg |

So a full clump of 5 = distracted + bleeding + barely mobile = **totally dependent on a teammate
to clean you.** Danger is **CONTEXTUAL:** a latcher alone is nothing; a latcher on you DURING a
wave is lethal (the Distract rung amplifies all the other incoming fire). Creates a live
priority call — keep fighting the wave, or peel off to help the ridden hero *because* they're
now taking amplified damage.

### Removal = a co-op demand (but with escape valves)
The latcher's whole reason to exist is to make the party reach into each other's kits — BUT a
full-hands hero isn't strictly helpless; there are escape valves:
- **Ally's shot** scrapes one off (brute, universal — anyone can help). *[OPEN knob: does the
  cleaning shot also HURT the ridden hero (rough scrape, spicier) or clean-remove (no downside)?
  — Mids undecided.]*
- **Caster self/ally (hands-INDEPENDENT):** anti-latcher spells (Bigby-flavored "plucking
  fingers" / fumigate) are deliberately **Verbal-only / Focus** — NO somatic component — so a
  caster can cast their way out **even with both hands full of weasels** (a voice isn't a hand).
  This makes the martial "can't self-clear with full hands" rule **MARTIAL-SPECIFIC**, and makes
  the squishy Wizard/Cleric the party's **weasel-insurance** (self-cleanses AND rescues the
  bruiser). Role-defining.
- **Environment (the MAP is a participant):** a latched hero can route to terrain that strips
  latchers — e.g. a **water tile** (wash/drown them off) or a **vent/gas tile** (environmental
  fumigate). A small set of SPECIFIC cases (not a system), and **per-latcher-type** (water helps
  a leech, drowns a weasel). Ties to the cell/hazard system (cf. Hole cell + authored set-piece
  chunks in levels.md) — now terrain has a combat purpose. Rewards spatial awareness as a
  self-rescue even for a full-hands martial.
- **(Future)** high-hand-count or extra-attack classes natively manage more.

(The spell answers live in the class/spell layer — NOT specced here; the latcher CREATES the
demand, real SRD spells FILL it later. What's banked now is the archetype + the hands-capacity
primitive + the removal-demand hooks. OPEN knobs: scrape-hurts-hero?, default hands value.)

## Web (two-stage: slow your FEET, then slow your GUN) — the corner-breaker

From the SRD monstrous spider's **Web**. Our version drops the SRD net-entangle/hidden-sheet and
uses a clean **two-stage** model (same {base, overflow} grammar as poison/latcher): *first
application paints the floor; re-application onto an already-webbed hero escalates onto the
person.*

- **Delivery = Web-lob:** an **air projectile** (arcs overhead — like a lob but its own type),
  lands at the target's hex.
- **Stage 1 — SPLAT (terrain):** paints **2 sticky hexes** — the target's hex + the hex BEHIND
  it (along the web's path, so it gums your RETREAT). While standing on a sticky hex: **−80%
  movement** (you crawl at ~20%). It's **floor, not a root** — step off and you're free (no
  escape check). **Persists**, and **decays ~40s after the spider that threw it DIES**
  (caster-linked — kill the spider and its webs rot on a timer, so clearing the room clears the
  gunk; ~40s is the same feel-knob as poison, stored as rounds/ticks).
- **Stage 2 — OVERFLOW (on the hero):** if a web lands on a hero **already standing on web**, it
  mostly lands on THEM → becomes a **sticky status ON the hero: ATTACK-slowed** (fire/attack rate
  cut), travelling with them; clears on timer / spider death.
- **Two different axes (the elegance):** Stage 1 wrecks **movement** (can't reposition/escape),
  Stage 2 wrecks **offense** (can't shoot). A doubly-webbed hero is **pinned AND defanged.**
  Player read: *"web slows your feet — but RUN, because the next one slows your gun."*
- **Faction:** slows heroes AND other mobs; the **throwing spider is immune to its own web**
  (treads it at climb speed) → you can bait OTHER mobs into it, but the spider owns the terrain.
- **ROLE — corner-breaker / zone-control:** Web is NOT a damage tool; it's **positioning denial**
  that feeds the wave/corner/push loop. The spider is the ANSWER to the hold-a-sightline-and-
  drain-the-wave tactic: it gums your footing so you can't hold cleanly and (as a tanky bruiser)
  **pushes your line back** off the corner, re-webbing as it advances → forces constant
  repositioning → keeps re-exposing heroes to the horde. Classic scene: the party runs, the
  rear-guard is stuck on web taking hits while the rest pull back, everyone shuffles around the
  corner slowed, re-establishes the hold... until the spider pushes them off it again.
- **Contextual** (like poison/latcher): web alone is nothing; web *while holding a line against a
  horde* is what wrecks you.
- **Hunter vs web-spinner (behavior flag):** SRD splits spiders into **hunters** (roam, +10 ft
  speed, NO web weapon — pure fast venomous brawler) and **web-spinners** (hold/trap, HAVE
  web-lob). One stat chassis, a flag picks which.

## Ranged-delivery taxonomy (one primitive, several delivery physics)

An attack/spell picks a DELIVERY TYPE. Both mob attacks and hero/spell attacks reuse these:

| Type | e.g. | Shape | Speed | Gate | Blocked by | Faction |
|---|---|---|---|---|---|---|
| **Flat shot** | player shot, goblin javelin, kobold sling | single lane | fast | **LoS** | bodies + walls (lane) | **faction-filtered** |
| **Lob / burst** | fireball | area (circle/zone) @ a landing hex | slow (~⅓ shot), **ARCS** | targets a hex, **no LoS needed** | nothing (arcs over cover) — **dodge the zone** | **faction-BLIND** |
| **Cone** | cone of cold | cone from caster | ~instant | direction from self | shape fills area | (TBD) |
| **LoS bolt** | magic missile, acid arrow | single target | fast, flies **overhead** | **needs LoS** | walls (LoS), not bodies | (TBD) |
| **Web-lob** | monstrous spider web | lands @ target hex → **2 sticky hexes** (target + hex-behind) | slow, air projectile (arcs, not a true lob) | targets a hex | — | slows heroes AND mobs; **throwing spider immune to own web** |

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

> **NOTE:** this section describes per-mob sensing as first worked out. The later **"Spawners —
> the SPAWNER is the brain"** section REFINES this: for the common case the *spawner* holds the
> target/aim and mobs just "run forward." Read both; where they differ, the spawner-brain model
> wins. (Scent-as-direction and the LoS one-round grace below still hold.)

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

## Spawners ("generators") — the SPAWNER is the brain, mobs are dumb "run forward"

**REFINEMENT (supersedes the per-mob "always-live vector" framing in Sensing above for the
common case):** intelligence lives in the **spawner**, not the individual mob. A mob's entire
instruction is **"run forward"** (a heading, not a target). It does not know where the player is,
does not path, does not think — it runs the direction it's pointed and everything else
(V-stack, spread, collision-slide-drain, pile, cascade) is emergent collision on top of that.

### Default: dumb extrusion
Most mobs (ghosts, goblins) get **no aimed vector at all.** The spawner extrudes them **directly
in front of itself**; each new spawn **shoves the pile outward** by pressure; they sit **inert
until there's a reason to move.** Pure pressure-fill + overflow. (This IS the Gauntlet room-fill:
the generator keeps pushing bodies out, they fill the room by extrusion pressure, going nowhere
until the door opens / the player is sighted and the mass rushes.)

### The spawner acquires a target (LoS-gated) and aims
- **LoS only** triggers targeting. (A **sense** like scent may hand the spawner a rough
  *direction* earlier — per-creature-type property, the minority case — but the real target lock
  and the aim is LoS.)
- When one of the spawner's mobs gains **LoS on the player**, the spawner locks onto **the CORNER
  / opening where that sighting happened** — a *geometry waypoint*, NOT the player (it can't aim
  through a wall). It **turns its extruder to point at that corner** and stamps every mob it makes
  with **"run straight at the corner."** The horde becomes a hose pointed at the chokepoint; mobs
  pour to the corner, cascade around it, each picking up its own LoS on the far side.

### Aim ratchets toward the player (monotonic, dumb rule)
- **Retarget rule:** whenever any mob achieves **LoS-to-player from a point CLOSER to the player
  than the current aim-waypoint**, the spawner moves its aim there. **Monotonic** — the aim only
  ever walks *toward* the player, corner-by-corner (A→B→C...), dragged forward by the horde's own
  progress. No pathfinding; just "closest-LoS-to-player wins."
- **Self-correcting:** if the player repositions, stale waypoints stop producing closer-LoS and
  the next closer sighting re-aims naturally.
- **Reverse pressure (beating back the spawn):** if the party out-DPSes the spawn rate, the front
  **recedes** → sightlines reach deeper → the aim-ratchet walks the contested point back UP the
  corridor toward the source → eventually the **spawner itself falls into a clean lane and dies.**
  The chokepoint war has a win condition built into the geometry.
- **Kill the spawner = decapitate the aim.** Destructible + priority. The waypoint chain dies with
  it; leftover mobs revert to dumb "run forward / inert."

### Pack flag (orthogonal, LoS-gated second-stage)
"Pack" is a per-creature flag layered on top. Stage 1 (universal): all mobs are V-ish footprints
that stack/spread by collision. Stage 2 (pack only, once LoS is gained & the front rounds the
corner toward the player): pack mobs **go AROUND a blocked same-kind ally instead of queuing**, so
**more than one line cascades around the corner** — defeating the single-file funnel. Scent is NOT
sufficient to trigger the pack stage; **LoS is.** (Collision/footprint GEOMETRY — the V shape, how
wedges deflect followers, hex tiling — is Mids-owned spatial work, flagged TBD; the fox implements
once Mids drives it.)

## Emergent tactics (fall out of the rules — not scripted)
- **Corner kill-zone:** establish a SIGHTLINE down a corridor/chokepoint; because shots have no
  range and spawned mobs come single-file, you mow the incoming line (cf. "the Elf charges ahead,
  eats a few ghosts, then holds an angle to block the line of ghosts around the corner").
- **Bait the lobber:** lure a lobber, position behind a wall/corner; its arcing, faction-BLIND
  splash (aimed at your position, ignoring cover) lands on its OWN spawner/allies → turn enemy
  area-fire into a weapon against them. (Emergent from: lob arcs over cover + faction-blind splash
  + lobber targets the player + spawners are destructible.)
- **Bait-and-flank (the classic Gauntlet co-op moment, EMERGENT):** "Valk, stand by that wall and
  distract the ghosts." Valk presents LoS at one corner → spawner's aim **ratchets to Valk's
  corner** → the whole horde floods HER (she's not a scripted aggro-magnet; she's just the
  closest-LoS point, and tanky enough to hold it). Meanwhile the Wizard flanks a DIFFERENT corridor
  — empty, because the aim is committed to Valk — gains LoS on the **spawner itself**, and (no-range)
  shoots it dead "from below." Falls out of: spawner-aims-closest-LoS + mobs-run-forward +
  no-range-LoS-fire + destructible-spawner. Nobody coded a "distraction mechanic."
- **Converge-and-grind (war of attrition, EMERGENT):** Elf holds a corner and mows the stream but
  CAN'T out-DPS the spawn rate alone → stalemate (winning the trade, hose never dries). Party goes
  around the south wall and **shoots up to support** — a second LoS lane converging on the same
  stream. Combined DPS now **exceeds spawn rate** → the front **recedes** → (aim-ratchet in reverse)
  sightlines reach deeper → the **spawner is beaten back into a clean lane and killed.** Falls out
  of: out-DPS-recedes-the-front + closest-LoS-retarget + destructible-spawner.
- These two are the **thesis proven twice:** signature co-op Gauntlet plays reproduced as pure
  consequences of simple rules — zero scripting. (Also validates the roles: tanky Valk eats the
  hose while squishy Wizard/Elf do safe-angle work; the shared survival pool makes the trade
  affordable — see survival.md / coordination.md.)

## Spawner movement note (collision)
Spawner-fed columns use the same collision-slide-drain; the "single-file lines + pile-up" above is
just that rule applied at the spawn mouth (bodies extrude and shove the pile outward).

## Sim boundary (what the non-graphical tester CAN'T do)
The numbers-sim validates **systems**: AP economy, LoS-gated shooting, breakpoints (shots-to-kill
vs HP), fire-rate-vs-movement, archetype behavior, faction interactions, spawner attrition. It
CANNOT simulate **spatial player skill** — dodging a lob's splash zone, breaking LoS, threading a
sightline, holding a corner angle. Those resolve by positioning in the real 2D game; in the sim
they must be **abstracted to probabilities/assumptions** (e.g. "assumed dodge rate"), and flagged
as such. The sim measures the lift of systems; it does not measure twitch/positioning skill.
