//-----------------------------------------------------------------
// File:         TilemapEditorPaintTool.cs
// Description:  Tile paint tool.
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         25/04/2024
//-----------------------------------------------------------------
using Til3map;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Til3mapEditor
{
    public class TilemapEditorPaintTool : TilemapEditorTool
    {
        private GUIContent m_IconContent;
        public override GUIContent toolbarIcon => m_IconContent;

        public TilemapEditorPaintTool(TilemapEditor editor) : base(editor)
        {
            m_IconContent = EditorGUIUtility.IconContent("Grid.PaintTool");
            m_IconContent.tooltip = "Paint Tool";
        }

        public override void DrawPreview(CommandBuffer cmd)
        {
            if (Editor.IsEraserEnabled) return;
            if (!Editor.Tilemap.IsInBounds(TilePose)) return;

            var tile = Editor.Tile;
            if (tile == null || !tile.IsValid()) return;

            for (int i = 0; i < tile.Materials.Length; i++)
            {
                cmd.DrawMesh(tile.Mesh, GetTilePreviewMatrix(), tile.Materials[i], i);
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
                        PutOrRemoveTile(tile, TilePose, Editor.IsEraserEnabled);
                        Event.current.Use();
                    }
                    break;
                case EventType.MouseDown:
                    if (Event.current.button == 0)
                    {
                        PutOrRemoveTile(tile, TilePose, Editor.IsEraserEnabled);
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

        private Matrix4x4 GetTilePreviewMatrix()
        {           
            return Editor.Tilemap.transform.localToWorldMatrix * TilePose.ToMatrix4x4() * Editor.Tile.TransformMatrix;
        }

        private void DrawHandle()
        {
            Handles.color = Editor.IsEraserEnabled ? Color.red : IsPlaceable ? Color.white : Color.yellow;
            var bounds = Editor.Tile.GetBounds(TilePose);
            Handles.DrawWireCube(bounds.center, bounds.size);
        }

    }
}