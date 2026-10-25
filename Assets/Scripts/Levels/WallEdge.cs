using System;
using Regauntless.Core;

namespace Regauntless.Levels
{
    /// <summary>
    /// A wall is an EDGE between two floor hexes, addressed as (floorHex, direction) — see
    /// .kiro/steering/levels.md. Because the edge is shared by two hexes, the same wall can be
    /// named from either side; <see cref="Canonical"/> picks ONE owner so each wall stores once
    /// (~3 owned edges per hex rather than 6). Walls are sparse "present/absent" bools keyed by
    /// this; the offset/30° lattice is just how they RENDER.
    /// </summary>
    [Serializable]
    public struct WallEdge : IEquatable<WallEdge>
    {
        public int Q;
        public int R;
        public int Dir;   // 0..5, direction from (Q,R) across the shared edge

        public WallEdge(HexCoord hex, int dir)
        {
            Q = hex.Q; R = hex.R; Dir = ((dir % 6) + 6) % 6;
        }

        public HexCoord Hex => new HexCoord(Q, R);
        public HexCoord Neighbor => Hex.Neighbor(Dir);

        /// <summary>
        /// Canonical form: the same physical wall from (A, dirToB) and (B, dirToA) collapses to
        /// ONE key. Owner = the hex with the lexicographically-smaller (q,r); Dir points from it.
        /// </summary>
        public WallEdge Canonical()
        {
            HexCoord a = Hex;
            HexCoord b = Neighbor;
            bool aOwns = a.Q < b.Q || (a.Q == b.Q && a.R <= b.R);
            if (aOwns) return new WallEdge(a, Dir);

            // find the direction from b back to a
            for (int d = 0; d < 6; d++)
                if (b.Neighbor(d).Equals(a)) return new WallEdge(b, d);
            return this; // shouldn't happen for adjacent hexes
        }

        public bool Equals(WallEdge other) => Q == other.Q && R == other.R && Dir == other.Dir;
        public override bool Equals(object obj) => obj is WallEdge w && Equals(w);
        public override int GetHashCode() => unchecked((Q * 73856093) ^ (R * 19349663) ^ (Dir * 83492791));
        public override string ToString() => $"Wall({Q},{R}|d{Dir})";
    }
}
