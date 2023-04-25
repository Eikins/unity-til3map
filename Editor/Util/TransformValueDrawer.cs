//-----------------------------------------------------------------
// File:         TransformValueDrawer.cs
// Description:  Property Drawer for TransformValue
// Module:       Til3map.Util
// Author:       Noé Masse
// Date:         04/04/2024
//-----------------------------------------------------------------
using Til3map.Util;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MonstersEditor.Util
{
    [CustomPropertyDrawer(typeof(TransformValue))]
    public class TransformValueDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var positionField = new PropertyField(property.FindPropertyRelative("position"));
            var rotationField = new PropertyField(property.FindPropertyRelative("rotation"));
            var scaleField = new PropertyField(property.FindPropertyRelative("scale"));

            container.Add(positionField);
            container.Add(rotationField);
            container.Add(scaleField);

            return container;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var labelContent = EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var positionRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            var rotationRect = new Rect(position.x, position.y + 2 * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            var scaleRect = new Rect(position.x, position.y + 3 * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(labelRect, labelContent, EditorStyles.boldLabel);
            EditorGUI.PropertyField(positionRect, property.FindPropertyRelative("position"));
            EditorGUI.PropertyField(rotationRect, property.FindPropertyRelative("rotation"));
            EditorGUI.PropertyField(scaleRect, property.FindPropertyRelative("scale"));

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 4 * EditorGUIUtility.singleLineHeight;
        }
    }
}