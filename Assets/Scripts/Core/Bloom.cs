using System.Collections.Generic;

namespace Regauntless.Core
{
    /// <summary>
    /// Bloom rules: given a center hex, produce the set of hexes a node occupies. The first and
    /// simplest is the party-spawn rosette — the shape derived directly from the 6-hero roster
    /// (see .kiro/steering/levels.md "chunk #0", vision.md).
    ///
    /// Pure/static, no engine — the generator (crystal maze) will call these to carve geography.
    /// </summary>
    public static class Bloom
    {
        /// <summary>
        /// The party-spawn rosette: the center hex plus its 6 neighbors = 7 hexes.
        /// The 6 ring hexes are the hero slots (one per hero); the center is the leader/marshal
        /// hub (ties to formations/coordination, coordination.md). Deterministic order:
        /// index 0 = center, indices 1..6 = the 6 ring hexes in Direction order.
        /// </summary>
        public static List<HexCoord> PartySpawn(HexCoord center)
        {
            var hexes = new List<HexCoord>(7) { center };
            hexes.AddRange(center.Ring(1)); // exactly 6, all distance-1
            return hexes;
        }

        /// <summary>The 6 hero-slot hexes of a party rosette (the ring only, no center).</summary>
        public static List<HexCoord> PartySlots(HexCoord center) => center.Ring(1);

        /// <summary>A filled hexagonal room of the given radius (center + all within radius).</summary>
        public static List<HexCoord> Room(HexCoord center, int radius) => center.Range(radius);
    }
}
