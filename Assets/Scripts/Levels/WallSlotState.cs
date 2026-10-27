namespace Regauntless.Levels
{
    /// <summary>
    /// State of one wall-slot (the slots are derived from the floor via Bloom.WallSlots; the
    /// chunk stores one of these per slot — see ChunkData.WallStates / levels.md).
    ///
    /// Doors and windows are NOT a separate openings system — they're just wall-slot states:
    ///   None = DOOR / opening / gap  → passes MOVEMENT and LoS (the floor continues through).
    ///                                   The maze stitches chunks by matching up None slots.
    ///   Half = WINDOW / half-wall    → blocks MOVEMENT, PASSES LoS (see/shoot over it).
    ///                                   (Interacts with the LoS combat model — combat-model.md.)
    ///   Full = WALL                  → blocks MOVEMENT and LoS.
    /// </summary>
    public enum WallSlotState
    {
        None = 0, // door / gap
        Half = 1, // window / half-wall
        Full = 2  // solid wall
    }
}
