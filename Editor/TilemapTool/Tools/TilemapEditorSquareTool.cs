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

        private Vector3Int _startPosition;
        private Vector3Int _endPosition;
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
                        _endPosition = TilePosition;
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
                            _endPosition = TilePosition;
                            PutOrRemoveTiles();
                            _previewPoses.Clear();
                        }
                    }
                    break;
                case EventType.MouseDrag:
                    if (Event.current.button == 0)
                    {
                        _endPosition = TilePosition;
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

        private void PutOrRemoveTiles()
        {
            Tile3D tile = Editor.Tile;
            if (tile == null) return;

            int rotation = Editor.Rotation;
            var rect = Editor.Tilemap.Rect;

            Vector3Int min = new Vector3Int()
            {
                x = Mathf.Clamp(Mathf.Min(_startPosition.x, _endPosition.x), rect.xMin, rect.xMax),
                y = Mathf.Min(_startPosition.y, _endPosition.y),
                z = Mathf.Clamp(Mathf.Min(_startPosition.z, _endPosition.z), rect.yMin, rect.yMax)
            };

            Vector3Int max = new Vector3Int()
            {
                x = Mathf.Clamp(Mathf.Max(_startPosition.x, _endPosition.x), rect.yMin, rect.yMax),
                y = Mathf.Max(_startPosition.y, _endPosition.y),
                z = Mathf.Clamp(Mathf.Max(_startPosition.z, _endPosition.z), rect.yMin, rect.yMax)
            };

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        var pose = new TilePose() 
                        { 
                            position = new Vector3Int(x, y, z), 
                            rotation = rotation 
                        };
                        PutOrRemoveTile(tile, pose, Editor.IsEraserEnabled);
                    }
                }
            }

        }

        private void UpdatePreviewPoses()
        {
            if (Editor.Tile == null || !Editor.Tile.IsValid()) return;

            int rotation = Editor.Rotation;
            var rect = Editor.Tilemap.Rect;

            _previewPoses.Clear();

            Vector3Int min = new Vector3Int()
            {
                x = Mathf.Clamp(Mathf.Min(_startPosition.x, _endPosition.x), rect.xMin, rect.xMax - 1),
                y = Mathf.Min(_startPosition.y, _endPosition.y),
                z = Mathf.Clamp(Mathf.Min(_startPosition.z, _endPosition.z), rect.yMin, rect.yMax - 1)
            };

            Vector3Int max = new Vector3Int()
            {
                x = Mathf.Clamp(Mathf.Max(_startPosition.x, _endPosition.x), rect.yMin, rect.yMax - 1),
                y = Mathf.Max(_startPosition.y, _endPosition.y),
                z = Mathf.Clamp(Mathf.Max(_startPosition.z, _endPosition.z), rect.yMin, rect.yMax - 1)
            };

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        var pose = new TilePose()
                        {
                            position = new Vector3Int(x, y, z),
                            rotation = rotation
                        };
                        _previewPoses.Add(pose);
                    }
                }
            }

            UpdatePreviewMatrices();
        }

        private void DrawPreviewHandle()
        {
            Handles.color = Editor.IsEraserEnabled ? Color.red : Color.white;
            var offset = new Vector3(0.5f, 0.0f, 0.5f);

            if (_isExpanding)
            {
                var center = (Vector3)(_endPosition - _startPosition) / 2.0f + _startPosition + offset;

                var size = _endPosition - _startPosition;
                size.x = Mathf.Abs(size.x);
                size.y = Mathf.Abs(size.y);
                size.z = Mathf.Abs(size.z);
                size += new Vector3Int(1, 0, 1);

                Handles.DrawWireCube(center, size);
            }
            else
            {
                Handles.DrawWireCube(TilePosition + offset, new Vector3(1.0f, 0.0f, 1.0f));
            }
        }
    }
}