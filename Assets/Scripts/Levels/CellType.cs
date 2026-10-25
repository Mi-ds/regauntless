namespace Regauntless.Levels
{
    /// <summary>
    /// The MECHANICAL type of a chunk cell — what the game logic cares about (walkable? spawner?
    /// an opening the maze can attach to?). This is the legend/palette (see .kiro/steering/levels.md,
    /// the Space Run Galaxy-style color-coded hex paint model).
    ///
    /// Presentation VARIANTS (which floor texture, etc.) are a SEPARATE cosmetic axis on
    /// <see cref="Cell"/> — gameplay logic must NEVER branch on variant, only on CellType.
    /// </summary>
    public enum CellType
    {
        /// <summary>Not part of this chunk (empty / outside the painted footprint).</summary>
        None = 0,

        /// <summary>Walkable floor. Painting floor defines the chunk's footprint/shape.</summary>
        Floor = 1,

        /// <summary>Solid wall. Cracks EMERGE where walls meet only at a vertex (see levels.md).</summary>
        Wall = 2,

        /// <summary>Enemy spawner ("generator" in Gauntlet terms).</summary>
        Generator = 3,

        /// <summary>A connection port — where the crystal maze may attach a corridor.</summary>
        Port = 4,

        /// <summary>A hero start slot (the party-spawn rosette uses these).</summary>
        HeroSpawn = 5,

        /// <summary>A food/health pickup (refills the shared party pool — survival.md).</summary>
        Food = 6,

        /// <summary>The level exit. Gated to the late game by the generator (levels.md).</summary>
        Exit = 7,
    }
}
