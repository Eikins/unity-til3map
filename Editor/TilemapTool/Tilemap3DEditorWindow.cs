//-----------------------------------------------------------------
// File:         Tilemap3DEditorWindow.cs
// Description:  Tilemap editor tool
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         25/04/2024
//-----------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Til3map;
using Til3mapEditor.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Til3mapEditor
{
    public class Tilemap3DEditorWindow : EditorWindow
    {
        [MenuItem("Window/Til3map/Tilemap Editor", priority = 90000)]
        public static void ShowTilemapEditor()
        {
            Tilemap3DEditorWindow wnd = GetWindow<Tilemap3DEditorWindow>();
            wnd.titleContent = new GUIContent("Tilemap 3D Editor");
        }

        [SerializeField] private VisualTreeAsset _visualTreeAsset;

        private TilemapEditor _editor = new TilemapEditor();

        // Toolbar State
        private List<TilemapEditorTool> _tools;
        private int _currentToolIndex = 0;
        private bool _showGrid = true;
        private Color _gridColor = new Color(0.5f, 0.5f, 0.5f);

        // Toolbar Icons
        private GUIContent _increaseHeightIcon;
        private GUIContent _decreaseHeightIcon;
        private GUIContent _eraserIcon;
        private GUIContent _rotateIcon;
        private GUIContent _gridIcon;

        // Palette View
        private PaletteView _paletteView;
        private VisualElement _tileSelectionSquareElement;
        [SerializeField] private Tile3DPalette _palette;

        // Rendering
        private CommandBuffer _commandBuffer;

        private TilemapEditorTool CurrentTool => _tools[_currentToolIndex];

        #region GUI Creation
        public void CreateGUI()
        {
            // Load Model and Stylesheet
            _visualTreeAsset.CloneTree(rootVisualElement);

            // Initialize Tools
            InitTools();
            IMGUIContainer toolbarContainer = rootVisualElement.Query<IMGUIContainer>(name = "tilemap-toolbar").First();
            toolbarContainer.onGUIHandler += DrawTilemapToolbar;

            // Bind Palette Field
            ObjectField paletteField = rootVisualElement.Query<ObjectField>(name = "palette-field").First();
            paletteField.objectType = typeof(Tile3DPalette);
            paletteField.value = _palette;
            _editor.SetPalette(_palette);
            paletteField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                _palette = e.newValue as Tile3DPalette;
                _editor.SetPalette(_palette);
                _paletteView.SetPalette(_palette);
            });

            // Initialize Palette
            InitTilePalette();
        }

        private void InitTools()
        {
            _increaseHeightIcon = EditorGUIUtility.IconContent("TerrainInspector.TerrainToolRaise");
            _decreaseHeightIcon = EditorGUIUtility.IconContent("TerrainInspector.TerrainToolLowerAlt");
            _eraserIcon = EditorGUIUtility.IconContent("Grid.EraserTool");
            _rotateIcon = EditorGUIUtility.IconContent("RotateTool");
            _gridIcon = EditorGUIUtility.IconContent("ToggleUVOverlay");

            _increaseHeightIcon.tooltip = "Increase Height (T)";
            _decreaseHeightIcon.tooltip = "Decrease Height (G)";
            _eraserIcon.tooltip = "Toggle Eraser (E)";
            _rotateIcon.tooltip = "Rotate Tile (R)";
            _gridIcon.tooltip = "Toggle Grid";

            _tools = new List<TilemapEditorTool>
            {
                new TilemapEditorSelectionTool(_editor),
                new TilemapEditorPaintTool(_editor),
                new TilemapEditorSquareTool(_editor)
            };
        }

        private void InitTilePalette()
        {
            _paletteView = rootVisualElement.Query<PaletteView>(className: "palette-view").First();

            var tileSelector = new TileSelector();

            _tileSelectionSquareElement = new VisualElement();
            _tileSelectionSquareElement.style.position = Position.Absolute;
            _tileSelectionSquareElement.transform.position = Vector2.zero;
            _tileSelectionSquareElement.style.width = PaletteView.s_TileSize;
            _tileSelectionSquareElement.style.height = PaletteView.s_TileSize;

            _tileSelectionSquareElement.style.borderTopColor = Color.white;
            _tileSelectionSquareElement.style.borderRightColor = Color.white;
            _tileSelectionSquareElement.style.borderBottomColor = Color.white;
            _tileSelectionSquareElement.style.borderLeftColor = Color.white;

            _tileSelectionSquareElement.style.borderTopWidth = 2f;
            _tileSelectionSquareElement.style.borderRightWidth = 2f;
            _tileSelectionSquareElement.style.borderBottomWidth = 2f;
            _tileSelectionSquareElement.style.borderLeftWidth = 2f;

            _paletteView.ViewContentContainer.Add(_tileSelectionSquareElement);

            tileSelector.onTileSelectionChanged += (tilePosition) =>
            {
                _tileSelectionSquareElement.transform.position = (Vector2)tilePosition * PaletteView.s_TileSize;
                _editor.SetSelectedTilePosition(tilePosition);
            };

            _paletteView.AddManipulator(new ContentDragger());
            _paletteView.AddManipulator(new ContentZoomer());
            _paletteView.AddManipulator(tileSelector);

            _paletteView.SetPalette(_palette);
        }
        #endregion

        #region Unity Callbacks / Logic
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;

            _commandBuffer = new CommandBuffer()
            {
                name = "Tilemap 3D Editor Preview"
            };
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            OnSelectionChange();
        }

        private void Update()
        {
            if (_paletteView != null)
            {
                _paletteView.UpdatePreviewLoadingTasks();
            }

            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;

            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            _commandBuffer.Release();
            Tools.hidden = false;

            if (_tools != null)
            {
                foreach (var tool in _tools)
                {
                    tool.Dispose();
                }
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            Tilemap3D tilemap = null;
            if (Selection.activeGameObject != null)
            {
                tilemap = Selection.activeGameObject.GetComponent<Tilemap3D>();
            }

            _editor.SetTilemap(tilemap);
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            HandleInputs();

            if (_editor.Tilemap != null)
            {
                if (_showGrid)
                {
                    DrawGrid(_editor.Tilemap, _editor.Height);
                }

                CurrentTool.PrepareSceneGUI();
                CurrentTool.OnSceneGUI();
            }

            if (Event.current.type == EventType.Repaint && RenderPipelineManager.currentPipeline == null)
            {
                if (RecordCommandBuffer(sceneView.camera))
                {
                    Graphics.ExecuteCommandBuffer(_commandBuffer);
                }
            }
        }
        
        private void OnUndoRedoPerformed()
        {
            _editor.Tilemap?.OnTilesChanged?.Invoke();
        }
        #endregion

        #region Drawing / Rendering
        private void DrawTilemapToolbar()
        {
            GUILayout.BeginHorizontal(GUILayout.Height(24f));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(GUILayout.Height(24f));
            _currentToolIndex = GUILayout.Toolbar(_currentToolIndex, _tools.Select((tool) => tool.toolbarIcon).ToArray(), GUILayout.Height(24f));
            Tools.hidden = _currentToolIndex > 0;

            EditorGUILayout.Space();
            bool eraserEnabled = GUILayout.Toggle(_editor.IsEraserEnabled, _eraserIcon, "Button", GUILayout.Height(24f));
            if (eraserEnabled != _editor.IsEraserEnabled) _editor.ToggleEraser();

            EditorGUILayout.Space();
            if (GUILayout.Button(_rotateIcon, GUILayout.Height(24f))) _editor.RotateTile();
            if (GUILayout.Button(_increaseHeightIcon, GUILayout.Height(24f))) _editor.IncreaseHeight();
            if (GUILayout.Button(_decreaseHeightIcon, GUILayout.Height(24f))) _editor.DecreaseHeight();

            EditorGUILayout.Space();

            _showGrid = GUILayout.Toggle(_showGrid, _gridIcon, "Button", GUILayout.Height(24f));
            _gridColor = EditorGUILayout.ColorField(new GUIContent(), _gridColor, false, true, false, GUILayout.Width(8f), GUILayout.Height(8f));

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawGrid(Tilemap3D tilemap, int height)
        {
            var rect = tilemap.Rect;

            var handleMatrix = Handles.matrix;
            var handleZTest = Handles.zTest;

            Handles.matrix = tilemap.transform.localToWorldMatrix;

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = _gridColor;
            
            for (int x = rect.xMin; x <= rect.xMax; x++)
            {
                var p1 = new Vector3(x, height + 0.02f, rect.yMin);
                var p2 = new Vector3(x, height + 0.02f, rect.yMax);
                Handles.DrawLine(p1, p2);
            }

            for (int z = rect.yMin; z <= rect.yMax; z++)
            {
                var p1 = new Vector3(rect.xMin, height + 0.02f, z);
                var p2 = new Vector3(rect.xMax, height + 0.02f, z);
                Handles.DrawLine(p1, p2);
            }

            Handles.Label(new Vector3(rect.xMin - 0.5f, height, rect.yMin - 0.5f), $"y = {height}");

            Handles.matrix = handleMatrix;
            Handles.zTest = handleZTest;

        }

        private bool RecordCommandBuffer(Camera camera)
        {
            var tilemap = _editor.Tilemap;
            var tile = _editor.Tile;

            if (tilemap == null) return false;
            if (tile == null || !tile.IsValid()) return false;

            if (camera.cameraType == CameraType.SceneView)
            {
                _commandBuffer.Clear();
                CurrentTool.DrawPreview(_commandBuffer);
                return true;
            }

            return false;
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (RecordCommandBuffer(camera))
            {
                context.ExecuteCommandBuffer(_commandBuffer);
                context.Submit();
            }
        }
        #endregion

        #region Inputs
        private void HandleInputs()
        {
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            switch (Event.current.type)
            {
                case EventType.Layout:
                    HandleUtility.AddDefaultControl(controlID);
                    break;
                case EventType.KeyDown:
                    if (ProcessKeyInput(Event.current.keyCode))
                    {
                        Event.current.Use();
                        Repaint();
                    }
                    break;
            }
        }

        private bool ProcessKeyInput(KeyCode keyCode)
        {
            // Do not capture input when not editing.
            if (_currentToolIndex == 0)
                return false;

            switch (keyCode)
            {
                case KeyCode.Escape:
                    CurrentTool.CancelAction();
                    return true;
                case KeyCode.T:
                    _editor.IncreaseHeight();
                    CurrentTool.RefreshPreview();
                    return true;
                case KeyCode.G:
                    _editor.DecreaseHeight();
                    CurrentTool.RefreshPreview();
                    return true;
                case KeyCode.R:
                    _editor.RotateTile();
                    CurrentTool.RefreshPreview();
                    return true;
                case KeyCode.E:
                    _editor.ToggleEraser();
                    return true;
                default:
                    return false;
            }
        }
        #endregion
    }
}