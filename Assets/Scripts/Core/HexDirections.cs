using UnityEngine;

namespace Regauntless.Core
{
    /// <summary>
    /// Pure, engine-free (math-only) direction snapping for the hex-native control scheme
    /// (see .kiro/steering/vision.md). Input is analog 360°; the SPRITE/presentation snaps to:
    ///   - 6 directions for movement  (the 6 hex edges,   60° apart)
    ///   - 12 directions for shooting  (6 edges + 6 vertices, 30° apart)
    ///
    /// This class only does the angle quantization. It is deterministic and has no Unity
    /// runtime dependencies beyond Vector2/Mathf math types, so it is fully unit-testable in
    /// EditMode with no scene.
    ///
    /// Orientation note: pointy-vs-flat is a dead question (camera default only), so we define
    /// the snap grid relative to a configurable "zero" offset. Default zero points +X (east).
    /// </summary>
    public static class HexDirections
    {
        public const int MovementSectors = 6;   // hex edges
        public const int AimSectors = 12;        // edges + vertices

        /// <summary>Snap an analog direction to one of 6 movement headings (unit vector).</summary>
        public static Vector2 SnapMovement(Vector2 dir, float zeroOffsetDegrees = 0f)
            => Snap(dir, MovementSectors, zeroOffsetDegrees);

        /// <summary>Snap an analog direction to one of 12 aim headings (unit vector).</summary>
        public static Vector2 SnapAim(Vector2 dir, float zeroOffsetDegrees = 0f)
            => Snap(dir, AimSectors, zeroOffsetDegrees);

        /// <summary>Snap to the index of one of 6 movement sectors (0..5).</summary>
        public static int SnapMovementIndex(Vector2 dir, float zeroOffsetDegrees = 0f)
            => SnapIndex(dir, MovementSectors, zeroOffsetDegrees);

        /// <summary>Snap to the index of one of 12 aim sectors (0..11).</summary>
        public static int SnapAimIndex(Vector2 dir, float zeroOffsetDegrees = 0f)
            => SnapIndex(dir, AimSectors, zeroOffsetDegrees);

        /// <summary>
        /// Quantize a direction to the nearest of <paramref name="sectors"/> evenly-spaced
        /// headings and return it as a unit vector. A zero/near-zero input returns Vector2.zero
        /// (no direction to snap).
        /// </summary>
        public static Vector2 Snap(Vector2 dir, int sectors, float zeroOffsetDegrees = 0f)
        {
            if (dir.sqrMagnitude < 1e-8f) return Vector2.zero;
            int index = SnapIndex(dir, sectors, zeroOffsetDegrees);
            float step = 360f / sectors;
            float angle = zeroOffsetDegrees + index * step;
            float rad = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        /// <summary>
        /// Quantize a direction to the nearest sector index in [0, sectors). Ties round toward
        /// the higher index deterministically (via Mathf.Round half-away-from-zero + wrap).
        /// </summary>
        public static int SnapIndex(Vector2 dir, int sectors, float zeroOffsetDegrees = 0f)
        {
            float step = 360f / sectors;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; // (-180, 180]
            float rel = angle - zeroOffsetDegrees;
            int index = Mathf.RoundToInt(rel / step);
            // wrap into [0, sectors)
            index %= sectors;
            if (index < 0) index += sectors;
            return index;
        }
    }
}
