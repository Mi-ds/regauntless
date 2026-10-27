using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Regauntless.Core;
using Regauntless.Levels;

namespace Regauntless.EditorTools
{
    /// <summary>
    /// Chunk painter — two REAL data layers, document model (see .kiro/steering/levels.md):
    ///
    ///   FLOOR layer : floor cells on the packed hex grid (what you paint = the footprint + types).
    ///   WALL layer  : a BITSTREAM. Slot POSITIONS are DERIVED from the floor via
    ///                 Bloom.WallSlots(floor, facing) (floor copy + half-bloom); we store only
    ///                 one bit per slot (present/absent). No wall coordinates are stored — the
    ///                 whole point is to replace computed geometry with a tiny datastream.
    ///
    /// The painter READS and EDITS data; it never treats the wall lattice as a live computation.
    /// Load replaces the workspace entirely (no more "loads overtop"). Standard file dialogs.
    /// Open via: Regauntless ▸ Chunk Painter.
    /// </summary>
    public class ChunkPainterWindow : EditorWindow
    {
        private enum Layer { Floor, Walls }

        // --- workspace (the one open document) ---
        private readonly Dictionary<HexCoord, Cell> _floor = new Dictionary<HexCoord, Cell>();
        private readonly Dictionary<int, WallSlotState> _walls = new Dictionary<int, WallSlotState>(); // slot index -> state (absent = None)
        private WallSlotState _wallBrush = WallSlotState.Full;        // which wall the Walls-layer brush paints
        private int _wallFacing = 1;                                  // half-bloom facing for slot derivation
        private string _openPath = null;                               // asset path of the open chunk (null = unsaved)
        private string _chunkType = "generic";
        private bool _dirty = false;

        // --- view ---
        private int _radius = 5;
        private float _hexSize = 22f;
        private float _spacing = 1.15f;
        private Vector2 _pan = new Vector2(360, 300);
        private int _viewRotation = 0;
        private Layer _active = Layer.Floor;
        private bool _showFloor = true, _showWalls = true;

        // --- floor brush ---
        private CellType _brush = CellType.Floor;

        private static readonly Dictionary<CellType, Color> Palette = new Dictionary<CellType, Color>
        {
            { CellType.Floor,     new Color(0.55f,0.55f,0.58f) },
            { CellType.Generator, new Color(0.80f,0.25f,0.25f) },
            { CellType.Port,      new Color(0.25f,0.55f,0.85f) },
            { CellType.HeroSpawn, new Color(0.30f,0.75f,0.35f) },
            { CellType.Food,      new Color(0.90f,0.75f,0.25f) },
            { CellType.Exit,      new Color(0.85f,0.45f,0.85f) },
            { CellType.Hole,      new Color(0.05f,0.05f,0.08f) },
        };

        [MenuItem("Regauntless/Chunk Painter")]
        public static void Open() => GetWindow<ChunkPainterWindow>("Chunk Painter");

        private void OnGUI()
        {
            DrawMenuBar();
            DrawLayerBar();
            if (_active == Layer.Floor) DrawFloorPalette();
            else DrawWallBar();
            DrawCanvas();
        }

        // ---------------------------------------------------------------- menu (real File dropdown)
        private void DrawMenuBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("File", EditorStyles.toolbarDropDown, GUILayout.Width(44)))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("New"), false, NewChunk);
                menu.AddItem(new GUIContent("Open…"), false, OpenChunk);
                menu.AddSeparator("");
                if (string.IsNullOrEmpty(_openPath)) menu.AddDisabledItem(new GUIContent("Save"));
                else menu.AddItem(new GUIContent("Save"), false, Save);
                menu.AddItem(new GUIContent("Save As…"), false, SaveAs);
                menu.DropDown(new Rect(0, EditorStyles.toolbar.fixedHeight, 0, 0));
            }

            GUILayout.Space(12);
            string title = string.IsNullOrEmpty(_openPath) ? "(unsaved)" : System.IO.Path.GetFileNameWithoutExtension(_openPath);
            GUILayout.Label((_dirty ? "*" : "") + title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"floor:{_floor.Count}  walls:{_walls.Count}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        // ---------------------------------------------------------------- layers
        private void DrawLayerBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("PAINT:", EditorStyles.boldLabel, GUILayout.Width(50));
            if (GUILayout.Toggle(_active == Layer.Floor, "Floor", "Button", GUILayout.Width(70))) _active = Layer.Floor;
            if (GUILayout.Toggle(_active == Layer.Walls, "Walls", "Button", GUILayout.Width(70))) _active = Layer.Walls;
            GUILayout.Space(16);
            GUILayout.Label("SHOW:", GUILayout.Width(44));
            _showFloor = GUILayout.Toggle(_showFloor, "Floor", GUILayout.Width(54));
            _showWalls = GUILayout.Toggle(_showWalls, "Walls", GUILayout.Width(54));
            GUILayout.FlexibleSpace();
            _radius = EditorGUILayout.IntSlider("Radius", _radius, 1, 10, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFloorPalette()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Brush:", GUILayout.Width(44));
            foreach (var kv in Palette)
            {
                var prev = GUI.backgroundColor;
                GUI.backgroundColor = kv.Value;
                if (GUILayout.Button(_brush == kv.Key ? "[" + kv.Key + "]" : kv.Key.ToString(), GUILayout.Height(22)))
                    _brush = kv.Key;
                GUI.backgroundColor = prev;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("FLOOR: left-click/drag paint brush · right-click/drag erase.", MessageType.None);
        }

        private void DrawWallBar()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Brush:", GUILayout.Width(44));
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.9f, 0.5f, 0.15f);
            if (GUILayout.Button(_wallBrush == WallSlotState.Full ? "[Wall]" : "Wall", GUILayout.Height(22), GUILayout.Width(90))) _wallBrush = WallSlotState.Full;
            GUI.backgroundColor = new Color(0.9f, 0.7f, 0.35f);
            if (GUILayout.Button(_wallBrush == WallSlotState.Half ? "[Half-wall]" : "Half-wall", GUILayout.Height(22), GUILayout.Width(110))) _wallBrush = WallSlotState.Half;
            GUI.backgroundColor = prev;
            GUILayout.Space(16);
            int newFacing = EditorGUILayout.IntSlider("Half-bloom facing", _wallFacing, 0, 5);
            if (newFacing != _wallFacing) { _wallFacing = newFacing; _walls.Clear(); _dirty = true; } // slot layout changed
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("WALLS: left-click a slot = paint selected brush (Wall / Half-wall) · right-click = clear to None (door). " +
                                    "None=door (passes move+LoS) · Half=window (blocks move, passes LoS) · Full=wall (blocks both). " +
                                    "Slots derived from the floor (floor + half-bloom).", MessageType.None);
        }

        // ---------------------------------------------------------------- canvas
        private void DrawCanvas()
        {
            Rect area = GUILayoutUtility.GetRect(position.width, position.height - 150);
            var layout = new HexLayout(_hexSize, false, new Vector2(area.x + _pan.x, area.y + _pan.y), _spacing);
            Event e = Event.current;

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.LeftArrow)  { _viewRotation = (_viewRotation + 5) % 6; e.Use(); Repaint(); }
                else if (e.keyCode == KeyCode.RightArrow) { _viewRotation = (_viewRotation + 1) % 6; e.Use(); Repaint(); }
            }

            bool overCanvas = area.Contains(e.mousePosition);

            // WHEEL = zoom (only when cursor is over the canvas), toward the cursor.
            if (overCanvas && e.type == EventType.ScrollWheel)
            {
                float old = _hexSize;
                float factor = e.delta.y > 0 ? 0.9f : 1f / 0.9f;  // scroll down = zoom out
                _hexSize = Mathf.Clamp(_hexSize * factor, 4f, 80f);
                // keep the point under the cursor stable: adjust pan by the scale change
                Vector2 pivot = e.mousePosition - new Vector2(area.x, area.y);
                _pan = (_pan - pivot) * (_hexSize / old) + pivot;
                e.Use(); Repaint();
            }

            // MIDDLE-CLICK = frame-to-content: snap the view to the painted chunk (centered,
            // zoom-to-fit), pitching all empty space except the E/S growth margin. "Show me my edits."
            if (overCanvas && e.type == EventType.MouseDown && e.button == 2)
            {
                FrameToContent(area);
                e.Use(); Repaint();
            }

            // derive the wall slots from the current floor footprint (pure, every repaint — this
            // is deriving the ADDRESSING, not storing geometry; the bits are the data).
            List<HexCoord> slots = Bloom.WallSlots(FloorFootprint(), _wallFacing);

            HandleClicks(e, area, layout, slots);

            Handles.BeginGUI();
            DrawReferences(area, layout);                  // candidate dots filling the window rect
            if (_showFloor) DrawFloor(layout);
            if (_showWalls) DrawWalls(layout, slots);
            Handles.EndGUI();
        }

        /// <summary>
        /// Snap the view to frame the painted content: compute the pixel bounds of all painted
        /// floor cells, choose a zoom that fits them in the view (with a small E/S growth margin
        /// left over to extend into), and pan to center them. The "show me my edits" reset — no
        /// matter how far the user zoomed/panned away, this reframes to the content.
        /// </summary>
        private void FrameToContent(Rect area)
        {
            if (_floor.Count == 0) return;

            // content bounds in hex space
            int minQ = int.MaxValue, maxQ = int.MinValue, minR = int.MaxValue, maxR = int.MinValue;
            foreach (var h in _floor.Keys)
            {
                minQ = Mathf.Min(minQ, h.Q); maxQ = Mathf.Max(maxQ, h.Q);
                minR = Mathf.Min(minR, h.R); maxR = Mathf.Max(maxR, h.R);
            }
            // extent in "cells" (+1 each side as hex footprint, + E/S growth margin of ~2 cells)
            float marginES = 2f;
            float cellsW = (maxQ - minQ) + 1 + marginES;   // margin added on the +Q (east) side
            float cellsH = (maxR - minR) + 1 + marginES;   // margin added on the +R (south) side

            // approximate pixels-per-cell at size 1 (flat-top spacings), pick zoom to fit
            float pxPerCellX = 1.5f * _spacing;
            float pxPerCellY = Mathf.Sqrt(3f) * _spacing;
            float fitX = (area.width  * 0.9f) / (cellsW * pxPerCellX);
            float fitY = (area.height * 0.9f) / (cellsH * pxPerCellY);
            _hexSize = Mathf.Clamp(Mathf.Min(fitX, fitY), 6f, 80f);

            // center the content bounds in the view (bias slightly NW so the E/S margin shows)
            var layoutAtZero = new HexLayout(_hexSize, false, Vector2.zero, _spacing);
            Vector2 cMin = layoutAtZero.HexToPixel(new HexCoord(minQ, minR).Rotate60(_viewRotation));
            Vector2 cMax = layoutAtZero.HexToPixel(new HexCoord(maxQ, maxR).Rotate60(_viewRotation));
            Vector2 contentCenter = (cMin + cMax) * 0.5f;
            Vector2 viewCenter = new Vector2(area.width, area.height) * 0.5f;
            // shift view center NW a touch so the E/S margin is visible, not split evenly
            viewCenter -= new Vector2(area.width, area.height) * 0.08f;
            _pan = viewCenter - contentCenter;
        }

        /// <summary>
        /// Candidate hexes = every hex whose center falls inside the visible canvas area.
        /// Window-responsive: resize the window → more/fewer empty candidate hexes. The canvas is a
        /// pure workspace; the saved chunk is cropped to painted content (see WriteAsset).
        /// </summary>
        private IEnumerable<HexCoord> VisibleHexes(Rect area, HexLayout layout)
        {
            // find the hex-coord bounds covering the rect by sampling its corners, then pad.
            HexCoord c0 = layout.PixelToHex(new Vector2(area.xMin, area.yMin)).Rotate60(-_viewRotation);
            HexCoord c1 = layout.PixelToHex(new Vector2(area.xMax, area.yMin)).Rotate60(-_viewRotation);
            HexCoord c2 = layout.PixelToHex(new Vector2(area.xMin, area.yMax)).Rotate60(-_viewRotation);
            HexCoord c3 = layout.PixelToHex(new Vector2(area.xMax, area.yMax)).Rotate60(-_viewRotation);
            int minQ = Mathf.Min(Mathf.Min(c0.Q, c1.Q), Mathf.Min(c2.Q, c3.Q)) - 1;
            int maxQ = Mathf.Max(Mathf.Max(c0.Q, c1.Q), Mathf.Max(c2.Q, c3.Q)) + 1;
            int minR = Mathf.Min(Mathf.Min(c0.R, c1.R), Mathf.Min(c2.R, c3.R)) - 1;
            int maxR = Mathf.Max(Mathf.Max(c0.R, c1.R), Mathf.Max(c2.R, c3.R)) + 1;

            for (int q = minQ; q <= maxQ; q++)
                for (int r = minR; r <= maxR; r++)
                {
                    var h = new HexCoord(q, r);
                    Vector2 c = layout.HexToPixel(h.Rotate60(_viewRotation));
                    if (area.Contains(c)) yield return h;
                }
        }

        /// <summary>Faint center dot at every candidate hex filling the window. Pure guide — no data.</summary>
        private void DrawReferences(Rect area, HexLayout layout)
        {
            Handles.color = new Color(1f, 1f, 1f, 0.16f);
            foreach (var h in VisibleHexes(area, layout))
            {
                Vector2 c = layout.HexToPixel(h.Rotate60(_viewRotation));
                Handles.DrawLine(new Vector3(c.x - 1.5f, c.y), new Vector3(c.x + 1.5f, c.y));
                Handles.DrawLine(new Vector3(c.x, c.y - 1.5f), new Vector3(c.x, c.y + 1.5f));
            }
        }

        private IEnumerable<HexCoord> FloorFootprint()
        {
            // the footprint = painted floor cells (fallback to a radius hexagon if empty, so the
            // wall slots have something to derive from on a blank canvas).
            if (_floor.Count > 0) return new List<HexCoord>(_floor.Keys);
            return HexCoord.Zero.Range(_radius);
        }

        private void HandleClicks(Event e, Rect area, HexLayout layout, List<HexCoord> slots)
        {
            if (!area.Contains(e.mousePosition)) return;
            if (e.type != EventType.MouseDown && e.type != EventType.MouseDrag) return;

            if (_active == Layer.Floor)
            {
                HexCoord h = layout.PixelToHex(e.mousePosition).Rotate60(-_viewRotation);
                if (e.button == 0) _floor[h] = new Cell(_brush);
                else if (e.button == 1) _floor.Remove(h);
                _dirty = true; e.Use(); Repaint();
            }
            else // Walls: find the nearest slot to the click; left = paint brush, right = clear to None
            {
                if (e.button != 0 && e.button != 1) return;
                int best = -1; float bestD = float.MaxValue;
                for (int i = 0; i < slots.Count; i++)
                {
                    Vector2 c = layout.HexToPixel(slots[i].Rotate60(_viewRotation));
                    float d = (c - e.mousePosition).sqrMagnitude;
                    if (d < bestD) { bestD = d; best = i; }
                }
                if (best >= 0 && bestD < (_hexSize * _hexSize))
                {
                    if (e.button == 0) _walls[best] = _wallBrush;   // paint selected wall state
                    else _walls.Remove(best);                       // right-click = clear to None (door)
                    _dirty = true; e.Use(); Repaint();
                }
            }
        }

        private void DrawFloor(HexLayout layout)
        {
            foreach (var kv in _floor)
            {
                HexCoord disp = kv.Key.Rotate60(_viewRotation);
                Vector2[] cr = layout.Corners(disp);
                var pts = new Vector3[7];
                for (int i = 0; i < 6; i++) pts[i] = cr[i];
                pts[6] = cr[0];
                Handles.color = Palette.TryGetValue(kv.Value.Type, out var col) ? col : Color.magenta;
                Handles.DrawAAConvexPolygon(pts);
                Handles.color = new Color(0, 0, 0, 0.5f);
                Handles.DrawPolyLine(pts);
            }
        }

        private void DrawWalls(HexLayout layout, List<HexCoord> slots)
        {
            // slot geometry = the offset+rotated lattice (display only). Full = solid amber;
            // Half (window) = lighter amber; None (door) = faint outline, shown only while editing
            // the walls layer so you can see paintable slots.
            float s = _hexSize * _spacing;
            float halfCellNS = (Mathf.Sqrt(3f) / 2f) * s;
            Vector2 offset = new Vector2(0f, halfCellNS);  // half a cell, pure vertical (see levels.md)

            for (int i = 0; i < slots.Count; i++)
            {
                HexCoord disp = slots[i].Rotate60(_viewRotation);
                Vector2 center = layout.HexToPixel(disp) + offset;
                WallSlotState st = _walls.TryGetValue(i, out var v) ? v : WallSlotState.None;
                if (st == WallSlotState.None && _active != Layer.Walls) continue; // hide empty slots unless editing walls
                Color col = st switch {
                    WallSlotState.Full => new Color(0.90f, 0.50f, 0.15f, 0.95f),   // solid amber
                    WallSlotState.Half => new Color(0.95f, 0.75f, 0.35f, 0.80f),   // lighter amber (window)
                    _                  => new Color(0.90f, 0.50f, 0.15f, 0.22f),   // faint outline (door/empty slot)
                };
                DrawHexAt(center, _hexSize, 30f, col);
                if (st == WallSlotState.Half)  // draw an inner dash to read "half"
                    DrawHexAt(center, _hexSize * 0.5f, 30f, col);
            }
        }

        private void DrawHexAt(Vector2 c, float size, float rotDeg, Color color)
        {
            var pts = new Vector3[7];
            for (int i = 0; i < 6; i++)
            {
                float a = Mathf.Deg2Rad * (60f * i + rotDeg);
                pts[i] = new Vector3(c.x + size * Mathf.Cos(a), c.y + size * Mathf.Sin(a), 0);
            }
            pts[6] = pts[0];
            Handles.color = color;
            Handles.DrawPolyLine(pts);
        }

        // ---------------------------------------------------------------- document ops
        private void NewChunk()
        {
            if (!ConfirmDiscard()) return;
            _floor.Clear(); _walls.Clear();
            _openPath = null; _dirty = false;
            Repaint();
        }

        private void OpenChunk()
        {
            if (!ConfirmDiscard()) return;
            string abs = EditorUtility.OpenFilePanel("Open Chunk", ChunksDir(), "asset");
            if (string.IsNullOrEmpty(abs)) return;
            string rel = ToProjectRelative(abs);
            var data = AssetDatabase.LoadAssetAtPath<ChunkData>(rel);
            if (data == null) { EditorUtility.DisplayDialog("Open", "Not a ChunkData asset.", "OK"); return; }
            LoadFrom(data, rel);
        }

        private void LoadFrom(ChunkData data, string path)
        {
            // REPLACE the workspace entirely (no loading overtop).
            _floor.Clear(); _walls.Clear();
            foreach (var entry in data.Cells) _floor[entry.Coord] = entry.Cell;
            _wallFacing = data.WallFacing;
            _chunkType = data.ChunkType;
            data.MigrateWallsIfNeeded();  // back-compat: old bool WallBits -> tri-state WallStates
            for (int i = 0; i < data.WallStates.Count; i++)
                if (data.WallStates[i] != WallSlotState.None) _walls[i] = data.WallStates[i];
            _openPath = path; _dirty = false;
            Repaint();
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(_openPath)) { SaveAs(); return; }
            WriteAsset(_openPath);
        }

        private void SaveAs()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Chunks"))
                AssetDatabase.CreateFolder("Assets", "Chunks");
            string rel = EditorUtility.SaveFilePanelInProject("Save Chunk", "Chunk", "asset", "Save chunk", "Assets/Chunks");
            if (string.IsNullOrEmpty(rel)) return;
            WriteAsset(rel);
        }

        /// <summary>Absolute path to Assets/Chunks (created if missing) for the file dialogs.</summary>
        private static string ChunksDir()
        {
            string rel = "Assets/Chunks";
            if (!AssetDatabase.IsValidFolder(rel)) AssetDatabase.CreateFolder("Assets", "Chunks");
            return System.IO.Path.Combine(Application.dataPath, "Chunks");
        }

        private void WriteAsset(string rel)
        {
            var data = AssetDatabase.LoadAssetAtPath<ChunkData>(rel) ?? CreateInstance<ChunkData>();
            bool isNew = AssetDatabase.LoadAssetAtPath<ChunkData>(rel) == null;

            data.ChunkType = _chunkType;
            data.ChunkName = System.IO.Path.GetFileNameWithoutExtension(rel);
            data.WallFacing = _wallFacing;

            data.Cells.Clear();
            foreach (var kv in _floor)
                data.Cells.Add(new ChunkData.Entry { Q = kv.Key.Q, R = kv.Key.R, Type = kv.Value.Type, Variant = kv.Value.Variant });

            // write the wall TRI-STATE stream in WallSlots order (None/Half/Full per slot)
            var slots = Bloom.WallSlots(FloorFootprint(), _wallFacing);
            data.WallStates = new List<WallSlotState>(slots.Count);
            for (int i = 0; i < slots.Count; i++)
                data.WallStates.Add(_walls.TryGetValue(i, out var st) ? st : WallSlotState.None);
            data.WallBits = new List<bool>(); // clear legacy bitstream; WallStates is authoritative now

            if (isNew) AssetDatabase.CreateAsset(data, rel);
            else EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            _openPath = rel; _dirty = false;
            Repaint();
        }

        private bool ConfirmDiscard()
        {
            if (!_dirty) return true;
            return EditorUtility.DisplayDialog("Unsaved changes",
                "Discard unsaved changes to the current chunk?", "Discard", "Cancel");
        }

        private static string ToProjectRelative(string absolute)
        {
            absolute = absolute.Replace('\\', '/');
            string data = Application.dataPath.Replace('\\', '/');
            return absolute.StartsWith(data) ? "Assets" + absolute.Substring(data.Length) : absolute;
        }
    }
}
