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
    }
}