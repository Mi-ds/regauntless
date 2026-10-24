# Design Note — The Announcer / Host

**Status: core to the Vision (see `../steering/vision.md`). Built as a voice-agnostic
system; specific VO casting is a flagged aspiration, NOT a build dependency.**

## What it is

Regauntless is a tabletop-fantasy game show (built on the open d20 SRD — never branded
"D&D", see `../steering/vision.md` legal posture), and a game show needs a **host** — a
dungeon-master-as-game-show-announcer (the TTRPG-referee archetype, generically). Charming,
theatrical, a little sadistic. Hypes the crowd, calls the action, taunts you on death, sells
the prize rounds, introduces the Dragon.

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

## Hosts are live-service CONTENT (not one baked-in voice)

The host is not a single hardcoded announcer — it's a **swappable content pack** and the
core unit of the live-service vision (see `../steering/vision.md`). A host pack =
VO bank + name/avatar/likeness assets + host-specific lines. Different personalities headline
different crawls/seasons. Architecturally this just *is* the voice-agnostic system above,
loaded at runtime (data-driven, hot-loadable). Build the system; packs are content.

## Talent: two DISTINCT tiers (different contracts, prices, likelihoods)

Do not blur these — budgeting and expectations depend on which one:

1. **Voice acting — "Featuring the voice of [Name]"** *(realistic target).* Hiring a VA to
   record announcer lines is bread-and-butter work; pros do it even for projects they're not
   passionate about. A "featuring the voice of" credit is a standard VO deal (or small add-on)
   and delivers ~90% of the legitimacy signal. **This is the attainable tier.**
2. **Likeness + name as marketing endorsement** *(aspirational stretch).* Using a person's
   name/face/likeness *as "the host" in marketing* is a right-of-publicity / endorsement
   deal — pricier, bigger contract, and a *harder yes* even with money (performers guard
   their endorsement more than their voice: a stamp-of-approval implies "I stand behind
   this"). **This is the moonshot tier.**

The TTRPG-fan association is delivered legitimately by **paid talent** on either tier —
never by borrowing trademarks.

## Aspirational casting — the "Mercer-like" dream host

The dream is a marquee TTRPG voice (e.g. the modern voice-of-the-hobby archetype) — and
there's a real thread: the dev is *personally acquainted* with one such performer (played the
tabletop game together back in high school). That makes it **genuine-but-long-shot**, not a
cold celebrity ask — emphasis on long shot (enormously busy; a real professional ask).

**Motivating north star, explicitly NOT a dependency** (same bucket as the hosted
leaderboard). The lever that makes the thread *pullable*: a **demoable game-show loop with
the announcer system in place** (scratch VO, obvious announcer-shaped hole). "Here's my
fantasy game show, here's exactly where your voice lives, here it is running" is a pitch;
"I have an idea" is not. Build the system; the ask becomes possible *because* the game exists.
