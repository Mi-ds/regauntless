# Design Note — The Announcer / Host

**Status: core to the Vision (see `../steering/vision.md`). Built as a voice-agnostic
system; specific VO casting is a flagged aspiration, NOT a build dependency.**

## What it is

Regauntless is a D&D game show, and a game show needs a **host** — a Dungeon-Master-as-
game-show-announcer. Charming, theatrical, a little sadistic. Hypes the crowd, calls the
action, taunts you on death, sells the prize rounds, introduces the Dragon.

> *"You might solve the dungeon... but can you solve the DRAGON?!"*

The host is the game's connective tissue and primary personality delivery — `whimsy is
signal` turned to 11. It is the same voice-energy as Gremlin Mode's "can you beat the dev?"
taunt, scaled up into the whole game's identity.

## Build it as a SYSTEM, voice-agnostic

Do NOT architect around a specific voice actor. The announcer is a system that fires lines
at trigger moments; the audio is a swappable asset bank.

- An `Announcer` service listens for game events and plays lines at trigger moments:
  round start, wave spawn, wave clear, prize round / pick-a-door, pickups, near-death,
  player death, boss (Dragon) intro, victory.
- Each line = an entry in a **VO bank** + subtitle text. The bank is swappable: ships fine
  with placeholder/scratch VO, text-only, or (someday) real VO.
- The **triggers and timing are the design**; the audio is a late-stage asset swap. The
  game is fully playable and demoable with zero real VO.

## Aspirational casting — Matt Mercer

The dream host voice is **Matt Mercer** (the modern voice of D&D) — and it's not pure
fantasy: the dev is *personally acquainted* with him (played D&D together back in high
school). That makes it a **genuine-but-long-shot** thread, not a cold celebrity ask —
emphasis on long shot (he's enormously busy; this would be a real professional ask).

**This is a motivating north star, explicitly NOT a dependency.** Same bucket as the hosted
leaderboard: cool, aspirational, never blocks shipping.

The lever that makes the thread even *pullable*: a **demoable game-show loop with the
announcer system in place** (scratch VO, obvious announcer-shaped hole). "Here's my D&D game
show, here's exactly where your voice lives, here it is running" is a pitch. "I have an
idea" is not. So — build the system; the VO ask becomes possible *because* the game exists.
