//-----------------------------------------------------------------
// File:         Tile3D.cs
// Description:  Tile asset
// Module:       Til3map
// Author:       Noé Masse
// Date:         04/04/2021
//-----------------------------------------------------------------
using Til3map.Util;
using UnityEngine;

namespace Til3map
{
    [CreateAssetMenu(fileName = "New Tile3D", menuName = "Til3map/Tile", order = 1002)]
    public class Tile3D : ScriptableObject
    {
        [Header("Renderer")]
        [SerializeField] private Mesh _mesh = null;
        [SerializeField] private Material[] _materials = null;

        [Header("Settings")]
        [SerializeField] private bool _canBeRotated = true;
        [SerializeField] private TransformValue _transform = TransformValue.Default;

        public Mesh Mesh => _mesh;
        public Material[] Materials => _materials;
        public bool CanBeRotated => _canBeRotated;
        public Matrix4x4 TransformMatrix => _transform.ToMatrix4x4();


        private void OnValidate()
        {
            if (_mesh != null && _materials != null && _mesh.subMeshCount > _materials.Length)
            {
                _materials = new Material[_mesh.subMeshCount];
            }
        }

        public bool IsValid()
        {
            return _mesh != null &&
                   _materials != null &&
                   _materials.Length > 0;
        }

#if UNITY_EDITOR
        [Header("Editor Specific")]
        [SerializeField] private Vector3 _previewCameraPosition = new Vector3(0.0f, 10.0f, 0.0f);
        [SerializeField] private Vector3 _previewCameraRotation = new Vector3(90.0f, 0.0f, 0.0f);
        [SerializeField] private float _previewCameraSize = 1.0f;

        public Vector3 EditorOnlyPreviewCameraPosition => _previewCameraPosition;
        public Vector3 EditorOnlyPreviewCameraRotation => _previewCameraRotation;
        public float EditorOnlyPreviewCameraSize => _previewCameraSize;
#endif
    }
}