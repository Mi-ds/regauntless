# Regauntless — Code Map

A living map of the code: what exists, how it flows, and where the designed-but-unbuilt
systems plug in. Prose design lives in `.kiro/steering/`; this is the wiring diagram.

Legend:  `[✓]` built & verified   `[~]` stubbed/placeholder   `[ ]` designed, not built yet

---

## The spine (built & running — commit a7e5746)

```
                 ┌─────────────────────────────────────────────┐
                 │                 THE SIM / SCENE              │
                 │   TestSceneDirector [✓]  (→ SpawnDirector [ ])│
                 │   pushes world knowledge into each hero's    │
                 │   IControllerContext each FixedUpdate        │
                 └───────────────┬─────────────────────────────┘
                                 │ writes NearestEnemyPosition
                                 ▼
   ┌──────────────┐      reads   ┌───────────────────────────┐
   │ IController  │◄─────────────│  IControllerContext        │
   │  (interface) │              │  (what a controller may see)│
   └──────┬───────┘              │  SelfPosition, Nearest...   │
          │ implemented by       └───────────────────────────┘
          │
   ┌──────┴───────────────┬────────────────────────┐
   │                      │                         │
┌──▼───────────┐   ┌──────▼────────┐        ┌───────▼──────────┐
│ AIController │[✓]│ PlayerController│[ ]    │ NetworkController │[ ]
│ walk/aim/fire│   │ (new Input Sys)│       │ (LAN→net, remote) │
└──────┬───────┘   └──────┬─────────┘        └───────┬──────────┘
       │                  │                          │
       └──────────────────┴──────────────┬───────────┘
                                          │ all emit the SAME
                                          ▼
                                 ┌──────────────────┐
                                 │   HeroIntent      │[✓]  PURE, world-space,
                                 │ Move / Aim / Attack│    serializable (goes over wire)
                                 └─────────┬─────────┘
                                           │ consumed by
                                           ▼
                                 ┌──────────────────────────────┐
                                 │           Hero                │[✓]
                                 │  - never asks WHO drove it     │
                                 │  - SnapMovement(6) / SnapAim(12)│──uses──► HexDirections [✓]
                                 │  - rate-limited Fire()         │         (pure 6/12 snap math,
                                 │  - SetController() = drop-in   │          unit-tested green)
                                 └─────────┬─────────────────────┘
                                           │ Fire() currently logs + debug ray [~]
                                           ▼
                                   ┌───────────────┐
                                   │  DummyEnemy    │[~] hits-to-kill placeholder
                                   └───────────────┘
```

**The one rule, enforced:** controllers EMIT INTENT ONLY; only Hero/sim touch world state.
This is why PlayerController and NetworkController slot in later with zero changes to Hero.

---

## As-built inventory (Assets/Scripts, ~400 LOC)

| File | Role | State |
|---|---|---|
| `Controllers/HeroIntent.cs` | pure Move/Aim/Attack struct | ✓ |
| `Controllers/IController.cs` | `IController` + `IControllerContext` seam | ✓ |
| `Controllers/AIController.cs` | phase-1 brain: walk→aim→fire | ✓ |
| `Core/HexDirections.cs` | pure 6-way/12-way snap math | ✓ unit-tested |
| `Core/TestSceneDirector.cs` | sim glue: feeds heroes nearest enemy | ✓ (throwaway) |
| `Heroes/Hero.cs` | consumes intent → move/aim/fire | ✓ (Fire is stub) |
| `Enemies/DummyEnemy.cs` | hits-to-kill target | ~ placeholder |
| `Tests/EditMode/HexDirectionsTests.cs` | 11 green tests | ✓ |
| `Regauntless.asmdef` / `EditModeTests.asmdef` | compile units | ✓ |
| `Scenes/ControllerTest.unity` | the walk+shoot slice scene | ✓ |

---

## Where the designed systems plug in (the roadmap, as wiring)

Each maps to a steering doc. Nothing here is built; this shows the plug-point.

- **Projectiles / lanes / collision** `[ ]` → replaces `Hero.Fire()`'s stub. New `Combat/`
  (projectile with width+shape, lane-centerline travel, one footprint-overlap rule). Hits
  feed the shared pool, not `DummyEnemy.TakeHit`. Spec: `combat-space.md`.
- **RollPolicy (Easy/Normal/Hard/Gremlin)** `[ ]` → `Combat/`, downstream of the pipeline;
  resolves die values for damage/to-hit. Pure, testable, great subagent job. Spec: `combat.md`.
- **Shared party pool** `[ ]` → a run-level service the sim owns; zone-tick drain (on the
  round-tick) + mitigated enemy damage; food refill; binary down. Spec: `survival.md`.
- **Two-clock / round-tick** `[ ]` → `Core/`: continuous FixedUpdate (have it) + an exact
  authoritative 6-second round-tick driving pool drain, round-resolution, net-sync.
  Spec: `architecture.md`.
- **HeroStats (derived, SRD)** `[ ]` → `Characters/`: ability scores/class/level/feats/gear
  → derived stats feeding Hero (speed, mitigation, fire cadence). Replaces Hero's placeholder
  floats. Spec: `principles.md` (follow-the-SRD), `vision.md` (metagame).
- **PlayerController + new Input System** `[ ]` → `Controllers/`: two-stick, camera-relative
  resolution to world-space BEFORE emitting intent. Spec: `architecture.md`, `vision.md`.
- **Camera snap-rotation** `[ ]` → `Core/`: 60° camera snaps, local/cosmetic; input resolved
  camera-relative. Spec: `architecture.md`.
- **Hex tilemap / world** `[ ]` → the actual hex floor (Unity Tilemap, hex cells).
- **Coordination: formations + roles** `[ ]` → radial (Leadership/Bard/Paladin-sourced) +
  per-AI roles. AI positioning must not block ally lanes. Spec: `coordination.md`.
- **SpawnDirector ("generators")** `[ ]` → replaces TestSceneDirector; waves/arenas.
- **Announcer, leaderboard, live-service** `[ ]` → later/flagged. Design notes exist.

---

## Build-order dependency (suggested next steps)

```
 DONE: HeroIntent ─ IController ─ HexDirections(tested) ─ Hero ─ AIController ─ slice scene
   │
   ├─► next-A: real Projectiles + lane collision   (replaces Fire() stub)  ← most visible payoff
   ├─► next-B: RollPolicy (pure, 4 modes, tested)   ← ideal scoped-subagent job
   ├─► next-C: HeroStats derivation (pure, SRD)     ← pure, testable; feeds Hero
   ├─► next-D: PlayerController + Input System       ← makes "take control" real
   └─► then:  round-tick ─► shared pool ─► zones ─► coordination ─► hex tilemap ─► spawn waves
```

Pure/testable pieces (RollPolicy, HeroStats, pool math, lane-collision resolver) are the
good candidates for parallel subagent work — they verify without the editor. Scene/feel work
stays human-in-the-loop (needs eyes + Play; headless editor is license-blocked here).

---

*Keep this current as systems land. Update the `[ ]`→`[✓]` markers and the spine diagram
when a plug-point gets filled.*
