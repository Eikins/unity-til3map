//-----------------------------------------------------------------
// File:         Tile3DPaletteEditor.cs
// Description:  Tile3D palette asset editor.
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         24/04/2024
//-----------------------------------------------------------------
using System.Linq;
using Til3map;
using UnityEditor;
using UnityEngine;

namespace Til3mapEditor
{
    [CustomEditor(typeof(Tile3DPalette))]
    public class Tile3DPaletteEditor : Editor
    {
        private Tile3DPalette _Palette;
        private int _tileIndex = -1;
        private int _paletteIndex = -1;

        public Tile3DPalette Tileset => _Palette;
        public int TileIndex => _tileIndex;

        private Vector2 _scrollPosition;
        private string _searchFilter;
        private int _tilePickerControlId = -1;

        private GUIContent _addIcon;
        private GUIContent _removeIcon;

        private void OnEnable()
        {
            _addIcon = EditorGUIUtility.IconContent("Toolbar Plus");
            _removeIcon = EditorGUIUtility.IconContent("Toolbar Minus");
            _searchFilter = "";
            _tileIndex = -1;

            _Palette = target as Tile3DPalette;
        }

        private void OnDisable()
        {
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(10f);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Tileset", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            _searchFilter = DrawSearchField(_searchFilter, ref _paletteIndex);
            DrawAddTileButton();
            DrawRemoveTileButton();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            bool changed = false;
            DrawPalette(_Palette, _searchFilter, ref _tileIndex, ref _paletteIndex, ref _scrollPosition, ref changed);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRemoveTileButton()
        {
            EditorGUI.BeginDisabledGroup(_tileIndex < 0 || _tileIndex >= _Palette.Count);
            if (GUILayout.Button(_removeIcon, EditorStyles.toolbarButton, GUILayout.Width(40f)))
            {
                //_Palette.Tiles.RemoveAt(_tileIndex);
                EditorUtility.SetDirty(_Palette);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawAddTileButton()
        {
            if (GUILayout.Button(_addIcon, EditorStyles.toolbarButton, GUILayout.Width(40f)) && _tilePickerControlId == -1)
            {
                _tilePickerControlId = GUIUtility.GetControlID(FocusType.Passive);
                EditorGUIUtility.ShowObjectPicker<Tile3D>(null, false, "", _tilePickerControlId);
            }

            string commandName = Event.current.commandName;
            if (commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == _tilePickerControlId)
            {
                Tile3D tile = EditorGUIUtility.GetObjectPickerObject() as Tile3D;
                if (tile != null)
                {
                    if (_Palette.TryAddTile(tile))
                    {
                        EditorUtility.SetDirty(_Palette);
                    }
                }
                _tilePickerControlId = -1;
            }
        }

        public static void DrawPalette(Tile3DPalette palette, string searchFilter, ref int tileIndex, ref int paletteIndex, ref Vector2 scrollPosition, ref bool changed, params GUILayoutOption[] options)
        {
            const float gridSize = 400;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
            var tiles = palette.Tiles;
            if (tiles != null)
            {
                EditorGUI.BeginChangeCheck();
                var gridElements = palette.Tiles.Select((tile) =>
                {
                    var content = new GUIContent(AssetPreview.GetAssetPreview(tile.Value));
                    content.tooltip = tile.Value.name;
                    return content;
                }).ToArray();

                paletteIndex = GUILayout.SelectionGrid(paletteIndex, gridElements, 4, GUILayout.Width(gridSize));

                if (EditorGUI.EndChangeCheck())
                {
                    changed = true;
                    if (paletteIndex < 0 || paletteIndex >= tiles.Count)
                    {
                        tileIndex = -1;
                    }
                    else
                    {
                        //tileIndex = tiles.IndexOf(tiles[paletteIndex]);
                    }
                }
            }

            GUILayout.EndScrollView();
        }

        public static string DrawSearchField(string searchText, ref int paletteIndex)
        {
            EditorGUI.BeginChangeCheck();
            searchText = GUILayout.TextField(searchText, EditorStyles.toolbarSearchField, GUILayout.Width(80f));
            if (EditorGUI.EndChangeCheck())
            {
                paletteIndex = -1;
            }
            return searchText;
        }
    }
}