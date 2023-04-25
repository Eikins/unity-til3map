//-----------------------------------------------------------------
// File:         Tilemap3D.cs
// Description:  Tilemap script.
// Module:       Til3map
// Author:       Noé Masse
// Date:         24/04/2023
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Til3map
{
    public class Tilemap3D : MonoBehaviour
    {
        public const int MaxHeight = 64;
        public const int MinHeight = -64;

        [SerializeField] private RectInt _rect = new RectInt(0, 0, 32, 32);
        [SerializeField] private List<Tile3DInstances> _tiles = new List<Tile3DInstances>();

        public Action OnTilesChanged;

        public RectInt Rect => _rect;
        public List<Tile3DInstances> Tiles => _tiles;

        public void Clear()
        {
            Tiles.Clear();
        }

        public bool IsInBounds(TilePose tilePose)
        {
            return _rect.Contains(new Vector2Int(tilePose.position.x, tilePose.position.z));
        }

        public BoundsInt GetBoundsInt()
        {
            return new BoundsInt(_rect.xMin, MinHeight, _rect.yMin, _rect.width, MaxHeight - MinHeight, _rect.height);
        }

        public Bounds GetBounds()
        {
            Bounds bounds = new Bounds();
            bounds.SetMinMax(new Vector3(_rect.xMin, MinHeight, _rect.yMin), new Vector3(_rect.xMax, MaxHeight, _rect.yMax));
            return bounds;
        }

        private void OnDrawGizmos()
        {
            var rect = Rect;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);

            for (int x = rect.xMin; x <= rect.xMax; x++)
            {
                var p1 = new Vector3(x, 0.02f, rect.yMin);
                var p2 = new Vector3(x, 0.02f, rect.yMax);
                Gizmos.DrawLine(p1, p2);
            }

            for (int z = rect.yMin; z <= rect.yMax; z++)
            {
                var p1 = new Vector3(rect.xMin, 0.02f, z);
                var p2 = new Vector3(rect.xMax, 0.02f, z);
                Gizmos.DrawLine(p1, p2);
            }
        }

        private void OnDrawGizmosSelected()
        {
            var rect = Rect;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(rect.xMin, 0.03f, rect.yMin), new Vector3(rect.xMax, 0.03f, rect.yMin));
            Gizmos.DrawLine(new Vector3(rect.xMin, 0.03f, rect.yMin), new Vector3(rect.xMin, 0.03f, rect.yMax));
            Gizmos.DrawLine(new Vector3(rect.xMax, 0.03f, rect.yMax), new Vector3(rect.xMax, 0.03f, rect.yMin));
            Gizmos.DrawLine(new Vector3(rect.xMax, 0.03f, rect.yMax), new Vector3(rect.xMin, 0.03f, rect.yMax));
        }
    }
}

