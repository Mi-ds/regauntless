using UnityEngine;

namespace Regauntless.Enemies
{
    /// <summary>
    /// A stationary dummy for the first vertical slice — something for the AI hero to walk to
    /// and whack. Health here is a local placeholder ONLY; real survival is the shared party
    /// pool (see .kiro/steering/survival.md) and enemies will get discrete hit-breakpoint
    /// visual states (presentation.md) later. This just registers hits so we can see it work.
    /// </summary>
    public class DummyEnemy : MonoBehaviour
    {
        [SerializeField] private int _hitsToKill = 2;
        private int _hitsTaken;

        public bool IsAlive => _hitsTaken < _hitsToKill;

        /// <summary>Register a hit. Returns true if this hit killed it.</summary>
        public bool TakeHit()
        {
            if (!IsAlive) return false;
            _hitsTaken++;
            Debug.Log($"{name} hit ({_hitsTaken}/{_hitsToKill})", this);
            if (!IsAlive)
            {
                Debug.Log($"{name} DOWN", this);
                return true;
            }
            return false;
        }
    }
}
