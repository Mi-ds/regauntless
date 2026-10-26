using UnityEngine;

namespace Regauntless.Core
{
    /// <summary>
    /// Pure hex↔pixel conversion (Red Blob Games layout math). Separate from <see cref="HexCoord"/>
    /// on purpose: coords are abstract/grid; layout is how they map to screen/world space (size +
    /// orientation + origin). Used by the chunk paint editor and later the Tilemap renderer.
    ///
    /// Orientation is a display choice (pointy-vs-flat is a "dead question" mechanically — see
    /// vision.md; the player snap-rotates the camera anyway). Default: flat-top.
    /// </summary>
    public struct HexLayout
    {
        public readonly bool PointyTop;
        public readonly float Size;      // center-to-corner distance (hex "radius")
        public readonly Vector2 Origin;
        public readonly float Spacing;   // center-distance multiplier (1 = seamless; >1 = gaps)

        public HexLayout(float size, bool pointyTop = false, Vector2 origin = default, float spacing = 1f)
        {
            Size = size;
            PointyTop = pointyTop;
            Origin = origin;
            Spacing = spacing <= 0f ? 1f : spacing;
        }

        /// <summary>Axial hex → pixel center. Centers are spread by <see cref="Spacing"/>.</summary>
        public Vector2 HexToPixel(HexCoord h)
        {
            float s = Size * Spacing; // spacing affects CENTER layout, not drawn hex size
            float x, y;
            if (PointyTop)
            {
                x = s * (Mathf.Sqrt(3f) * h.Q + Mathf.Sqrt(3f) / 2f * h.R);
                y = s * (1.5f * h.R);
            }
            else // flat-top
            {
                x = s * (1.5f * h.Q);
                y = s * (Mathf.Sqrt(3f) / 2f * h.Q + Mathf.Sqrt(3f) * h.R);
            }
            return new Vector2(x + Origin.x, y + Origin.y);
        }

        /// <summary>
        /// Pixel → rounded hex. Exact algebraic inverse of <see cref="HexToPixel"/> (same spaced
        /// scale s = Size*Spacing), so clicking a hex center returns that hex. For picking.
        /// </summary>
        public HexCoord PixelToHex(Vector2 p)
        {
            float s = Size * Spacing;
            float px = (p.x - Origin.x) / s;
            float py = (p.y - Origin.y) / s;
            float q, r;
            if (PointyTop)
            {
                // inverse of: px = √3·q + (√3/2)·r ; py = 1.5·r
                r = py / 1.5f;
                q = (px - Mathf.Sqrt(3f) / 2f * r) / Mathf.Sqrt(3f);
            }
            else // flat-top — inverse of: px = 1.5·q ; py = (√3/2)·q + √3·r
            {
                q = px / 1.5f;
                r = (py - Mathf.Sqrt(3f) / 2f * q) / Mathf.Sqrt(3f);
            }
            return HexCoord.CubeRound(q, -q - r, r);
        }

        /// <summary>The 6 corner offsets (pixels) around a hex center, for drawing the polygon.</summary>
        public Vector2[] Corners(HexCoord h)
        {
            Vector2 center = HexToPixel(h);
            var corners = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float angleDeg = 60f * i + (PointyTop ? 30f : 0f);
                float a = Mathf.Deg2Rad * angleDeg;
                corners[i] = new Vector2(
                    center.x + Size * Mathf.Cos(a),
                    center.y + Size * Mathf.Sin(a));
            }
            return corners;
        }
    }
}
