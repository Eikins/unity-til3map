//-----------------------------------------------------------------
// File:         Tilemap3DSquareTool.cs
// Description:  Square paint tool.
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         25/04/2024
//-----------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Til3map;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Til3mapEditor
{
    public class TilemapEditorSquareTool : TilemapEditorTool
    {
        private GUIContent m_IconContent;
        public override GUIContent toolbarIcon => m_IconContent;

        private BoundsInt _selection;
        private Vector3Int _startPosition;
        private bool _isExpanding = false;

        private Vector3Int _lastTilePosition;
        private List<TilePose> _previewPoses;
        private InstanceBatcher<Matrix4x4> _previewInstances;

        public TilemapEditorSquareTool(TilemapEditor editor) : base(editor)
        {
            m_IconContent = EditorGUIUtility.IconContent("Grid.BoxTool");
            m_IconContent.tooltip = "Area Tool";
            _previewPoses = new List<TilePose>(64);

            _previewInstances = new InstanceBatcher<Matrix4x4>(null, false);
            UpdatePreviewMatrices();
        }

        private void UpdatePreviewMatrices()
        {
            _previewInstances.SetInstances(_previewPoses.Select((pose) => Editor.Tilemap.transform.localToWorldMatrix * pose.ToMatrix4x4() * Editor.Tile.TransformMatrix).ToList());
        }

        public override void DrawPreview(CommandBuffer cmd)
        {
            if (Editor.IsEraserEnabled || Editor.Tile == null) return;

            var tile = Editor.Tile;
            if (_isExpanding)
            {
                for (int i = 0; i < tile.Materials.Length; i++)
                {
                    var material = tile.Materials[i];
                    if (material.enableInstancing)
                    {
                        foreach (var batch in _previewInstances.Batches)
                        {
                            cmd.DrawMeshInstanced(tile.Mesh, i, material, -1, batch);
                        }
                    }
                    else
                    {
                        foreach (var matrix in _previewInstances.Instances)
                        {
                            cmd.DrawMesh(tile.Mesh, matrix, material, i);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < tile.Materials.Length; i++)
                {
                    cmd.DrawMesh(tile.Mesh, Editor.Tilemap.transform.localToWorldMatrix * TilePose.ToMatrix4x4() * tile.TransformMatrix, tile.Materials[i], i);
                }
            }
        }

        public override void OnSceneGUI()
        {
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            switch (Event.current.type)
            {
                case EventType.Layout:
                    HandleUtility.AddDefaultControl(controlID);
                    break;
                case EventType.MouseDown:
                    if (Event.current.button == 0)
                    {
                        _isExpanding = true;
                        _startPosition = TilePosition;
                        UpdateSelection(TilePosition);

                        _lastTilePosition = TilePosition;
                        UpdatePreviewPoses();
                    }
                    break;
                case EventType.MouseUp:
                    if (Event.current.button == 0)
                    {
                        if (_isExpanding)
                        {
                            _isExpanding = false;
                            UpdateSelection(TilePosition);
                            PutOrRemoveTiles();
                            _previewPoses.Clear();
                        }
                    }
                    break;
                case EventType.MouseDrag:
                    if (Event.current.button == 0)
                    {
                        UpdateSelection(TilePosition);
                        if (TilePosition != _lastTilePosition)
                        {
                            _lastTilePosition = TilePosition;
                            UpdatePreviewPoses();
                        }
                    }
                    break;
            }

            var handleMatrix = Handles.matrix;
            Handles.matrix = Editor.Tilemap.transform.localToWorldMatrix;
            DrawPreviewHandle();
            Handles.matrix = handleMatrix;
            SceneView.currentDrawingSceneView.Repaint();
        }

        public override void RefreshPreview()
        {
            if (_isExpanding) UpdatePreviewPoses();
        }

        public override void CancelAction()
        {
            _isExpanding = false;
            _previewPoses.Clear();
        }

        private void UpdateSelection(Vector3Int position)
        {
            var tileSize = Editor.Tile != null ? Editor.Tile.Size : Vector3Int.one;
            var size = position - _startPosition;

            size.x += size.x >= 0 ? 1 : -tileSize.x;
            size.y += size.y >= 0 ? 1 : -tileSize.y;
            size.z += size.z >= 0 ? 1 : -tileSize.z;

            size.x = SnapToInt((float) size.x / tileSize.x) * tileSize.x;
            size.y = SnapToInt((float) size.y / tileSize.y) * tileSize.y;
            size.z = SnapToInt((float) size.z / tileSize.z) * tileSize.z;

            var pos = _startPosition;
            if (size.x < 0) pos.x += tileSize.x;
            if (size.y < 0) pos.y += tileSize.y;
            if (size.z < 0) pos.z += tileSize.z;

            _selection = new BoundsInt(pos, size);
        }

        private int SnapToInt(float value)
        {
            return value > 0 ? Mathf.CeilToInt(value) : Mathf.FloorToInt(value);
        }

        private void PutOrRemoveTiles()
        {
            Tile3D tile = Editor.Tile;
            if (tile == null) return;

            int rotation = Editor.Rotation;
            var rect = Editor.Tilemap.Rect;

            foreach (var pos in _selection.allPositionsWithin)
            {
                var refPos = pos - _selection.min;
                if (refPos.x % Editor.Tile.Size.x == 0 &&
                    refPos.y % Editor.Tile.Size.y == 0 &&
                    refPos.z % Editor.Tile.Size.z == 0)
                {
                    var pose = new TilePose() { 
                        position = pos, 
                        rotation = rotation 
                    };
                    PutOrRemoveTile(tile, pose, Editor.IsEraserEnabled);
                }
            }
        }

        private void UpdatePreviewPoses()
        {
            if (Editor.Tile == null || !Editor.Tile.IsValid()) return;

            int rotation = Editor.Rotation;
            var rect = Editor.Tilemap.Rect;

            _previewPoses.Clear();

            foreach (var pos in _selection.allPositionsWithin)
            {
                var refPos = pos - _selection.min;
                if (refPos.x % Editor.Tile.Size.x == 0 &&
                    refPos.y % Editor.Tile.Size.y == 0 &&
                    refPos.z % Editor.Tile.Size.z == 0)
                {
                    _previewPoses.Add(new TilePose() { position = pos, rotation = rotation });
                }
            }

            UpdatePreviewMatrices();
        }

        private void DrawPreviewHandle()
        {
            Handles.color = Editor.IsEraserEnabled ? Color.red : Color.white;

            if (_isExpanding)
            {
                var size = new Vector3(
                    Mathf.Abs(_selection.size.x),
                    Mathf.Abs(_selection.size.y),
                    Mathf.Abs(_selection.size.z)
                );

                Handles.DrawWireCube(_selection.min + size / 2.0f, size);
            }
            else
            {
                var size = Editor.Tile != null ? Editor.Tile.Size : Vector3.one;
                Handles.DrawWireCube(TilePosition + size / 2.0f, size);
            }
        }
    }
}