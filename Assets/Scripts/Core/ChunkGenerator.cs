using System.Collections.Generic;

namespace Regauntless.Core
{
    /// <summary>
    /// Programmatic chunk construction (see .kiro/steering/levels.md). Blooms a floor footprint
    /// and encloses it with a border wall ring. Pure data — produces a list of (HexCoord, kind)
    /// cells with no engine dependency, so it's headless-testable and can be serialized to a
    /// ChunkData asset by a thin writer (editor or hand-authored YAML).
    ///
    /// NOTE: the full two-grid wall construction (copy floor → half-cell vertical move → half-bloom
    /// → 30° rotate) is the *rendered* wall lattice. For the GAMEPLAY grid we resolve walls onto
    /// the hex grid as a border ring (the enclosure). Interior/maze walls come later from the
    /// crystal generator. This first pass = floor bloom + enclosing border.
    /// </summary>
    public static class ChunkGenerator
    {
        public enum Kind { Floor, Wall }

        public struct GenCell
        {
            public HexCoord Coord;
            public Kind Kind;
            public GenCell(HexCoord c, Kind k) { Coord = c; Kind = k; }
        }

        /// <summary>
        /// A filled-hexagon floor of the given radius, enclosed by a one-hex border wall ring.
        /// </summary>
        public static List<GenCell> BloomRoom(HexCoord center, int radius)
        {
            var cells = new List<GenCell>();
            var floor = new HashSet<HexCoord>(center.Range(radius));

            // floor
            foreach (var h in floor)
                cells.Add(new GenCell(h, Kind.Floor));

            // border wall ring = the ring one step beyond the floor (radius+1), i.e. every hex
            // adjacent to the floor that isn't itself floor.
            var walls = new HashSet<HexCoord>();
            foreach (var h in floor)
                foreach (var n in h.Neighbors())
                    if (!floor.Contains(n)) walls.Add(n);

            foreach (var w in walls)
                cells.Add(new GenCell(w, Kind.Wall));

            return cells;
        }

        /// <summary>
        /// A floor grown from a seed by a half-bloom on one side (an asymmetric room), enclosed by
        /// a border wall ring. Gives shape variety beyond the symmetric hexagon.
        /// </summary>
        public static List<GenCell> BloomHalf(HexCoord center, int radius, int facing)
        {
            var floor = new HashSet<HexCoord>(center.Range(radius));
            // extend the floor on 3 adjacent sides (a lobe)
            foreach (var h in Bloom.HalfBloom(floor, facing))
                floor.Add(h);

            var cells = new List<GenCell>();
            foreach (var h in floor)
                cells.Add(new GenCell(h, Kind.Floor));

            var walls = new HashSet<HexCoord>();
            foreach (var h in floor)
                foreach (var n in h.Neighbors())
                    if (!floor.Contains(n)) walls.Add(n);
            foreach (var w in walls)
                cells.Add(new GenCell(w, Kind.Wall));

            return cells;
        }
    }
}
