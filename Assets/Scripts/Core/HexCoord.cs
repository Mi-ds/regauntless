using System;
using System.Collections.Generic;
using UnityEngine;

namespace Regauntless.Core
{
    /// <summary>
    /// Pure, engine-light hex coordinate (axial q,r with cube conversions). The spatial bedrock
    /// everything hex-native sits on (see .kiro/steering/levels.md, vision.md, architecture.md).
    ///
    /// Conventions (Red Blob Games standard):
    ///   - Axial storage: (q, r). Cube derived: x=q, z=r, y=-x-z, with x+y+z==0.
    ///   - 6 neighbors, indexed 0..5 (directions are a fixed, documented order).
    ///   - Distance = cube distance = (|dx|+|dy|+|dz|)/2.
    ///
    /// This type is immutable, value-equatable, and has NO Unity runtime dependency beyond the
    /// Vector math types — fully unit-testable in EditMode with no scene. World<->hex layout
    /// (pixel conversions, orientation, size) lives in HexLayout, kept separate on purpose.
    /// </summary>
    [Serializable]
    public readonly struct HexCoord : IEquatable<HexCoord>
    {
        public readonly int Q;
        public readonly int R;

        public HexCoord(int q, int r) { Q = q; R = r; }

        // Cube coordinates (derived). x + y + z == 0 always holds.
        public int X => Q;
        public int Z => R;
        public int Y => -Q - R;

        public static readonly HexCoord Zero = new HexCoord(0, 0);

        /// <summary>
        /// The 6 axial direction vectors, in a fixed order (index 0..5). Order is counted
        /// counter-clockwise starting from "east-ish" (+q). Rotation by one step = index+1.
        /// </summary>
        public static readonly HexCoord[] Directions =
        {
            new HexCoord( 1,  0),  // 0
            new HexCoord( 1, -1),  // 1
            new HexCoord( 0, -1),  // 2
            new HexCoord(-1,  0),  // 3
            new HexCoord(-1,  1),  // 4
            new HexCoord( 0,  1),  // 5
        };

        public static HexCoord Direction(int index)
        {
            index = ((index % 6) + 6) % 6; // wrap
            return Directions[index];
        }

        public HexCoord Neighbor(int directionIndex) => this + Direction(directionIndex);

        /// <summary>All 6 neighbors of this hex (a new list each call).</summary>
        public List<HexCoord> Neighbors()
        {
            var list = new List<HexCoord>(6);
            for (int i = 0; i < 6; i++) list.Add(Neighbor(i));
            return list;
        }

        // --- arithmetic ---
        public static HexCoord operator +(HexCoord a, HexCoord b) => new HexCoord(a.Q + b.Q, a.R + b.R);
        public static HexCoord operator -(HexCoord a, HexCoord b) => new HexCoord(a.Q - b.Q, a.R - b.R);
        public static HexCoord operator *(HexCoord a, int k) => new HexCoord(a.Q * k, a.R * k);

        /// <summary>Cube distance between two hexes (number of steps along the grid).</summary>
        public static int Distance(HexCoord a, HexCoord b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            int dz = a.Z - b.Z;
            return (Mathf.Abs(dx) + Mathf.Abs(dy) + Mathf.Abs(dz)) / 2;
        }

        public int DistanceTo(HexCoord other) => Distance(this, other);

        /// <summary>
        /// Rotate this hex around the origin by <paramref name="steps"/> × 60° (counter-clockwise
        /// for positive steps). Used for the camera snap-rotation's grid math and symmetry ops.
        /// </summary>
        public HexCoord Rotate60(int steps)
        {
            // Cube rotation by 60°: (x,y,z) -> (-z,-x,-y) per CCW step.
            int x = X, y = Y, z = Z;
            steps = ((steps % 6) + 6) % 6;
            for (int i = 0; i < steps; i++)
            {
                int nx = -z, ny = -x, nz = -y;
                x = nx; y = ny; z = nz;
            }
            // back to axial (q=x, r=z)
            return new HexCoord(x, z);
        }

        /// <summary>
        /// All hexes exactly <paramref name="radius"/> steps from this hex (the ring). radius 0
        /// returns just this hex; radius 1 returns the 6 neighbors.
        /// </summary>
        public List<HexCoord> Ring(int radius)
        {
            var results = new List<HexCoord>();
            if (radius < 0) return results;
            if (radius == 0) { results.Add(this); return results; }

            // start at the hex 'radius' steps in direction 4, then walk around the 6 sides.
            HexCoord hex = this + Direction(4) * radius;
            for (int side = 0; side < 6; side++)
            {
                for (int step = 0; step < radius; step++)
                {
                    results.Add(hex);
                    hex = hex.Neighbor(side);
                }
            }
            return results;
        }

        /// <summary>
        /// All hexes within <paramref name="radius"/> steps (inclusive) — a filled hexagon.
        /// Count = 1 + 3*radius*(radius+1).
        /// </summary>
        public List<HexCoord> Range(int radius)
        {
            var results = new List<HexCoord>();
            for (int dq = -radius; dq <= radius; dq++)
            {
                int rLow = Mathf.Max(-radius, -dq - radius);
                int rHigh = Mathf.Min(radius, -dq + radius);
                for (int dr = rLow; dr <= rHigh; dr++)
                    results.Add(new HexCoord(Q + dq, R + dr));
            }
            return results;
        }

        /// <summary>
        /// The straight line of hexes from this to <paramref name="target"/> (inclusive of both
        /// ends), using cube linear interpolation + rounding. Length = distance + 1.
        /// </summary>
        public List<HexCoord> LineTo(HexCoord target)
        {
            int n = DistanceTo(target);
            var results = new List<HexCoord>(n + 1);
            if (n == 0) { results.Add(this); return results; }

            for (int i = 0; i <= n; i++)
            {
                float t = (float)i / n;
                float x = Mathf.Lerp(X, target.X, t);
                float y = Mathf.Lerp(Y, target.Y, t);
                float z = Mathf.Lerp(Z, target.Z, t);
                results.Add(CubeRound(x, y, z));
            }
            return results;
        }

        /// <summary>Round fractional cube coordinates to the nearest valid hex.</summary>
        public static HexCoord CubeRound(float x, float y, float z)
        {
            int rx = Mathf.RoundToInt(x);
            int ry = Mathf.RoundToInt(y);
            int rz = Mathf.RoundToInt(z);

            float dx = Mathf.Abs(rx - x);
            float dy = Mathf.Abs(ry - y);
            float dz = Mathf.Abs(rz - z);

            if (dx > dy && dx > dz) rx = -ry - rz;
            else if (dy > dz)       ry = -rx - rz;
            else                    rz = -rx - ry;

            return new HexCoord(rx, rz); // axial (q=x, r=z)
        }

        // --- equality / hashing ---
        public bool Equals(HexCoord other) => Q == other.Q && R == other.R;
        public override bool Equals(object obj) => obj is HexCoord h && Equals(h);
        public override int GetHashCode() => unchecked((Q * 73856093) ^ (R * 19349663));
        public static bool operator ==(HexCoord a, HexCoord b) => a.Equals(b);
        public static bool operator !=(HexCoord a, HexCoord b) => !a.Equals(b);

        public override string ToString() => $"Hex({Q},{R})";
    }
}
