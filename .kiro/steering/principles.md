# Regauntless — Governing Design Principle

## Follow the SRD. Don't reinvent it.

The **d20 SRD 3.5 (OGL)** is a complete, battle-tested, 20-years-balanced game design
sitting right there. **It is the design authority.** When a question is "how should the
*rules* work?" — feats, prerequisites, skill ranks, multiclassing, gear, class progression,
derivation of stats, saves, crits — the answer is **look it up in the SRD and implement it
faithfully.** We do not invent, redesign, or re-balance mechanics. We *transcribe*.

This is the escape hatch for nearly every design fork: the trap is "how do *we* want to do
X?"; the escape is "what does 3.5 already say?" Almost every open question dissolves into
transcription.

### Mechanics vs. Presentation — the dividing line

- **Mechanics = SOLVED by the SRD.** Zero invention, zero balancing, zero debate. Transcribe.
- **Presentation = OUR actual job.** How the character sheet looks and feels to manipulate,
  how feats are browsed/picked (tree vs. list is a *presentation* choice, not a mechanics
  one), the shop UI, the juice, the readability. This is where creativity goes — governed by
  `presentation.md` ("modern tech, 1985 concepts / legible through feel / whimsy is signal").

The SRD is the engine; presentation is the show.

### The ONE carve-out: real-time translation

The only place we may deviate is where **real-time play makes a turn-based rule literally
inapplicable** (initiative, the round economy, 5-ft-step-doesn't-provoke, etc. — see
`combat.md` and `architecture.md`). When we must deviate, we **translate the SRD's intent
minimally — never reinvent.** Translate the round into clocks/rates; keep the ratios sacred.

### The OTHER carve-out: allow leniency for Mids making stupid 🦊

The SRD is the default authority, **not a tyrant.** The dev (Mids) retains **house-rule
override authority** — exactly as a real DM overrules the book at the table whenever they
want. If following the rules-as-written produces something un-fun, broken-for-our-context, or
Mids just has a better/sillier idea, **Mids wins.** "Follow the SRD" is the *default*, not a
cage. House rules are legal, in-character (it's a tabletop game — the DM always wins), and
expected. Document deviations when they're deliberate so future-us knows it was a choice, not
a bug.

**Priority order:** Prime Directive (see `product.md`) > Mids' house rules > SRD-as-written >
our invention (which should approach zero).
