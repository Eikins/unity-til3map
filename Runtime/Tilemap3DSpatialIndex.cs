//-----------------------------------------------------------------
// File:         Tilemap3DSpatialIndex.cs
// Description:  Spatial index for retrieving tiles at positions.
// Module:       Til3map
// Author:       Noé Masse
// Date:         26/04/2023
//-----------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace Til3map
{
    public class Tilemap3DSpatialIndex
    {
        public class Node
        {
            public Tile3D tile;
            public TilePose pose;

            public Node(Tile3D tile, TilePose pose)
            {
                this.tile = tile;
                this.pose = pose;
            }

            public BoundsInt GetBounds() => tile.GetBounds(pose);
        }

        private const int DefaultCapacity = 1024;
        private Dictionary<Vector3Int, Node> _nodes;

        public Tilemap3DSpatialIndex(Tilemap3D tilemap)
        {
            _nodes = new Dictionary<Vector3Int, Node>(DefaultCapacity);
            InitFromTilemap(tilemap);
        }

        private void InitFromTilemap(Tilemap3D tilemap)
        {
            foreach (var instances in tilemap.Tiles)
            {
                var tile = instances.tile;
                foreach (var pose in instances.poses)
                {
                    AddTileAt(tile, pose);
                }
            }
        }

        public bool HasTileAt(Vector3Int position)
        {
            return _nodes.ContainsKey(position);
        }

        public bool TryGetTileAt(Vector3Int position, out Node node)
        {
            return _nodes.TryGetValue(position, out node);
        }

        public bool AddTileAt(Tile3D tile, TilePose pose)
        {
            if (tile == null) return false;

            var bounds = tile.GetBounds(pose);
            if (!HasTileWithin(bounds)) return false;

            foreach (var position in bounds.allPositionsWithin)
            {
                _nodes.Add(position, new Node(tile, pose));
            }

            return true;
        }

        public bool HasTileWithin(BoundsInt bounds)
        {
            foreach (var position in bounds.allPositionsWithin)
            {
                if (_nodes.ContainsKey(position))
                {
                    return false;
                }
            }
            return true;
        }

        public void GetTilesWithin(List<Node> result, BoundsInt bounds)
        {
            result.Clear();
            foreach (var position in bounds.allPositionsWithin)
            {
                if (_nodes.TryGetValue(position, out var node))
                {
                    // List is sufficient here has tiles usually have small bounds.
                    if (!result.Contains(node))
                    {
                        result.Add(node);
                    }
                }
            }
        }

        public void Remove(Node node)
        {
            var bounds = node.GetBounds();
            foreach (var pos in bounds.allPositionsWithin)
            {
                _nodes.Remove(pos);
            }
        }

        public Node RemoveTileAt(Vector3Int position)
        {
            if (_nodes.TryGetValue(position, out var node))
            {
                Remove(node);
                return node;
            }
            return null;
        }
    }
}

