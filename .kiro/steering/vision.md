# Regauntless — The Vision™ (north star)

**Regauntless is a D&D-themed game show.** A televised dungeon crawl. Contestants (the four
heroes) run the gauntlet for *big money, big prizes* while a host hypes the crowd and the
dungeon tries to eat them.

> *"You might solve the dungeon... but can you solve the DRAGON?!"*

## The lineage (what this actually is)

We said "Gauntlet" at the start. That was the coat of paint. The real DNA, in layers:

- **D&D 3.5 (OGL)** — the rules engine and the fiction. Classes, feats, dragons, dungeons,
  *real* numbers (see `combat.md`).
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

Smash TV's structure wearing a D&D-game-show costume:

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

## World — true top-down, hex

- **True top-down.** No ¾ slant, no isometric diamond. Flat overhead read (OG Gauntlet's
  flatness, not its grid).
- **Hex world.** The dungeon floor is built of hexagonal tiles that tessellate seamlessly
  ("snap together like a Go board"). Hexes are the floor you stand *in* (faces), not
  intersections.
- 6 and 12 are *the* hex numbers (6 edges, 6 vertices) — which is why movement is 6-way and
  shooting 12-way. The control scheme is hex-native by construction.
- Open: pointy-top vs flat-top orientation; whether movement is continuous glide along a
  snapped heading vs discrete hex-hops (leaning continuous glide — twin-stick, not
  boardgame). To be confirmed.

## The Host / Announcer

A game show lives or dies on personality. There is a **host** — a Dungeon-Master-as-
game-show-announcer. Sadistic-but-charming, hypes the crowd, taunts on death, narrates the
spectacle. This is `whimsy is signal` turned up to 11 — the host is connective tissue, not
decoration. See `../design-notes/announcer.md`.

## Status of the Vision

This document is the **north star**, captured mid-flight during the session that reframed
the game from "Gauntlet clone" to "D&D game show." Several elements are settled (controls,
world geometry, lineage, game-show frame); several are flagged open (run structure, prize
rounds, hex orientation, movement glide-vs-hop). Build toward this; refine the open parts as
we plot.
