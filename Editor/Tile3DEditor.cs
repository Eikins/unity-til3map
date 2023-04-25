﻿//-----------------------------------------------------------------
// File:         Tile3DEditor.cs
// Description:  Tile asset editor.
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         24/04/2023
//-----------------------------------------------------------------
using System.Collections.Generic;
using Til3map;
using UnityEditor;
using UnityEngine;

namespace Til3mapEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Tile3D))]
    public class Tile3DEditor : Editor
    {
        private PreviewRenderUtility _previewRenderUtility = null;
        private List<string> _materialNames = new List<string>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _materialNames.Clear();

            var tile = target as Tile3D;
            foreach (var material in tile.Materials)
            {
                if (!material.enableInstancing)
                {
                    _materialNames.Add(material.name);
                }
            }

            if (_materialNames.Count > 0)
            {
                EditorGUILayout.HelpBox($"Material(s): [{string.Join(", ", _materialNames)}] should have GPU Instancing Enabled. Otherwise, this will cause huge performance issues.", MessageType.Warning);
            }
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            Tile3D tile = target as Tile3D;

            if (tile == null || !tile.IsValid())
                return null;

            var rect = new Rect(0, 0, width, height);

            _previewRenderUtility = new PreviewRenderUtility();

            // Setup Camera
            _previewRenderUtility.camera.transform.position = tile.EditorOnlyPreviewCameraPosition;
            _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(tile.EditorOnlyPreviewCameraRotation);
            _previewRenderUtility.camera.orthographic = true;
            _previewRenderUtility.camera.orthographicSize = tile.EditorOnlyPreviewCameraSize / 2.0f;
            _previewRenderUtility.camera.farClipPlane = 30.0f;
            _previewRenderUtility.camera.nearClipPlane = 0.01f;

            _previewRenderUtility.camera.backgroundColor = Color.cyan;

            _previewRenderUtility.lights[0].transform.forward = _previewRenderUtility.camera.transform.forward;

            // Draw Tile
            _previewRenderUtility.BeginStaticPreview(rect);
            for (int i = 0; i < tile.Materials.Length; i++)
            {
                _previewRenderUtility.DrawMesh(tile.Mesh, tile.TransformMatrix, tile.Materials[i], i);
            }

            _previewRenderUtility.Render();

            var tex = _previewRenderUtility.EndStaticPreview();

            _previewRenderUtility.Cleanup();
            _previewRenderUtility = null;

            return tex;
        }
    }
}