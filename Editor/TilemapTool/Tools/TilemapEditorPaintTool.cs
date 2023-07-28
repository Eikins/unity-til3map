//-----------------------------------------------------------------
// File:         TilemapEditorPaintTool.cs
// Description:  Tile paint tool.
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         25/04/2024
//-----------------------------------------------------------------
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Til3map;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Til3mapEditor
{
    public class TilemapEditorPaintTool : TilemapEditorTool
    {
        private GUIContent _iconContent;
        public override GUIContent toolbarIcon => _iconContent;
        private List<TilePose> _poseBuffer;
        private InstanceBatcher<Matrix4x4> _previewInstances;

        public TilemapEditorPaintTool(TilemapEditor editor) : base(editor)
        {
            _iconContent = EditorGUIUtility.IconContent("Grid.PaintTool");
            _iconContent.tooltip = "Paint Tool";
            _poseBuffer = new List<TilePose>(64);

            _previewInstances = new InstanceBatcher<Matrix4x4>(null, false);
            UpdatePreviewInstances();
        }

        private void UpdatePreviewInstances()
        {
            _previewInstances.SetInstances(_poseBuffer.Select((pose) => Editor.Tilemap.transform.localToWorldMatrix * pose.ToMatrix4x4() * Editor.Tile.TransformMatrix).ToList());
        }

        public override void DrawPreview(CommandBuffer cmd)
        {
            if (Editor.IsEraserEnabled || Editor.Tile == null) return;

            var tile = Editor.Tile;
            if (_previewInstances.Instances.Count > 0)
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
            var tilemap = Editor.Tilemap;
            Tile3D tile = Editor.Tile;

            if (tilemap == null || tile == null) return;
            if (!Editor.Tilemap.IsInBounds(TilePose)) return;

            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            switch (Event.current.type)
            {
                case EventType.Layout:
                    HandleUtility.AddDefaultControl(controlID);
                    break;
                case EventType.MouseDrag:
                    if (Event.current.button == 0)
                    {
                        _poseBuffer.Add(TilePose);
                        UpdatePreviewInstances();
                        Event.current.Use();
                    }
                    break;
                case EventType.MouseDown:
                    if (Event.current.button == 0)
                    {
                        _poseBuffer.Add(TilePose);
                        UpdatePreviewInstances();
                        Event.current.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (Event.current.button == 0)
                    {
                        PutOrRemoveTiles(tile, _poseBuffer, Editor.IsEraserEnabled);
                        _poseBuffer.Clear();
                        UpdatePreviewInstances();
                        Event.current.Use();
                    }
                    break;
            }

            var handleMatrix = Handles.matrix;
            Handles.matrix = tilemap.transform.localToWorldMatrix;
            DrawHandle();
            Handles.matrix = handleMatrix;
            SceneView.currentDrawingSceneView.Repaint();
        }

        private void DrawHandle()
        {
            if (_poseBuffer.Count == 0)
            {
                Handles.color = Editor.IsEraserEnabled ? Color.red : IsPlaceable ? Color.white : Color.yellow;
                var bounds = Editor.Tile.GetBounds(TilePose);
                Handles.DrawWireCube(bounds.center, bounds.size);
            }
            else
            {
                foreach (var pose in _poseBuffer)
                {
                    var isPlaceable = Editor.TilemapBuilder.CanPlaceTile(Editor.Tile, pose);
                    Handles.color = Editor.IsEraserEnabled ? Color.red : isPlaceable ? Color.white : Color.yellow;
                    
                    var bounds = Editor.Tile.GetBounds(pose);
                    Handles.DrawWireCube(bounds.center, bounds.size);
                }
            }
        }

    }
}