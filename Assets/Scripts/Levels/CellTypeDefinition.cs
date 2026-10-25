using System;
using System.Collections.Generic;
using UnityEngine;

namespace Regauntless.Levels
{
    /// <summary>
    /// A single cosmetic variant of a cell type. Label-first, sprite-LATER: a variant is a valid,
    /// fully-functional identity with just an id + label ("Floor 2 - stub"); the sprite is a late
    /// binding (null → renderer falls back to the palette color + label). Art is a drop-in.
    /// </summary>
    [Serializable]
    public struct CellVariant
    {
        public int Id;
        [Tooltip("Human label, valid without art, e.g. \"Floor 2 - stub\".")]
        public string Label;
        [Tooltip("Optional. Null is fine — renderer falls back to palette color + label.")]
        public Sprite Sprite;
    }

    /// <summary>
    /// Data-driven catalog for one <see cref="CellType"/>: its editor palette color, display name,
    /// and its list of cosmetic variants. Adding a new texture later = drop a sprite into a
    /// variant slot (or add a slot) — zero code (fits "content is data-driven", vision.md).
    ///
    /// Gameplay NEVER reads these — this is a presentation/authoring asset only.
    /// </summary>
    [CreateAssetMenu(fileName = "CellTypeDefinition",
                     menuName = "Regauntless/Cell Type Definition")]
    public class CellTypeDefinition : ScriptableObject
    {
        public CellType Type = CellType.Floor;
        public string DisplayName = "Floor";
        [Tooltip("Swatch color in the chunk paint editor (the SRG-style legend).")]
        public Color PaletteColor = Color.gray;

        [Tooltip("Cosmetic variants. Label-first; sprites optional/added later.")]
        public List<CellVariant> Variants = new List<CellVariant>();

        /// <summary>Resolve a variant by id, falling back to the first/default if out of range.</summary>
        public CellVariant GetVariant(int id)
        {
            if (Variants == null || Variants.Count == 0)
                return new CellVariant { Id = 0, Label = DisplayName + " (default)", Sprite = null };
            foreach (var v in Variants)
                if (v.Id == id) return v;
            return Variants[0];
        }

        public int VariantCount => Variants?.Count ?? 0;
    }
}
