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

        /// <summary>
        /// HALF BLOOM: given a footprint (set of hexes), return the hexes one step OUT from it on
        /// only 3 ADJACENT directions — a 180° half-ring/arc on one side (see .kiro/steering/levels.md).
        /// Walls are built by half-blooming the floor footprint (then offsetting + rotating 30°).
        ///
        /// <paramref name="facing"/> is the first of the 3 adjacent directions; the arc covers
        /// {facing, facing+1, facing+2} (mod 6). Result excludes hexes already in the footprint
        /// (only genuinely-new, outward hexes), de-duplicated.
        /// </summary>
        public static List<HexCoord> HalfBloom(IEnumerable<HexCoord> footprint, int facing)
        {
            var set = new HashSet<HexCoord>(footprint);
            var result = new List<HexCoord>();
            var seen = new HashSet<HexCoord>();

            foreach (var h in set)
            {
                for (int k = 0; k < 3; k++)                 // 3 adjacent directions = 180° arc
                {
                    int dir = (((facing + k) % 6) + 6) % 6;
                    HexCoord n = h.Neighbor(dir);
                    if (set.Contains(n)) continue;          // not a new outward hex
                    if (seen.Add(n)) result.Add(n);         // dedupe
                }
            }
            return result;
        }

        /// <summary>Convenience: half-bloom a single hex (its footprint = just itself).</summary>
        public static List<HexCoord> HalfBloom(HexCoord center, int facing)
            => HalfBloom(new[] { center }, facing);
    }
}
