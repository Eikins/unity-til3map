//-----------------------------------------------------------------
// File:         Tilemap3DEditor.cs
// Description:  Tilemap3D component editor.
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         24/04/2023
//-----------------------------------------------------------------
using Til3map;
using UnityEditor;
using UnityEngine;

namespace Til3mapEditor
{
    [CustomEditor(typeof(Tilemap3D))]
    public class Tilemap3DEditor : Editor
    {
        private SerializedProperty _rect;

        private void OnEnable()
        {
            _rect = serializedObject.FindProperty("_rect");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_rect);

            if (GUILayout.Button("Clear") && 
                EditorUtility.DisplayDialog("Clear Tilemap", "Do you really want to clear this tilemap ? This action can not be undo.", "Confirm", "Cancel"))
            {
                var tilemap = target as Tilemap3D;
                tilemap.Clear();
            }

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("GameObject/Til3map/Tilemap3D", false, 10)]
        private static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Tilemap 3D");
            go.AddComponent<Tilemap3D>();
            go.AddComponent<Tilemap3DRenderer>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
    }
}