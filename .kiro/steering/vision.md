# Regauntless — The Vision™ (north star)

**Regauntless is a tabletop-fantasy game show** — built on the **open d20 SRD 3.5 (OGL)**
ruleset. A televised dungeon crawl. Contestants (the six heroes) run the gauntlet for
*big money, big prizes* while a host hypes the crowd and the dungeon tries to eat them.

> *"You might solve the dungeon... but can you solve the DRAGON?!"*

> **LEGAL POSTURE — read this.** We use the **open mechanics** (d20 SRD 3.5, released under
> the OGL — that's what the SRD is *for*). We do **NOT** use "D&D" / "Dungeons & Dragons"
> (a Wizards/Hasbro trademark) or any Product Identity (named PI monsters like beholder,
> mind flayer, etc.). Generic fantasy (dragon, skeleton, goblin, warrior, wizard, elf) is
> fine. Describe the game as an **"SRD / d20 fantasy game show,"** never as "D&D." The
> TTRPG-fan *association* is delivered legitimately by **paid talent** (see announcer),
> not by borrowing marks. See `../OGL-NOTICE.md`. (Fox is not a lawyer; a real IP review is
> warranted before any commercial/public-scale release.)

## The lineage (what this actually is)

We said "Gauntlet" at the start. That was the coat of paint. The real DNA, in layers:

- **The d20 SRD 3.5 (OGL)** — the rules engine and the fiction. Classes, feats, dragons,
  dungeons, *real* numbers (see `combat.md`). The *open* mechanics — never branded "D&D".
- **Smash TV (1990, Williams / Eugene Jarvis)** — the **mechanics**. Dual-stick decoupled
  aim, clear-the-arena rooms, pick-a-door between rooms, hordes pouring from every edge,
  over-the-top power fantasy. This is the actual gameplay ancestor, not Gauntlet.
- **The game show** — the **framing device** wrapping both. A host, prize rounds, an
  audience, spectacle, a run = an episode/season. This is the connective tissue.
- **Gauntlet (1985)** — demoted to tonal/visual *great-grandparent*. The fantasy-arcade
  flavor and the "generators" vocabulary survive; the maze-march structure does not.

The name still fits: **re-gauntlet + relentless** — and now also reads as the *name of the
show*. Season after season of contestants running the gauntlet.

## Structure (game-show vocabulary)

Smash TV's structure wearing a tabletop-fantasy game-show costume:

- A **run** = an episode / season of the show.
- Each room = a **round** — survive the arena (Smash-TV wave combat).
- Between rounds = a **prize round / pick-a-door** — winnings are 3.5 loot / level-ups /
  feats. Progression arrives as *winnings*, game-show style.
- The finale = **the Dragon** — the marquee boss round. "Can you solve the dragon?!"

Open: run length (fixed season vs endless/escalating), what a prize round yields, how
pick-a-door branches. To be plotted.

## Controls — dual-stick, decoupled (the Smash TV core verb)

Movement direction and aim/shoot direction are **independent**. Strafe one way, hose fire
another. This is the heart of the feel and it is non-negotiable.

- **Input:** analog 360° (thumbstick, or the AI's computed vector). No snapping at input.
- **Presentation/resolution snap** happens on the sprite, at different resolutions per
  system:
  - **Movement: 6-way** — snapped to the 6 hex directions (edges). Body faces one of 6.
  - **Shooting: 12-way** — snapped to 12 directions (6 edges + 6 vertices, 30° apart). Aim
    is movement's resolution *doubled* — you can shoot angles you can't walk.
- Both feed the **same `HeroIntent`** whether from `PlayerController` or `AIController`.
  Prime Directive (see `product.md`) stays pristine — the decoupling is in the intent, not
  in who produces it.
- **Signature mechanic — fire-hold brake-and-pivot:** movement is hex-discrete steps;
  *holding fire* can freeze the hero mid-step on "the line" (trading progress for the shot),
  and *releasing* resumes toward whatever direction is held at release (neutral = original
  target). Fire-hold doubles as a juke/re-aim. It's the hex-native evolution of the Gauntlet
  tap-move/tap-fire interleave technique. Full model in `architecture.md`.

## World — true top-down, hex

- **True top-down.** No ¾ slant, no isometric diamond. Flat overhead read (OG Gauntlet's
  flatness, not its grid).
- **Hex world.** The dungeon floor is built of hexagonal tiles that tessellate seamlessly
  ("snap together like a Go board"). Hexes are the floor you stand *in* (faces), not
  intersections.
- 6 and 12 are *the* hex numbers (6 edges, 6 vertices) — which is why movement is 6-way and
  shooting 12-way. The control scheme is hex-native by construction.
- **Camera snap-rotation (Q/E):** the player snap-rotates the *camera* in 60° clicks (6
  hex-native angles). The **world stays fixed / absolute** — only the view turns. Controls
  are **always camera-relative** ("up" = up-on-screen), resolved to world-space before
  intent; the hero owns its own persistent world-facing (camera never turns the hero). See
  `architecture.md`. **Pointy-top vs flat-top is therefore a dead question** — just the
  camera's default angle.
- Open: whether movement is continuous glide along a snapped heading — **resolved: glide**,
  at SRD speed via the time-scale dial (see `architecture.md`).

## The Host / Announcer

A game show lives or dies on personality. There is a **host** — a Dungeon-Master-as-
game-show-announcer. Sadistic-but-charming, hypes the crowd, taunts on death, narrates the
spectacle. This is `whimsy is signal` turned up to 11 — the host is connective tissue, not
decoration. See `../design-notes/announcer.md`.

## The Metagame IS the game (for the human)

**The human's primary loop is the character-sheet metagame — the beloved "between
sessions" tabletop stuff.** Labor splits cleanly:

- **The AI plays the session** — runs the crawl, executes real-time combat (the "at the
  table" part).
- **The human plays the character sheet** — levels up, shops, manages gear, allocates
  feats, builds the party (the "between sessions" part tabletop players love *most*).

This is the part a video game does *better* than the table: shopping, respec, planning a
feat build, optimizing gear — bookkeeping friction at the table, pure dopamine with good UI.
**This is what the "real SRD 3.5 numbers" decision was FOR** — in a twitch arcade game that
depth is nearly wasted, but as a *build-craft metagame* the full 3.5 richness is the entire
toy and finally pays full rent.

Consequence (settled, per `principles.md` — follow the SRD): stats are **derived** from
class + level + ability scores + feats + gear (NOT flat authored numbers — you can't have a
build-craft metagame on flat stats). **Feats + skill/class progression are first-class
systems, transcribed straight from the SRD.** How they're *presented* (sheet, shop, feat
browser) is our creative job; the mechanics are the SRD's.

### Demo = the auto-crawler (and the funnel)

Phase 1 (AI-driven heroes) isn't just a dev stepping-stone — it's a **shippable
auto-crawler / idle-battler** and the **demo**. Engagement = **unlock + configure +
build-craft**: unlock party slots (grow the active party toward 6), unlock classes (reveal
the roster), level and gear them. You're the **coach / producer**; the AI are the
contestants. Then — Prime Directive — **"take control" is the upsell / full-game phase 2.**
You've lovingly built your roster watching them compete; now grab the stick. The conversion
is built into the fiction (audience → contestant). Demo progression should carry into the
full game (on-ramp, not a walled sampler).

## The roster — SIX heroes (PC-first)

Not four. **Six** playable classes — and 6 *rhymes with the hexagon* (6 edges / 6 directions;
whimsy is signal). Four was a Gauntlet/couch-hardware holdover; PC-first retires it. The four
Gauntlet archetypes (Warrior / Valkyrie / Wizard / Elf) + two more SRD classes (candidates:
Cleric, Rogue — TBD). All six are driven by `Controller`s (AI or human, interchangeable), so
the roster works at every player count: single-player = 1 human + 5 AI; co-op = N humans +
(6−N) AI. Roster size, party size, and co-op player count are the same number (6) but filled
by any mix of Controllers. Level-design must keep 6 heroes + hordes readable (a design
constraint, not a capacity one).

## Multiplayer (PC-first, phased — see `architecture.md`)

Host-authoritative; remote devices join as networked `Controller`s (bring-your-own-device,
Jackbox-style). Roadmap: **single-player + AI → LAN/WiFi co-op (first multiplayer) →
internet co-op (later)**. The host-authoritative 6-second tick doubles as the net-sync
heartbeat (EQ model). Couch same-screen co-op is demoted. All later-phase; core-loop-first.

## Business / Live-Service North Star (funded-product stage — aspirational)

> **FLAG:** This entire section is a *funded-product-stage* vision — studio-scale, with
> servers, a storefront, recurring talent deals, and a content pipeline. It is NOT a hobby
> milestone. It informs **architecture today** (so the ambitious version stays possible) but
> must never be *built* before the core loop earns it. See the sequencing rule below.

**The pitch, in three reference points:**
- **Fortnite** — the live-service *engine*: seasons, rotating content, a storefront, the
  game as a *platform* not a one-time purchase.
- **Diablo 2** — the replayability *core*: a crawl loop so good you run it a thousand times.
  Build variety, loot, escalating difficulty, "just one more crawl." This keeps players *in*
  between content drops.
- **TTRPG celebrity culture** — the acquisition + monetization *hook*: a passionate, spendy
  fanbase that will pay for *their* personality as announcer/avatar. The culture is the
  marketing channel most games can't buy.

**Guest Hosts as live-service content.** The game show rotates guest hosts — different
personalities headline different crawls/seasons, dropping over time. A "host" is a
**data-driven content pack** (VO bank + name/likeness/avatar + host-specific lines). This is
the game-show format made literal, and the $ unit, the content unit, the cosmetic flex, and
the marketing event are **the same object** (the personality promotes their own pack to
their own audience).

**Season = Battle Pass = Show Season.** The season/battle-pass is the spine of the loop and
it IS the fiction: a "Season of Regauntless, hosted by [X], big prizes on the line" is
simultaneously the battle pass *and* the narrative frame.
- Modeled on Fortnite's *self-funding* pass: first pass is a small paid entry (~$15, often
  on sale); **completing a pass yields enough premium currency to buy the next pass plus a
  surplus.** Engaged players self-fund forever after one purchase; surplus flows into
  cosmetic/content. **The pass monetizes RETENTION, not power** — it rewards *playing*.

## Monetization Principles (law, even at hobby stage, because design depends on it)

- **One pseudo-currency, earnable by play OR purchasable with cash** (early-DDO /
  Turbine-Points model). Real money and earned currency buy the *same* things.
- **It buys COSMETIC + CONTENT only** — host packs, crawls, avatars, announcer voices,
  visual flair. **NEVER power, stats, XP boosts, or convenience-that-fixes-fake-friction.**
- **Pay-to-win is banned AND architecturally impossible.** Combat is deterministic with
  legible breakpoints (see `combat.md`); selling a damage boost would *break the breakpoint
  puzzle that is the game*. The ethical choice and the design-integrity choice are the same
  choice — the game literally *cannot* be P2W without ceasing to be the game.
- **The earn path must be REAL and GENEROUS** — a committed f2p player unlocks meaningful
  content without paying; pass completion is reachable by normal engaged play. The
  earn-rate / pass difficulty is tuned to *feel fair*, **NEVER as a conversion lever.** This
  is the exact line between early-DDO (good) and modern-DDO (extractive). State it with teeth.

## Anti-cheat posture: structural immunity, not enforcement

Learned from Fortnite's relaxed stance on Creative/AFK XP farms: **don't police HOW players
earn — police the RATE.** Set a rate floor (effort/time per unit of reward) and let players
earn however they enjoy, including degenerate/AFK/creative methods.

- **This is only safe because the economy is 100% cosmetic/content with zero P2W** — faster
  earning has *zero competitive blast radius*. There is nothing worth cheating *for* that
  affects anyone else. We lean on **structural immunity** (nothing worth cheating for), not
  **enforcement** (expensive, adversarial, alienating cheat-fortress).
- **The one exception: the Gremlin Mode hosted leaderboard.** A shared *competitive ranking*
  is the one place cheating has real blast radius (a fake #1 ruins it for everyone). So:
  **relaxed everywhere, strict only on the competitive leaderboard.** Two postures, because
  they protect different things — a personal cosmetic economy (don't care) vs. a shared
  ranking (do care). See `../design-notes/leaderboard.md`.

## Sequencing rule (protects the project from its own ambition)

**Core-loop-first, always.** The crawl must be *Diablo-replayable* with zero celebrities,
zero live-service, zero storefront, fully offline, before ANY of the above plumbing is
justified. Fortnite's model only works because the game is fun to return to. Building
live-service/store/currency before the core loop is proven = building the cash register
before the restaurant has food. The ambition informs architecture (**everything is a
data-driven, hot-loadable content pack** — hosts, crawls, enemies, classes, loot); it does
not get *built* ahead of the loop.

## Status of the Vision

This document is the **north star**, captured mid-flight during the session that reframed
the game from "Gauntlet clone" to "tabletop-fantasy game show" with a full live-service /
monetization vision. Settled: controls, world geometry, lineage, game-show frame, legal
posture, monetization principles, anti-cheat posture, core-loop-first sequencing. Flagged
open: run/season structure, prize rounds, hex orientation, movement glide-vs-hop. The
business/live-service tier is explicitly funded-product-stage — it shapes architecture now
(data-driven content) but is not built ahead of the core loop. Build toward this; refine the
open parts as we plot.
ne the open parts as
we plot.
