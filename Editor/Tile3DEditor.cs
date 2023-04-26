//-----------------------------------------------------------------
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
using UnityEngine.Rendering;

namespace Til3mapEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Tile3D))]
    public class Tile3DEditor : Editor
    {
        private PreviewRenderUtility _previewRenderUtility = null;
        private List<string> _noGPUInstancingmaterialNames = new List<string>();
        private List<int> _nullMaterialIndices = new List<int>();
        private PreviewRenderUtility _inspectorPreview = null;

        public override void OnInspectorGUI()
        {
            var previewRect = EditorGUILayout.GetControlRect(false, 300);
            RenderTilePreview(previewRect);
            base.OnInspectorGUI();
            CheckMaterials();
        }

        private void RenderTilePreview(Rect rect)
        {
            if (_inspectorPreview == null)
            {
                _inspectorPreview = new PreviewRenderUtility();
            }

            var tile = target as Tile3D;

            // Setup Camera
            var camera = _inspectorPreview.camera;
            var focusPoint = (Vector3)tile.Size / 2.0f - new Vector3(0.5f, 0.0f, 0.5f);

            Quaternion lookRotation = Quaternion.Euler(tile.EditorOnlyPreviewCameraAngles);
            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = focusPoint - lookDirection * tile.Size.magnitude * 2.0f;
            camera.transform.SetPositionAndRotation(lookPosition, lookRotation);

            camera.orthographic = true;
            camera.orthographicSize = tile.EditorOnlyPreviewCameraSize / 2.0f;
            camera.farClipPlane = 30.0f;
            camera.nearClipPlane = 0.01f;
            camera.cameraType = CameraType.SceneView;

            _inspectorPreview.lights[0].transform.forward = camera.transform.forward;
            _inspectorPreview.BeginPreview(rect, GUIStyle.none);

            for (int i = 0; i < tile.Materials.Length; i++)
            {
                _inspectorPreview.DrawMesh(tile.Mesh, tile.TransformMatrix, tile.Materials[i], i);
            }

            _inspectorPreview.Render();

            Handles.SetCamera(camera);
            var zTest = Handles.zTest = CompareFunction.LessEqual;
            using (new Handles.DrawingScope(Color.white))
            {
                var gridSize = 5;
                var offset = new Vector3(0.5f, 0.0f, 0.5f);
                for (int x = -gridSize; x < gridSize; x++)
                {
                    var p1 = offset + new Vector3(x, 0.0f, -gridSize);
                    var p2 = offset + new Vector3(x, 0.0f, gridSize - 1);
                    Handles.DrawLine(p1, p2);
                }

                for (int z = -gridSize; z < gridSize; z++)
                {
                    var p1 = offset + new Vector3(-gridSize, 0.0f, z);
                    var p2 = offset + new Vector3(gridSize - 1, 0.0f, z);
                    Handles.DrawLine(p1, p2);
                }
            }

            using (new Handles.DrawingScope(Color.green))
            {
                Handles.DrawWireCube(focusPoint, tile.Size);
            }
            Handles.zTest = zTest;

            _inspectorPreview.EndAndDrawPreview(rect);
        }

        private void CheckMaterials()
        {
            _noGPUInstancingmaterialNames.Clear();
            _nullMaterialIndices.Clear();

            var tile = target as Tile3D;
            for (int i = 0; i < tile.Materials.Length; i++)
            {
                var material = tile.Materials[i];
                if (material == null)
                {
                    _nullMaterialIndices.Add(i);
                }
                else if (!material.enableInstancing)
                {
                    _noGPUInstancingmaterialNames.Add(material.name);
                }
            }

            if (_nullMaterialIndices.Count > 0)
            {
                EditorGUILayout.HelpBox($"Material(s): [{string.Join(", ", _nullMaterialIndices)}] are not set. Tile will not be valid.", MessageType.Error);

            }
            if (_noGPUInstancingmaterialNames.Count > 0)
            {
                EditorGUILayout.HelpBox($"Material(s): [{string.Join(", ", _noGPUInstancingmaterialNames)}] should have GPU Instancing Enabled. Otherwise, this will cause huge performance issues.", MessageType.Warning);
            }
        }

        private void OnDisable()
        {
            _inspectorPreview?.Cleanup();
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            Tile3D tile = target as Tile3D;

            if (tile == null || !tile.IsValid())
                return null;

            var rect = new Rect(0, 0, width, height);

            _previewRenderUtility = new PreviewRenderUtility();
            var focusPoint = (Vector3) tile.Size / 2.0f - new Vector3(0.5f, 0.0f, 0.5f);
            // Setup Camera
            Quaternion lookRotation = Quaternion.Euler(tile.EditorOnlyPreviewCameraAngles);
            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = focusPoint - lookDirection * tile.Size.magnitude * 2.0f;
            _previewRenderUtility.camera.transform.SetPositionAndRotation(lookPosition, lookRotation);

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