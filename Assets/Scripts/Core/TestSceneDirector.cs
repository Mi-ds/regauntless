using UnityEngine;
using Regauntless.Heroes;
using Regauntless.Enemies;

namespace Regauntless.Core
{
    /// <summary>
    /// Minimal glue for the ControllerTest slice. Each tick it computes the nearest living
    /// enemy to each hero and writes it into the hero's context. This keeps the Controller seam
    /// clean: the SIM supplies world knowledge; the controller only READS it to form intent
    /// (never queries the world itself). A real spawn-director/perception system replaces this.
    /// </summary>
    public class TestSceneDirector : MonoBehaviour
    {
        [SerializeField] private Hero[] _heroes;
        [SerializeField] private DummyEnemy[] _enemies;

        private void Reset()
        {
            _heroes = FindObjectsByType<Hero>(FindObjectsSortMode.None);
            _enemies = FindObjectsByType<DummyEnemy>(FindObjectsSortMode.None);
        }

        private void FixedUpdate()
        {
            if (_heroes == null) return;

            foreach (var hero in _heroes)
            {
                if (hero == null) continue;
                hero.NearestEnemyPosition = FindNearestEnemyPosition(hero.SelfPosition);
            }
        }

        private Vector2? FindNearestEnemyPosition(Vector2 from)
        {
            Vector2? best = null;
            float bestSqr = float.MaxValue;
            if (_enemies == null) return best;

            foreach (var e in _enemies)
            {
                if (e == null || !e.IsAlive) continue;
                Vector2 p = e.transform.position;
                float d = (p - from).sqrMagnitude;
                if (d < bestSqr)
                {
                    bestSqr = d;
                    best = p;
                }
            }
            return best;
        }
    }
}
