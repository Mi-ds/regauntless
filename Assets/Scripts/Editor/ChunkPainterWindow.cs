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
        private readonly bool _pointyTop = false; // flat-top default (display only)
        private Vector2 _pan = new Vector2(320, 260);

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
            DrawPalette();
            DrawGrid();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _target = (ChunkData)EditorGUILayout.ObjectField(_target, typeof(ChunkData), false, GUILayout.Width(220));
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50))) Load();
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50))) Save();
            if (GUILayout.Button("Save As", EditorStyles.toolbarButton, GUILayout.Width(70))) SaveAs();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"cells: {_cells.Count}", EditorStyles.miniLabel);
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
            { if (EditorUtility.DisplayDialog("Clear", "Erase all painted cells?", "Yes", "No")) _cells.Clear(); Repaint(); }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _radius = EditorGUILayout.IntSlider("Grid radius", _radius, 1, 12);
            _hexSize = EditorGUILayout.Slider("Hex size", _hexSize, 10f, 48f);
            EditorGUILayout.EndHorizontal();
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
                                    "Variant is cosmetic only.", MessageType.None);
        }

        private void DrawGrid()
        {
            Rect area = GUILayoutUtility.GetRect(position.width, position.height - 140);
            var layout = new HexLayout(_hexSize, _pointyTop, new Vector2(area.x + _pan.x, area.y + _pan.y));

            // handle mouse paint
            Event e = Event.current;
            if (area.Contains(e.mousePosition) &&
                (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
            {
                HexCoord h = layout.PixelToHex(e.mousePosition);
                if (HexCoord.Zero.DistanceTo(h) <= _radius)
                {
                    if (e.button == 0) _cells[h] = new Cell(_brush, _brushVariant);
                    else if (e.button == 1) _cells.Remove(h);
                    e.Use();
                    Repaint();
                }
            }

            // draw every hex in range; painted ones get their color, empties a faint outline
            Handles.BeginGUI();
            foreach (var h in HexCoord.Zero.Range(_radius))
            {
                Vector2[] corners = layout.Corners(h);
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
            }
            Handles.EndGUI();
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
