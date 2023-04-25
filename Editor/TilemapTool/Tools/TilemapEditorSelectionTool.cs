//-----------------------------------------------------------------
// File:         Tilemap3DSelectionTool.cs
// Description:  Selection tool for tilemap editor.
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         25/04/2024
//-----------------------------------------------------------------
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Til3mapEditor
{
    public class TilemapEditorSelectionTool : TilemapEditorTool
    {
        private GUIContent m_IconContent;
        public override GUIContent toolbarIcon => m_IconContent;

        public TilemapEditorSelectionTool(TilemapEditor editor) : base(editor)
        {
            m_IconContent = EditorGUIUtility.IconContent("Grid.Default");
            m_IconContent.tooltip = "Default Tool";
        }

        public override void DrawPreview(CommandBuffer cmd)
        {
        }

        public override void OnSceneGUI()
        {
        }

        private void DrawHandle(Vector3Int position)
        {
            Handles.color = Color.white * 0.3f;
            for (int x = -1; x < 3; x++)
            {
                var p1 = position + new Vector3Int(x, 0, -1);
                var p2 = position + new Vector3Int(x, 0, 2);
                Handles.DrawLine(p1, p2);
            }

            for (int z = -1; z < 3; z++)
            {
                var p1 = position + new Vector3Int(-1, 0, z);
                var p2 = position + new Vector3Int(2, 0, z);
                Handles.DrawLine(p1, p2);
            }

            Handles.color = Color.white;
            Handles.DrawWireCube(position + new Vector3(0.5f, 0.0f, 0.5f), new Vector3(1.0f, 0.0f, 1.0f));
        }
    }
}