using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Regauntless.Core;
using Regauntless.Levels;

namespace Regauntless.EditorTools
{
    /// <summary>
    /// Brutal-but-functional hex chunk paint tool (Space Run Galaxy-style). Pick a cell type from
    /// the palette, click/drag hexes to paint, Save to a ChunkData asset. No polish, just works.
    ///
    /// Open via: Regauntless ▸ Chunk Painter.
    /// </summary>
    public class ChunkPainterWindow : EditorWindow
    {
        // --- grid / view ---
        private int _radius = 5;                 // how many rings of hexes to show
        private float _hexSize = 22f;            // pixels, center-to-corner
        private float _spacing = 1.15f;          // >1 spreads centers apart → visible gaps between hexes
        private readonly bool _pointyTop = false; // flat-top default (display only)
        private Vector2 _pan = new Vector2(320, 260);
        private int _viewRotation = 0;            // display rotation in 60° steps (0..5), data unchanged
        private bool _showFloorLayer = true;      // layer panel: show floor grid
        private bool _showWallLayer = true;       // overlay: floor-sized hexes on shared edges, crosswise
        private bool _wallsOnTop = true;          // draw order: which layer renders in FRONT

        // --- Hex orientation as a TRINARY (0 / 15 / 30), NOT a bool. ---------------------------
        // By hexagon 6-fold symmetry there are only TWO tessellating orientations today:
        //   0°  = aligned with the floor grid (flat-to-flat), and
        //   30° = the crosswise "other" orientation (== 90° mod 60°; this is what makes wall
        //         hexes lattice POINT-TO-POINT over the floor — see levels.md two-grid model).
        // So a bool would cover current needs. We deliberately use a THREE-stop trit (0/15/30)
        // to FUTURE-PROOF: 15° is the reserved in-between detent for future NON-tessellating
        // elements (rotated sprites/props/decorations/directional features) that may want the
        // half-step orientation. Model the fuller space now; use a subset today; no retrofit
        // later. DO NOT "simplify" this to a bool — the middle stop is intentional. 🦊
        private enum HexOrient { Deg0 = 0, Deg15 = 1, Deg30 = 2 }
        private static float OrientDegrees(HexOrient o) => (int)o * 15f;   // 0 → 0°, 1 → 15°, 2 → 30°

        private HexOrient _wallOrient = HexOrient.Deg30;  // default 30°
        private float _wallSizeFrac = 1.0f;               // same size as floor
        private bool _wallNorth = false;         // move floor copy S (opposite the N-ward bloom) — see below
        private int _wallFacing = 1;             // half-bloom NE/N/NW arc (dirs {1,2,3}); offset goes opposite (S)

        // --- paint state ---
        private CellType _brush = CellType.Floor;
        private int _brushVariant = 0;
        private readonly Dictionary<HexCoord, Cell> _cells = new Dictionary<HexCoord, Cell>();

        // --- asset io ---
        private ChunkData _target;

        // --- palette colors (fallback if no CellTypeDefinition assets are present) ---
        private static readonly Dictionary<CellType, Color> Fallback = new Dictionary<CellType, Color>
        {
            { CellType.None,      new Color(0.15f,0.15f,0.15f) },
            { CellType.Floor,     new Color(0.55f,0.55f,0.58f) },
            { CellType.Wall,      new Color(0.25f,0.20f,0.18f) },
            { CellType.Generator, new Color(0.80f,0.25f,0.25f) },
            { CellType.Port,      new Color(0.25f,0.55f,0.85f) },
            { CellType.HeroSpawn, new Color(0.30f,0.75f,0.35f) },
            { CellType.Food,      new Color(0.90f,0.75f,0.25f) },
            { CellType.Exit,      new Color(0.85f,0.45f,0.85f) },
            { CellType.Hole,      new Color(0.05f,0.05f,0.08f) }, // dark; in-game it's a bright light-well
        };

        [MenuItem("Regauntless/Chunk Painter")]
        public static void Open() => GetWindow<ChunkPainterWindow>("Chunk Painter");

        private void OnGUI()
        {
            DrawToolbar();
            DrawLayerPanel();
            DrawPalette();
            DrawGrid();
        }

        private void DrawLayerPanel()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("LAYERS:", EditorStyles.boldLabel, GUILayout.Width(60));
            _showFloorLayer = GUILayout.Toggle(_showFloorLayer, "Floor", "Button", GUILayout.Width(80));
            _showWallLayer  = GUILayout.Toggle(_showWallLayer,  "Walls", "Button", GUILayout.Width(80));
            GUILayout.FlexibleSpace();
            // draw-order: which layer is in front
            GUILayout.Label("Front:", GUILayout.Width(44));
            if (GUILayout.Button(_wallsOnTop ? "Walls ▲" : "Floor ▲", GUILayout.Width(80)))
                _wallsOnTop = !_wallsOnTop;
            GUILayout.Space(10);
            // quick isolate buttons
            if (GUILayout.Button("Floor only", GUILayout.Width(80))) { _showFloorLayer = true;  _showWallLayer = false; }
            if (GUILayout.Button("Walls only", GUILayout.Width(80))) { _showFloorLayer = false; _showWallLayer = true;  }
            if (GUILayout.Button("Both",       GUILayout.Width(60))) { _showFloorLayer = true;  _showWallLayer = true;  }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _target = (ChunkData)EditorGUILayout.ObjectField(_target, typeof(ChunkData), false, GUILayout.Width(220));
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50))) Load();
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50))) Save();
            if (GUILayout.Button("Save As", EditorStyles.toolbarButton, GUILayout.Width(70))) SaveAs();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"rot: {_viewRotation * 60}°", EditorStyles.miniLabel);
            GUILayout.Label($"cells: {_cells.Count}", EditorStyles.miniLabel);
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
            { if (EditorUtility.DisplayDialog("Clear", "Erase all painted cells?", "Yes", "No")) _cells.Clear(); Repaint(); }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _radius = EditorGUILayout.IntSlider("Grid radius", _radius, 1, 12);
            _hexSize = EditorGUILayout.Slider("Hex size", _hexSize, 10f, 48f);
            _spacing = EditorGUILayout.Slider("Spacing", _spacing, 1f, 1.6f);
            _showWallLayer = EditorGUILayout.ToggleLeft("Wall layer (on edge-mids)", _showWallLayer, GUILayout.Width(190));
            EditorGUILayout.EndHorizontal();

            if (_showWallLayer)
            {
                EditorGUILayout.BeginHorizontal();
                _wallSizeFrac = EditorGUILayout.Slider("Wall size (×floor)", _wallSizeFrac, 0.1f, 1f);
                // Trinary orientation switch (0 / 15 / 30) — see HexOrient comment above.
                GUILayout.Label("Wall orient:", GUILayout.Width(80));
                _wallOrient = (HexOrient)GUILayout.Toolbar((int)_wallOrient,
                    new[] { "0°", "15°", "30°" }, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Move floor copy:", GUILayout.Width(110));
                if (GUILayout.Button(_wallNorth ? "North ▲" : "South ▼", GUILayout.Width(80)))
                    _wallNorth = !_wallNorth;
                _wallFacing = EditorGUILayout.IntSlider("Half-bloom facing", _wallFacing, 0, 5);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawPalette()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Brush:", GUILayout.Width(44));
            foreach (CellType t in System.Enum.GetValues(typeof(CellType)))
            {
                if (t == CellType.None) continue; // None isn't a brush — right-click erases
                var prev = GUI.backgroundColor;
                GUI.backgroundColor = ColorFor(t);
                bool selected = _brush == t;
                string label = selected ? $"[{t}]" : t.ToString();
                if (GUILayout.Button(label, GUILayout.Height(22)))
                    _brush = t;
                GUI.backgroundColor = prev;
            }
            EditorGUILayout.EndHorizontal();
            _brushVariant = EditorGUILayout.IntField("Brush variant", Mathf.Max(0, _brushVariant));
            EditorGUILayout.HelpBox("Left-click/drag: paint brush.  Right-click/drag: erase (set None).  " +
                                    "Left/Right arrows: rotate view 60°.  Variant is cosmetic only.", MessageType.None);
        }

        private void DrawGrid()
        {
            Rect area = GUILayoutUtility.GetRect(position.width, position.height - 140);
            var layout = new HexLayout(_hexSize, _pointyTop, new Vector2(area.x + _pan.x, area.y + _pan.y), _spacing);

            Event e = Event.current;

            // Left/Right arrows rotate the VIEW 60° (display only — data stays orientation-fixed,
            // mirroring the in-game "world fixed, camera snap-rotates" model).
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.LeftArrow)  { _viewRotation = (_viewRotation + 5) % 6; e.Use(); Repaint(); }
                else if (e.keyCode == KeyCode.RightArrow) { _viewRotation = (_viewRotation + 1) % 6; e.Use(); Repaint(); }
            }

            // handle mouse paint — pick the DISPLAYED hex, then un-rotate to the DATA hex
            if (area.Contains(e.mousePosition) &&
                (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
            {
                HexCoord displayed = layout.PixelToHex(e.mousePosition);
                HexCoord h = displayed.Rotate60(-_viewRotation); // back to data space
                if (HexCoord.Zero.DistanceTo(h) <= _radius)
                {
                    if (e.button == 0) _cells[h] = new Cell(_brush, _brushVariant);
                    else if (e.button == 1) _cells.Remove(h);
                    e.Use();
                    Repaint();
                }
            }

            // draw layers in the order set by the panel (which layer is in FRONT).
            Handles.BeginGUI();
            if (_wallsOnTop)
            {
                if (_showFloorLayer) DrawFloorLayer(layout);
                if (_showWallLayer)  DrawWallLayer(layout);
            }
            else
            {
                if (_showWallLayer)  DrawWallLayer(layout);
                if (_showFloorLayer) DrawFloorLayer(layout);
            }
            Handles.EndGUI();
        }

        private void DrawFloorLayer(HexLayout layout)
        {
            foreach (var h in HexCoord.Zero.Range(_radius))
            {
                HexCoord displayed = h.Rotate60(_viewRotation);
                Vector2[] corners = layout.Corners(displayed);
                var pts = new Vector3[7];
                for (int i = 0; i < 6; i++) pts[i] = corners[i];
                pts[6] = corners[0];

                Color fill = _cells.TryGetValue(h, out var cell)
                    ? ColorFor(cell.Type)
                    : new Color(0.12f, 0.12f, 0.12f, 0.6f);

                Handles.color = fill;
                Handles.DrawAAConvexPolygon(pts);
                Handles.color = new Color(0, 0, 0, 0.5f);
                Handles.DrawPolyLine(pts);

                // coordinate label (axial q,r) — ground truth for talking about positions
                Vector2 c = layout.HexToPixel(displayed);
                var lbl = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
                Rect lr = new Rect(c.x - _hexSize, c.y - 8, _hexSize * 2, 16);
                GUI.Label(lr, $"{h.Q},{h.R}", lbl);
            }
        }
        // --- WALL LAYER = Mids' recipe: copy the floor layer, offset half a cell (left + toward a
        //     flat face), rotate each hex 30° in place. The point-to-point lattice EMERGES from
        //     these transforms — do NOT try to manufacture it per-edge (that was the fox's earlier
        //     over-engineering). Plus a padding ring (radius+1) so walls enclose the floor edges.
        private void DrawWallLayer(HexLayout layout)
        {
            float s = _hexSize * _spacing;

            // SIMPLE: move the floor copy half a cell straight NORTH or SOUTH (pure vertical,
            // zero horizontal), then half-bloom the other 3 directions. For flat-top, the N-S
            // center spacing is √3*size, so "half a cell" N/S = (√3/2)*size. Sign = N or S.
            float halfCellNS = (Mathf.Sqrt(3f) / 2f) * s;
            Vector2 offset = new Vector2(0f, _wallNorth ? -halfCellNS : halfCellNS);

            // Copy the floor footprint, then HALF-BLOOM it (grow one step on 3 adjacent sides) —
            // that combined set IS the wall grid. Then offset + rotate each 30° (the point-to-
            // point lattice emerges). See .kiro/steering/levels.md.
            var floor = HexCoord.Zero.Range(_radius);
            var wallHexes = new HashSet<HexCoord>(floor);              // copy the floor
            foreach (var h in Bloom.HalfBloom(floor, _wallFacing))     // + half-bloom it
                wallHexes.Add(h);

            foreach (var h in wallHexes)
            {
                HexCoord displayed = h.Rotate60(_viewRotation);
                Vector2 center = layout.HexToPixel(displayed) + offset;
                DrawRotatedHex(center, _hexSize, OrientDegrees(_wallOrient));
            }
        }

        /// <summary>Draw a same-size hex outline centered at a pixel point, rotated by degrees.</summary>
        private void DrawRotatedHex(Vector2 center, float size, float rotationDeg)
        {
            var pts = new Vector3[7];
            for (int i = 0; i < 6; i++)
            {
                // flat-top base corners at 60*i, plus the extra rotation
                float ang = Mathf.Deg2Rad * (60f * i + rotationDeg);
                pts[i] = new Vector3(center.x + size * Mathf.Cos(ang),
                                     center.y + size * Mathf.Sin(ang), 0);
            }
            pts[6] = pts[0];
            Handles.color = new Color(0.9f, 0.5f, 0.15f, 0.9f); // amber wall-layer outline (fox colors 🦊)
            Handles.DrawPolyLine(pts);
        }

        private Color ColorFor(CellType t) =>
            Fallback.TryGetValue(t, out var c) ? c : Color.magenta;

        // --- IO ---
        private void Load()
        {
            if (_target == null) return;
            _cells.Clear();
            foreach (var entry in _target.Cells)
                _cells[entry.Coord] = entry.Cell;
            Repaint();
        }

        private void Save()
        {
            if (_target == null) { SaveAs(); return; }
            WriteInto(_target);
            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
        }

        private void SaveAs()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Chunk", "Chunk", "asset", "Save chunk asset", "Assets");
            if (string.IsNullOrEmpty(path)) return;
            var data = CreateInstance<ChunkData>();
            WriteInto(data);
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            _target = data;
        }

        private void WriteInto(ChunkData data)
        {
            data.Cells.Clear();
            foreach (var kv in _cells)
                data.Cells.Add(new ChunkData.Entry
                {
                    Q = kv.Key.Q, R = kv.Key.R,
                    Type = kv.Value.Type, Variant = kv.Value.Variant
                });
        }
    }
}
