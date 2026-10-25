namespace Regauntless.Controllers
{
    /// <summary>
    /// Produces a <see cref="HeroIntent"/> each time it's asked. The ONE seam of the Prime
    /// Directive (see .kiro/steering/product.md): AIController, PlayerController, and (later)
    /// NetworkController all implement this and are fully interchangeable. A hero holds an
    /// IController and consumes its intent — it never branches on "is this player or AI?".
    ///
    /// Contract rules (enforce from day one — see .kiro/steering/architecture.md):
    /// - A controller EMITS INTENT AND NOTHING ELSE. It must not touch authoritative world
    ///   state directly; only the hero/sim does. This keeps networked/host-authoritative play
    ///   possible without a rewrite.
    /// - Intent is produced in ABSOLUTE WORLD SPACE (camera-relative input is resolved here,
    ///   before the intent leaves the controller).
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// Compute this hero's intent for the current tick.
        /// </summary>
        /// <param name="ctx">Read-only snapshot of what the controller may look at to decide
        /// (self + world queries). Kept as an interface so AI and player share one entry point
        /// and so it stays testable without the engine.</param>
        HeroIntent GetIntent(IControllerContext ctx);
    }

    /// <summary>
    /// Read-only view of the world a controller is allowed to see when forming intent.
    /// Deliberately minimal for the first milestone (self position + nearest enemy); grows as
    /// systems come online. Keeping it an interface lets tests feed fake contexts with no engine.
    /// </summary>
    public interface IControllerContext
    {
        /// <summary>This hero's current world position.</summary>
        UnityEngine.Vector2 SelfPosition { get; }

        /// <summary>
        /// Nearest hostile target's world position, or null if none is known/visible.
        /// </summary>
        UnityEngine.Vector2? NearestEnemyPosition { get; }
    }
}
