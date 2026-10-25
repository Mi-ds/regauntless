using UnityEngine;

namespace Regauntless.Controllers
{
    /// <summary>
    /// The phase-1 auto-crawler brain. Produces the SAME <see cref="HeroIntent"/> a human would
    /// (Prime Directive, see .kiro/steering/product.md) — the hero can't tell it apart from a
    /// PlayerController.
    ///
    /// First-milestone behaviour: walk toward the nearest enemy, aim at it, and fire once within
    /// range. Deliberately dumb and deterministic (enemy/ally AI philosophy: predictable baseline
    /// — see .kiro/steering open threads). Positioning/formation smarts come later.
    ///
    /// This is a MonoBehaviour so it can be dropped on the hero in the editor and wired via the
    /// inspector, but its decision logic is trivial and side-effect-free (emits intent only —
    /// never touches world state, per the contract).
    /// </summary>
    public class AIController : MonoBehaviour, IController
    {
        [Tooltip("Stop advancing once within this distance of the target (world units).")]
        [SerializeField] private float _engageRange = 4f;

        [Tooltip("Fire when within this distance of the target (world units).")]
        [SerializeField] private float _fireRange = 6f;

        public HeroIntent GetIntent(IControllerContext ctx)
        {
            var intent = HeroIntent.None;

            var target = ctx.NearestEnemyPosition;
            if (!target.HasValue)
                return intent; // nothing to do — idle

            Vector2 toTarget = target.Value - ctx.SelfPosition;
            float dist = toTarget.magnitude;
            if (dist < 1e-4f)
                return intent;

            Vector2 dir = toTarget / dist;

            // Always aim at the target (decoupled from movement, even if we aim == move here).
            intent.Aim = dir;

            // Advance until inside engage range.
            if (dist > _engageRange)
                intent.Move = dir;

            // Fire when in range.
            if (dist <= _fireRange)
                intent.Attack = true;

            return intent;
        }
    }
}
