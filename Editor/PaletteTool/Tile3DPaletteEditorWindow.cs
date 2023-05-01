//-----------------------------------------------------------------
// File:         Tile3DPaletteEditorWindow.cs
// Description:  Palette editor window.
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         24/04/2024
//-----------------------------------------------------------------
using Til3map;
using Til3mapEditor.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Til3mapEditor
{
    public class Tile3DPaletteEditorWindow : EditorWindow
    {
        [MenuItem("Window/Til3map/Palette Editor", priority = 90001)]
        public static void ShowTilemapEditor()
        {
            var wnd = GetWindow<Tile3DPaletteEditorWindow>();
            wnd.titleContent = new GUIContent("Tile3D Palette Editor");
        }

        [SerializeField] private VisualTreeAsset _visualTreeAsset;

        // Palette View
        private PaletteView _paletteView;
        private VisualElement _tileSelectionSquareElement;

        [SerializeField] private Tile3DPalette _palette;
        private Tile3D _tile;
        private Vector2Int _tileIndex2D;

        private ObjectField _tileField;
        private Editor _tileEditor = null;

        #region GUI Creation
        public void CreateGUI()
        {
            // Load Model and Stylesheet
            _visualTreeAsset.CloneTree(rootVisualElement);

            // Bind Palette Field
            {
                ObjectField paletteField = rootVisualElement.Query<ObjectField>(name = "palette-field").First();
                paletteField.objectType = typeof(Tile3DPalette);
                paletteField.value = null;
                paletteField.RegisterCallback<ChangeEvent<Object>>(e =>
                {
                    _palette = e.newValue as Tile3DPalette;
                    _paletteView.SetPalette(_palette);
                    _tileField.SetEnabled(_palette != null);
                    OnTileIndex2DChanged(_tileIndex2D);
                });
            }

            // Bind Tile Field
            {
                _tileField = rootVisualElement.Query<ObjectField>(name = "tile-field").First();
                _tileField.objectType = typeof(Tile3D);
                _tileField.value = null;
                _tileField.RegisterCallback<ChangeEvent<Object>>(e =>
                {
                    ChangeTile(e.newValue as Tile3D);
                });
            }

            // Register Tile Inspector
            IMGUIContainer tileInspector = rootVisualElement.Query<IMGUIContainer>(name = "tile-unity-inspector").First();
            tileInspector.onGUIHandler += DrawTileInspector;


            // Initialize Palette
            InitTilePalette();
            _tileField.SetEnabled(_palette != null);
        }

        private void Update()
        {
            _paletteView.UpdatePreviewLoadingTasks();
        }

        private void DrawTileInspector()
        {
            if (_tileEditor)
            {
                EditorGUIUtility.wideMode = true;
                _tileEditor.OnInspectorGUI();
            }
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
                OnTileIndex2DChanged(tilePosition);
            };

            _paletteView.AddManipulator(new ContentDragger());
            _paletteView.AddManipulator(new ContentZoomer());
            _paletteView.AddManipulator(tileSelector);
        }
        #endregion

        private void ChangeTile(Tile3D tile)
        {
            if (_palette != null)
            {
                if (tile != null)
                {
                    _palette.Tiles[_tileIndex2D] = tile;
                }
                else
                {
                    _palette.Tiles.Remove(_tileIndex2D);
                }

                EditorUtility.SetDirty(_palette);

                _tile = tile;
                RefreshTileInspector();
                _paletteView.SetPalette(_palette);
            }
        }

        private void OnTileIndex2DChanged(Vector2Int newTileIndex2D)
        {
            _tileIndex2D = newTileIndex2D;

            if (_palette)
            {
                if (_palette.Tiles.TryGetValue(newTileIndex2D, out var tile))
                {
                    _tile = tile;
                }
                else
                {
                    _tile = null;
                }

                _tileField.value = _tile;
            }
            else
            {
                _tile = null;
                _tileField.value = null;
            }
            RefreshTileInspector();
        }

        private void RefreshTileInspector()
        {
            if (_tile != null)
            {
                _tileEditor = Editor.CreateEditor(_tile);
            }
            else
            {
                _tileEditor = null;
            }
        }
    }
}