using System;

namespace Regauntless.Levels
{
    /// <summary>
    /// A single chunk cell: a MECHANICAL <see cref="CellType"/> plus a COSMETIC
    /// <see cref="Variant"/> index. Two axes, deliberately separate:
    ///   - Type   → gameplay meaning. Logic branches on this.
    ///   - Variant→ which texture/visual flavor. ONLY the renderer reads this; gameplay never does.
    ///
    /// This separation guarantees cosmetic variants can never alter gameplay (determinism /
    /// legibility preserved — see presentation.md, combat.md). Variant 0 = default.
    /// Variants need no art to exist: they are label-first (e.g. "Floor 2 - stub"),
    /// sprite-bound later (see CellTypeDefinition).
    /// </summary>
    [Serializable]
    public struct Cell : IEquatable<Cell>
    {
        public CellType Type;
        public int Variant;

        public Cell(CellType type, int variant = 0)
        {
            Type = type;
            Variant = variant;
        }

        public static Cell None => new Cell(CellType.None, 0);

        public bool IsWalkable => Type == CellType.Floor
                                  || Type == CellType.Port
                                  || Type == CellType.HeroSpawn
                                  || Type == CellType.Food
                                  || Type == CellType.Exit;

        /// <summary>Solid for movement/line-of-fire purposes (walls; cracks are a geometry layer).</summary>
        public bool IsSolid => Type == CellType.Wall;

        public bool IsPartOfChunk => Type != CellType.None;

        public bool Equals(Cell other) => Type == other.Type && Variant == other.Variant;
        public override bool Equals(object obj) => obj is Cell c && Equals(c);
        public override int GetHashCode() => ((int)Type * 397) ^ Variant;
        public override string ToString() => $"{Type}#{Variant}";
    }
}
