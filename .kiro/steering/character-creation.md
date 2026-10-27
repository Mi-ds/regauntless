# Regauntless — Character Creation & the Build Model

*Written inherit-safe (intent + rationale, for a maintainer or a scoped content-gen subagent with
no memory of the conversation that produced it).*

**Edition: SRD 3.5 (OGL), not 5e** (see `combat.md`). This is the heart of **metagame-is-the-game**
(`vision.md`): the human's play *is* the character sheet. The field hero is deliberately a floor;
the lift comes from what you built (`.kiro/design-notes/floor-and-lift.md`).

## A character = stat-mods + a pile of FEATS. That's it.

Everything grantable — class features, racial traits, background perks, background drawbacks — is
the **same data primitive: a feat/ability entry.** There is NOT a separate "class-feature system"
and "feat system" and "racial-traits system" — there is **one unified feat/ability list**, and
**"class features" are simply feats a class grants you** (SRD already works this way: bonus feats,
features that read like feats). *Divine Casting is a feat that Clerics/Paladins happen to be
granted.* So:

- **No feat is OWNED by a source.** Source is a **tag**, not ownership.
- **Feats are SOURCE-AGNOSTIC** — a feat gates on its own **prerequisites + data**, NEVER on "which
  class/background/item granted me this." (Same spirit as the Prime Directive's "never branch on
  is-this-player-or-AI." If a feat's effect hard-codes "my class == Cleric," that's a design smell.)
- A character is just the **accumulated feat pile + stat modifiers**, regardless of how each entry
  got there. Multiclass / prestige / unlocks / items all just **grant more feats** into the one pile.

## The three creation axes = grant sources (data bundles)

You pick **Race / Class / Background**. Each is a bundle that injects feats + modifiers into the one
sheet. (All three are *content-as-data* — "add more as we go"; new races/classes/backgrounds are new
bundles, authorable by hand or by a scoped subagent.)

| Axis | Grants |
|---|---|
| **Race** | ability stat mods (±N) · **the build/buy-point pool** (how many points you get to raise abilities) · some granted feats (racial traits) |
| **Class** | granted feats (= the "class features": divine casting, turn undead, sneak attack, rage, flurry…) · **Hit Die** size · weapon/armor **proficiencies** |
| **Background** | another modifier bundle — grants feats (perks) **and may grant negative feats (drawbacks)** · flavor. A background can grant ANYTHING a class can (see Raised-in-a-Cult below). |

## Capability = an OUTCOME, reachable by multiple mechanisms

A gameplay capability (e.g. "cast a divine spell") is an **outcome**, not a possession of one class.
Multiple sheet-paths reach the same outcome via **different mechanisms** — and each path carries its
own **power envelope** (see below). Worked example — three ways to cast a divine spell:

1. **Cleric** — class-granted **Divine Casting** feat → **full divine progression** (full slots,
   full caster level, scales with class level). The devotee.
2. **Warrior "Raised in a Cult"** — the SAME Divine Casting feat, granted by a **background** → but a
   **reduced "dabbler" progression** (e.g. ~half slots, low/fixed caster level, capped spell tier).
   Same prayer/prepare RITUAL, far fewer SLOTS. Access, not mastery.
3. **Rogue** — has NO casting feat at all → uses **Use Magic Device (UMD)** skill to fake-activate a
   divine **item** (scroll/wand/holy symbol). Different mechanism entirely: a skill check on an item,
   no slots, no preparation. The faker.

→ Same outcome, three mechanisms (feat / background-granted-feat / skill+item), each in-character.
This is the same "multiple tools converge on one effect" shape as the latcher-removal ecosystem and
the grab-escape family (`combat-model.md`) — Regauntless keeps building systems where **the answer
you reach for is defined by your build.**

**Mechanism hooks this implies:** items as ability-grant sources; **UMD** as the skill to activate
off-list magic from items (the Rogue's shtick leans on it).

## A grant carries its own POWER ENVELOPE (set by the source)

The grant doesn't just say *that* you have a capability — it bundles **how strong** it is. The
capability is universal; the **magnitude is attached to the grant source.**

- Divine Casting from **Cleric** → full caster progression; from a **background** → thin "dabbler"
  slice; from **UMD** → none (item-gated). Same feat name, different attached **progression/scaling
  table.**
- Generalizes beyond casting (sneak-attack dice, Heal effectiveness-by-ranks, etc.): **the grant
  references a scaling table; different sources point at different tables** (full / half / minimal /
  none). Keeps balance (a background pick never grants class-parity power) and it's pure data.
- **SRD-grounded:** partial/half-casters already exist (Paladin/Ranger cast divine at a reduced,
  delayed, fraction-of-level progression). "Cult-Warrior casts at a reduced progression" transcribes
  an existing 3.5 pattern, not an invention.

## Backgrounds: perks AND drawbacks (packages with teeth)

**A background grants JAGGED, PARTIAL scraps — never a full class kit.** Class = mastery/
completeness (trained-as); background = where-you-came-from, and you picked up *bits of this and
that*. So a background grabs **divergent partial pieces**, deliberately mismatched, with gaps —
e.g. Street Rat gets **Hide but NOT Move Silently**, **Sleight of Hand but NOT Open Lock**. The
**gaps are the character**, and they keep a background from ever reading as "class-lite" (the Rogue
*class* has the complete stealth kit; the urchin *background* has scraps). Same line as Army Brat
(one weapon + camp survival, not the Fighter kit) and Medical (Heal + Self-Sufficient, not the
Cleric kit).

**Magnitude discipline:** not every modifier is a ±5 "advantage." A ±5 is a *circumstance/
advantage* (big, situational); a plain **+2** is a *steady competence bump* (SRD +2-to-skills
style, always-on). Pick the right size — Situational Awareness is a +2 (passive street-eyes), not a
±5.

**Cosmetic gear-lens (purely visual, zero mechanics).** A background can carry a `cosmetic`
treatment that **skins the SAME gear differently** — put a Lesser Noble and a Street Rat in the
identical armor set and the noble's reads **polished/gilt**, the urchin's reads **dingy/grimy**,
with **identical stats.** It's a material/shader param (same separation as `Cell.Variant`:
cosmetic NEVER touches gameplay — determinism, presentation.md), rides the crosshatch-material art
direction (a tint/wear pass, no bespoke art), and doubles as free characterization + a
live-service cosmetic hook. **Hard rule: the sparkle does NOTHING mechanical** (shiny ≠ +AC).

Backgrounds are not pure-upside. A background can pair a **perk** with a **drawback**, making the
pick a real decision. Examples:
- **Pirate:** *Greedy* feat → +10% looted gold/coin **AND** *Unsavory* drawback → −Cha in social
  situations.
- **Medical Background:** start with **Heal 1** AND **Heal becomes a class skill for you** (buy at
  1 pt/rank native instead of the cross-class 2× penalty). [This "seed a rank + reclassify that skill
  to native" is the reusable **Backgrounds pattern** — works for any skill: Scholar→Knowledge,
  Scout→Survival, …]
- **Raised in a Cult:** grants **Divine Casting** (at the dabbler envelope) — proof a background can
  grant what's normally a class feature.

**Disadvantages = NEGATIVE feats** (the clean dual of the feat system). They typically express as
**conditional ±5 circumstance triggers** — e.g. *Unsavory* = "in social situations, −5" — which live
in the **±5 circumstance pile** (`combat.md`). So a drawback is just a feat that adds a conditional
−5 (or a flat penalty / restriction). No separate "disadvantage system."

## Stat generation
Stats are **point-buy**: **Race sets the size of the build-point pool**, you spend it to raise
abilities, then racial stat-mods apply. (Point-buy over rolled stats fits the deterministic/legible
ethos — `combat.md`. CONFIRM if ever revisited; this is the working assumption.)

## House feats can be DIRTY — it's a videogame, not a sourcebook

SRD feats are balanced for a *tabletop economy* (scarce feat slots, careful costing). **We do not
owe that balance.** House-authored feats can be **dirtier, chunkier, double-sided, flavor-first** —
the SRD is our mechanics *vocabulary and tone*, NOT a balance cage.
- A house feat may **do two things at once** / mix a perk and a quirk in **one** feat
  (`kind: "mixed"`), rather than being a timid flat `+2`.
- Example: **Soft Hands** (Lesser Noble background) = **−5 on Str/hardship/manual-labor** AND
  **+5 on Dex-finesse checks** (deft uncalloused hands) — a real tradeoff with a personality,
  in a single feat. The drawback *is* a perk from another angle (very Pip-Boy).
- Balance is handled at the **videogame** level (difficulty modes, encounter tuning, the shared
  pool), not by making every feat individually tabletop-fair.
- **Still honest** (presentation.md voice rule): the *mechanics* stay exact; only the *costing
  discipline* is relaxed. Dirty ≠ lying about the numbers.
- Authors/subagents writing house feats: **lean into flavor + double-sided tradeoffs; don't
  flatten everything to a clean symmetric bonus.**

## SRD-honesty (what's transcribed vs house-added)
- **SRD 3.5 (transcribe):** the feat system, class features, racial mods, Hit Dice, proficiencies,
  skills + cross-class 2×-cost, UMD, partial-caster progressions, point-buy, *Self-Sufficient* feat.
- **House-added (legit under feats/classes/skills-as-data — "add more as we go"; mark as ours, do
  NOT pretend-SRD):** the **Backgrounds layer** (incl. Pirate, Medical, Raised-in-a-Cult), the
  *Field Medic* feat, the *Doctor* prestige class, any new races/classes/backgrounds/feats we author.

## Open knobs (non-blocking, tuning)
- Point-buy pool sizes per race; stat-mod values.
- Whether backgrounds MUST pair perk+drawback, or may be pure-upside (Medical looks pure-upside) —
  leaning **usually paired, not required**.
- The exact "dabbler" caster progression numbers (how-gimped is a background-granted caster).

## Why this converges
One primitive (source-agnostic feats-as-data) + three grant-source bundles + capability-as-outcome +
grant-carried envelopes = a huge, replayable build space that is **all data** (hand- or subagent-
authorable), stays balanced (envelopes gate power by source), and makes "who is this character and
what can they do" an emergent outcome of the player's choices. The sheet is the game.
