# Design Note — Blocking Banter (legible co-op friction, by design)

**The feel target:** co-op blocking should produce *"MOVE LEFT, VALK!"* banter — the good,
attributable, fixable kind of friction, never the random "why did my shot vanish?!" rage.
Because positions are hex-discrete and everything collides legibly (`combat-space.md`),
players can coach and roast each other in precise grid-language:

- The **Elf** whines the fat-ass Valk is parked on his line when she should shift one hex
  left to her own target.
- The **Warrior** gets told his wide axe-throw is clogging the whole lane ("your shot's too
  big, I can't fire past you").
- The **Wizard** catches heat for his bulk *and* his teardrop ("move your fat ass" / "it's a
  TEARDROP, it threads, YOU move").

Each class's **width/shape = a personality that generates arguments.** The stat is a balance
knob, a positioning constraint, *and* a running joke — whimsy is signal.

## Why this is a spec, not just a gag — two real requirements

1. **Presentation acceptance test.** Blocking must be legible enough that a player can
   instantly see *why* they're blocked and name the fix in grid terms ("your lane / her hex /
   your fat shot"). If players can't banter precisely about it, the presentation
   (`presentation.md`) has failed. The Elf's rant is the test.

2. **AI positioning spec.** In the auto-crawler / phase 1, the *AI* drives these heroes. Good
   hero AI must **not** be the doofus parked on an ally's firing line — it should avoid
   blocking teammates' lanes and move to its own target hex. The Elf's complaint about the
   Valk is literally the `AIController` positioning requirement.

## Future tie-in (flag, not now)

The **announcer / host** (`announcer.md`) narrating co-op screwups — *"Looks like the Elf's a
bit... blocked. Awkward!"* — blocking-banter and host-personality can feed each other.
