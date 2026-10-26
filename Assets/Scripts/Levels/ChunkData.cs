using System;
using System.Collections.Generic;
using UnityEngine;
using Regauntless.Core;

namespace Regauntless.Levels
{
    /// <summary>
    /// A hand-authored set-piece CHUNK (guard room, arena, trap, etc. — see .kiro/steering/levels.md),
    /// stored PRECISELY: an explicit list of (HexCoord, Cell) entries. No ASCII, no parser — the
    /// data IS the chunk. Authored by the Space-Run-Galaxy-style hex paint editor; the painted
    /// floor cells define the chunk's footprint (unpainted = None/outside).
    ///
    /// Serialization note: Unity can't serialize a Dictionary directly, so cells are stored as a
    /// flat list of entries and indexed into a dictionary at load. This stays precise and
    /// parser-free (deserialize, don't interpret).
    /// </summary>
    [CreateAssetMenu(fileName = "Chunk", menuName = "Regauntless/Chunk")]
    public class ChunkData : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public int Q;
            public int R;
            public CellType Type;
            public int Variant;

            public HexCoord Coord => new HexCoord(Q, R);
            public Cell Cell => new Cell(Type, Variant);
        }

        [Tooltip("Which chunk-pool this belongs to (guard-room, arena, trap, spawn, exit, ...).")]
        public string ChunkType = "generic";

        [Tooltip("Author-facing name.")]
        public string ChunkName = "Unnamed Chunk";

        [Tooltip("The painted cells. Floor cells define the footprint; unpainted hexes are outside.")]
        public List<Entry> Cells = new List<Entry>();

        // --- WALL LAYER = a BITSTREAM, not coordinates. --------------------------------------
        // The wall-slot POSITIONS are re-derived from the floor every load (Bloom.WallSlots:
        // floor copy + half-bloom = "the floor footprint as a bitstream with slightly more
        // positions"). We store ONLY the bits (1 = wall present at that slot, 0 = not), in
        // Bloom.WallSlots order. No wall coordinates are stored — this is the whole point:
        // replace computation-stored-as-geometry with a tiny datastream. (See levels.md.)
        [Tooltip("Facing used to derive the wall-slot layout (which 3 sides the half-bloom grows).")]
        public int WallFacing = 1;

        [Tooltip("Wall bitstream: 1 bit per slot in Bloom.WallSlots(floor, WallFacing) order.")]
        public List<bool> WallBits = new List<bool>();

        /// <summary>Build a fast lookup from the serialized list. Call once at load.</summary>
        public Dictionary<HexCoord, Cell> ToMap()
        {
            var map = new Dictionary<HexCoord, Cell>(Cells.Count);
            foreach (var e in Cells)
                map[e.Coord] = e.Cell; // last write wins on dup coords (editor should prevent dups)
            return map;
        }

        public Cell GetCell(HexCoord c)
        {
            foreach (var e in Cells)
                if (e.Q == c.Q && e.R == c.R) return e.Cell;
            return Cell.None;
        }

        /// <summary>All hexes that are part of the chunk (any non-None cell).</summary>
        public IEnumerable<HexCoord> Footprint()
        {
            foreach (var e in Cells)
                if (e.Type != CellType.None) yield return e.Coord;
        }

        /// <summary>Connection ports — where the crystal maze may attach a corridor.</summary>
        public IEnumerable<HexCoord> Ports()
        {
            foreach (var e in Cells)
                if (e.Type == CellType.Port) yield return e.Coord;
        }
    }
}
