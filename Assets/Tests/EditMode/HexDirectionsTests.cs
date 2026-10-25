using NUnit.Framework;
using UnityEngine;
using Regauntless.Core;

namespace Regauntless.Tests
{
    /// <summary>
    /// Pure-math tests for hex direction snapping (6-way movement / 12-way aim).
    /// No engine runtime needed — this is the testable heart of the control scheme.
    /// </summary>
    public class HexDirectionsTests
    {
        const float Eps = 1e-3f;

        [Test]
        public void Snap_ZeroVector_ReturnsZero()
        {
            Assert.AreEqual(Vector2.zero, HexDirections.Snap(Vector2.zero, 6));
            Assert.AreEqual(Vector2.zero, HexDirections.SnapMovement(Vector2.zero));
            Assert.AreEqual(Vector2.zero, HexDirections.SnapAim(Vector2.zero));
        }

        [Test]
        public void Movement_HasSixSectors()
        {
            Assert.AreEqual(6, HexDirections.MovementSectors);
        }

        [Test]
        public void Aim_HasTwelveSectors()
        {
            Assert.AreEqual(12, HexDirections.AimSectors);
        }

        [Test]
        public void SnapMovement_ExactEast_StaysEast()
        {
            var snapped = HexDirections.SnapMovement(Vector2.right);
            Assert.AreEqual(1f, snapped.x, Eps);
            Assert.AreEqual(0f, snapped.y, Eps);
        }

        [Test]
        public void SnapMovement_SnapsToNearestOfSixSectors()
        {
            // 6 sectors = 60° each, zero at +X: headings at 0,60,120,180,240,300.
            // A dir at 50° should snap to the 60° sector.
            float rad = 50f * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            int idx = HexDirections.SnapMovementIndex(dir);
            Assert.AreEqual(1, idx, "50 degrees should snap to sector 1 (60 degrees)");
        }

        [Test]
        public void SnapMovement_TenDegrees_SnapsToEastSector()
        {
            float rad = 10f * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            int idx = HexDirections.SnapMovementIndex(dir);
            Assert.AreEqual(0, idx, "10 degrees should snap to sector 0 (0 degrees)");
        }

        [Test]
        public void SnapAim_FinerThanMovement_ThirtyDegreeStep()
        {
            // 12 sectors = 30° each. A dir at 25° should snap to sector 1 (30°),
            // where movement (60° step) would have snapped 25° to sector 0 (0°).
            float rad = 25f * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Assert.AreEqual(1, HexDirections.SnapAimIndex(dir), "aim: 25 -> sector 1 (30)");
            Assert.AreEqual(0, HexDirections.SnapMovementIndex(dir), "move: 25 -> sector 0 (0)");
        }

        [Test]
        public void SnapIndex_WrapsNegativeAngles()
        {
            // Pointing just below +X (e.g. -10°) should wrap to sector 0, not a negative index.
            float rad = -10f * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            int idx = HexDirections.SnapMovementIndex(dir);
            Assert.AreEqual(0, idx);
        }

        [Test]
        public void Snap_ReturnsUnitVectors()
        {
            for (float a = 0; a < 360f; a += 17f)
            {
                float rad = a * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                var sMove = HexDirections.SnapMovement(dir);
                var sAim = HexDirections.SnapAim(dir);
                Assert.AreEqual(1f, sMove.magnitude, Eps, $"move snap at {a} not unit length");
                Assert.AreEqual(1f, sAim.magnitude, Eps, $"aim snap at {a} not unit length");
            }
        }

        [Test]
        public void SnapAim_AllTwelveSectorsReachable()
        {
            var seen = new System.Collections.Generic.HashSet<int>();
            for (int i = 0; i < 12; i++)
            {
                float a = i * 30f;
                float rad = a * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                seen.Add(HexDirections.SnapAimIndex(dir));
            }
            Assert.AreEqual(12, seen.Count, "all 12 aim sectors should be distinctly reachable");
        }

        [Test]
        public void HeroIntent_None_IsInert()
        {
            var none = Controllers.HeroIntent.None;
            Assert.IsFalse(none.HasMove);
            Assert.IsFalse(none.HasAim);
            Assert.IsFalse(none.Attack);
        }
    }
}
