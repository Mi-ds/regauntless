using UnityEngine;
using Regauntless.Controllers;
using Regauntless.Core;

namespace Regauntless.Heroes
{
    /// <summary>
    /// A hero: holds an <see cref="IController"/> and consumes the <see cref="HeroIntent"/> it
    /// produces. The hero NEVER asks who produced the intent (Prime Directive, see
    /// .kiro/steering/product.md). AI, local player, or remote controller — all the same here.
    ///
    /// First-milestone scope: continuous movement at a derived-ish speed, body facing snapped
    /// to 6 hex directions, aim snapped to 12, and a fire flag. The hex-step / fire-hold
    /// brake-pivot refinement (architecture.md) comes later — this is the "walk + shoot a dummy"
    /// vertical slice.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Hero : MonoBehaviour, IControllerContext
    {
        [Header("Wiring")]
        [Tooltip("Optional: explicit controller. If left empty, the Hero auto-finds an " +
                 "IController component on this same GameObject (e.g. an AIController).")]
        [SerializeField] private MonoBehaviour _controllerBehaviour; // optional; must be IController
        [Tooltip("Transform used as the muzzle / shot origin. Defaults to this transform.")]
        [SerializeField] private Transform _muzzle;

        [Header("Stats (placeholder — becomes derived HeroStats, SRD-sourced)")]
        [Tooltip("World units per second. Placeholder for SRD speed -> hexes/sec via time-scale.")]
        [SerializeField] private float _moveSpeed = 3f;
        [Tooltip("Seconds between shots. Placeholder for attacks-per-round -> cadence.")]
        [SerializeField] private float _fireCooldown = 0.5f;

        private IController _controller;
        private Rigidbody2D _rb;
        private float _fireTimer;

        /// <summary>The hero's persistent world-space facing (snapped to 6). Camera-independent.</summary>
        public Vector2 Facing { get; private set; } = Vector2.right;

        /// <summary>The hero's last aim direction (snapped to 12), for shooting/visuals.</summary>
        public Vector2 AimFacing { get; private set; } = Vector2.right;

        // --- IControllerContext (what a controller is allowed to see) ---
        public Vector2 SelfPosition => _rb != null ? _rb.position : (Vector2)transform.position;
        public Vector2? NearestEnemyPosition { get; set; } // set by the scene/sim for the slice

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;              // top-down, no gravity
            _rb.freezeRotation = true;
            if (_muzzle == null) _muzzle = transform;

            // Prefer an explicitly-assigned controller; otherwise auto-find one on this
            // GameObject (e.g. an AIController sitting alongside the Hero). This avoids the
            // fragile inspector-slot wiring for the common case.
            _controller = _controllerBehaviour as IController;
            if (_controller == null)
                _controller = GetComponent<IController>();
            if (_controller == null)
                Debug.LogWarning($"{name}: no IController found; hero will idle. " +
                                 "Add an AIController (or assign one) to this GameObject.", this);
        }

        /// <summary>Assign/replace the controller at runtime — this IS the drop-in-coin swap.</summary>
        public void SetController(IController controller) => _controller = controller;

        private void FixedUpdate()
        {
            var intent = _controller != null ? _controller.GetIntent(this) : HeroIntent.None;

            // Movement: analog intent -> snapped-6 heading, move continuously along it.
            if (intent.HasMove)
            {
                Vector2 heading = HexDirections.SnapMovement(intent.Move);
                Facing = heading;
                _rb.linearVelocity = heading * _moveSpeed;
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
            }

            // Aim: analog intent -> snapped-12 (decoupled from movement).
            if (intent.HasAim)
                AimFacing = HexDirections.SnapAim(intent.Aim);

            // Fire: rate-limited (placeholder for attacks-per-round cadence).
            _fireTimer -= Time.fixedDeltaTime;
            if (intent.Attack && _fireTimer <= 0f)
            {
                Fire();
                _fireTimer = _fireCooldown;
            }
        }

        private void Fire()
        {
            // Milestone stub: log + debug ray. Projectiles (lanes/width, combat-space.md) next.
            Debug.DrawRay(_muzzle.position, (Vector3)(AimFacing * 1.5f), Color.cyan, 0.2f);
            Debug.Log($"{name} FIRE -> {AimFacing}", this);
        }
    }
}
