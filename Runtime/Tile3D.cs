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
        [SerializeField] private Vector3Int _size = Vector3Int.one;
        [SerializeField] private bool _canBeRotated = true;
        [SerializeField] private TransformValue _transform = TransformValue.Default;

        public Mesh Mesh => _mesh;
        public Material[] Materials => _materials;
        public Vector3Int Size => _size;
        public bool CanBeRotated => _canBeRotated;
        public Matrix4x4 TransformMatrix => _transform.ToMatrix4x4();

        public BoundsInt GetBounds(TilePose pose)
        {
            // Todo : Take in account the rotation.
            return new BoundsInt(pose.position, _size);
        }

        private void OnValidate()
        {
            if (_mesh != null && _materials != null && _mesh.subMeshCount > _materials.Length)
            {
                _materials = new Material[_mesh.subMeshCount];
            }

            _size.x = Mathf.Max(_size.x, 1);
            _size.y = Mathf.Max(_size.y, 1);
            _size.z = Mathf.Max(_size.z, 1);
        }

        public bool IsValid()
        {
            return _mesh != null &&
                   _materials != null &&
                   _materials.Length > 0;
        }

#if UNITY_EDITOR
        [Header("Editor Specific")]
        [SerializeField] private Vector2 _previewCameraAngles = new Vector2(45.0f, 45.0f);
        [SerializeField] private float _previewCameraSize = 2.0f;

        public Vector2 EditorOnlyPreviewCameraAngles => _previewCameraAngles;
        public float EditorOnlyPreviewCameraSize => _previewCameraSize;
#endif
    }
}