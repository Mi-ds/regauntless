using UnityEngine;

namespace Regauntless.Controllers
{
    /// <summary>
    /// The per-tick command a <see cref="IController"/> produces for a hero to consume.
    /// This is the whole ballgame (see .kiro/steering/structure.md): AI and player controllers
    /// emit the SAME HeroIntent, and the hero never asks who produced it.
    ///
    /// Dual-stick, decoupled (the Smash TV core verb): Move and Aim are independent.
    /// Input is analog 360°; presentation-snapping (6-way body / 12-way aim) happens
    /// DOWNSTREAM of this struct — intent carries raw analog world-space directions.
    ///
    /// IMPORTANT (Prime Directive + multiplayer seam, see .kiro/steering/architecture.md):
    /// intent is ABSOLUTE WORLD-SPACE. Any camera-relative resolution happens in the
    /// controller BEFORE the intent is produced. Keep this struct pure and serializable —
    /// it is also the thing that will travel over the wire for networked players.
    /// </summary>
    [System.Serializable]
    public struct HeroIntent
    {
        /// <summary>Desired movement direction in world space, magnitude 0..1. Zero = no move.</summary>
        public Vector2 Move;

        /// <summary>Desired aim/shoot direction in world space (unit-ish). Decoupled from Move.</summary>
        public Vector2 Aim;

        /// <summary>Fire requested this tick (hold-to-fire; brake-and-pivot lives higher up).</summary>
        public bool Attack;

        // (later) bool UseItem; and command/formation signals — add when earned.

        /// <summary>An intent that does nothing. Use instead of default when intent is explicit.</summary>
        public static HeroIntent None => new HeroIntent
        {
            Move = Vector2.zero,
            Aim = Vector2.zero,
            Attack = false
        };

        public bool HasMove => Move.sqrMagnitude > 0.0001f;
        public bool HasAim => Aim.sqrMagnitude > 0.0001f;
    }
}
