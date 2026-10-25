using System.Collections.Generic;
using NUnit.Framework;
using Regauntless.Core;

namespace Regauntless.Tests
{
    /// <summary>
    /// Pure-math tests for the hex coordinate bedrock and the first bloom. No engine runtime.
    /// These are the "same seed / same shape, provably correct" guarantees the generator builds on.
    /// </summary>
    public class HexCoordTests
    {
        [Test]
        public void Cube_InvariantHolds()
        {
            for (int q = -5; q <= 5; q++)
            for (int r = -5; r <= 5; r++)
            {
                var h = new HexCoord(q, r);
                Assert.AreEqual(0, h.X + h.Y + h.Z, $"cube invariant broken at {h}");
            }
        }

        [Test]
        public void Neighbors_AreSixDistinctHexesAtDistanceOne()
        {
            var h = new HexCoord(2, -1);
            var ns = h.Neighbors();
            Assert.AreEqual(6, ns.Count);
            CollectionAssert.AllItemsAreUnique(ns);
            foreach (var n in ns)
                Assert.AreEqual(1, h.DistanceTo(n), $"{n} should be distance 1 from {h}");
        }

        [Test]
        public void Distance_IsSymmetricAndZeroToSelf()
        {
            var a = new HexCoord(0, 0);
            var b = new HexCoord(3, -2);
            Assert.AreEqual(HexCoord.Distance(a, b), HexCoord.Distance(b, a));
            Assert.AreEqual(0, a.DistanceTo(a));
            // (3,-2): cube (x=3, y=-1, z=-2) -> distance = (3+1+2)/2 = 3
            Assert.AreEqual(3, a.DistanceTo(b));
        }

        [Test]
        public void Ring_ZeroIsSelf()
        {
            var h = new HexCoord(1, 1);
            var ring = h.Ring(0);
            Assert.AreEqual(1, ring.Count);
            Assert.AreEqual(h, ring[0]);
        }

        [Test]
        public void Ring_RadiusN_HasSixNHexesAllAtDistanceN()
        {
            var center = new HexCoord(0, 0);
            for (int radius = 1; radius <= 4; radius++)
            {
                var ring = center.Ring(radius);
                Assert.AreEqual(6 * radius, ring.Count, $"ring {radius} count");
                CollectionAssert.AllItemsAreUnique(ring);
                foreach (var h in ring)
                    Assert.AreEqual(radius, center.DistanceTo(h), $"{h} not at distance {radius}");
            }
        }

        [Test]
        public void Range_FilledHexCount_MatchesFormula()
        {
            var center = new HexCoord(0, 0);
            for (int radius = 0; radius <= 4; radius++)
            {
                var range = center.Range(radius);
                int expected = 1 + 3 * radius * (radius + 1);
                Assert.AreEqual(expected, range.Count, $"range {radius} count");
                CollectionAssert.AllItemsAreUnique(range);
            }
        }

        [Test]
        public void Rotate60_SixStepsReturnsToOrigin()
        {
            var h = new HexCoord(3, -2);
            Assert.AreEqual(h, h.Rotate60(6), "6 x 60 = 360 should be identity");
            Assert.AreEqual(h, h.Rotate60(0));
            Assert.AreEqual(h, h.Rotate60(12));
        }

        [Test]
        public void Rotate60_PreservesDistanceFromOrigin()
        {
            var h = new HexCoord(3, -2);
            int d = HexCoord.Zero.DistanceTo(h);
            for (int s = 0; s < 6; s++)
                Assert.AreEqual(d, HexCoord.Zero.DistanceTo(h.Rotate60(s)), $"rotation {s} changed distance");
        }

        [Test]
        public void Rotate60_NeighborStepsThroughAllSixDirections()
        {
            // Rotating direction 0 by k steps should cycle through all 6 direction vectors.
            var seen = new HashSet<HexCoord>();
            var d0 = HexCoord.Direction(0);
            for (int s = 0; s < 6; s++) seen.Add(d0.Rotate60(s));
            Assert.AreEqual(6, seen.Count, "rotating a direction should visit all 6 directions");
            foreach (var dir in HexCoord.Directions)
                Assert.IsTrue(seen.Contains(dir), $"rotation set missing direction {dir}");
        }

        [Test]
        public void LineTo_LengthIsDistancePlusOne_AndEndpointsMatch()
        {
            var a = new HexCoord(0, 0);
            var b = new HexCoord(4, -2);
            var line = a.LineTo(b);
            Assert.AreEqual(a.DistanceTo(b) + 1, line.Count);
            Assert.AreEqual(a, line[0]);
            Assert.AreEqual(b, line[line.Count - 1]);
            // each consecutive pair is adjacent
            for (int i = 1; i < line.Count; i++)
                Assert.AreEqual(1, line[i - 1].DistanceTo(line[i]), "line steps must be adjacent");
        }

        [Test]
        public void CubeRound_ReturnsValidHex()
        {
            var h = HexCoord.CubeRound(1.4f, -0.6f, -0.8f);
            Assert.AreEqual(0, h.X + h.Y + h.Z, "rounded hex must satisfy cube invariant");
        }

        [Test]
        public void Equality_And_HashSet_Behaviour()
        {
            var a = new HexCoord(2, -3);
            var b = new HexCoord(2, -3);
            var c = new HexCoord(2, -2);
            Assert.AreEqual(a, b);
            Assert.AreNotEqual(a, c);
            Assert.IsTrue(a == b);
            Assert.IsTrue(a != c);
            var set = new HashSet<HexCoord> { a, b, c };
            Assert.AreEqual(2, set.Count, "a and b are the same key");
        }

        // --- Bloom: the party-spawn rosette (chunk #0) ---

        [Test]
        public void PartySpawn_IsSevenHexes_CenterPlusSixRing()
        {
            var center = new HexCoord(5, -2);
            var rosette = Bloom.PartySpawn(center);
            Assert.AreEqual(7, rosette.Count, "rosette = center + 6");
            Assert.AreEqual(center, rosette[0], "index 0 is the center/leader hub");
            CollectionAssert.AllItemsAreUnique(rosette);
        }

        [Test]
        public void PartySpawn_SixSlotsAreDistanceOneFromCenter()
        {
            var center = HexCoord.Zero;
            var slots = Bloom.PartySlots(center);
            Assert.AreEqual(6, slots.Count, "6 hero slots, one per roster member");
            foreach (var s in slots)
                Assert.AreEqual(1, center.DistanceTo(s));
        }

        [Test]
        public void Room_RadiusTwo_Is19Hexes()
        {
            var room = Bloom.Room(HexCoord.Zero, 2);
            Assert.AreEqual(1 + 3 * 2 * 3, room.Count); // 19
        }
    }
}
