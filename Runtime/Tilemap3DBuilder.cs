//-----------------------------------------------------------------
// File:         Tilemap3DBuilder.cs
// Description:  Used to edit a tilemap content.
// Module:       Til3map
// Author:       Noé Masse
// Date:         24/04/2023
//-----------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace Til3map
{
    public class Tilemap3DBuilder
    {

        private Tilemap3D _tilemap;
        private Tilemap3DSpatialIndex _spatialIndex;
        private List<Tile3DInstances> _tiles;

        public Tilemap3DBuilder(Tilemap3D tilemap)
        {
            _tilemap = tilemap;
            _tiles = tilemap.Tiles;
            _spatialIndex = new Tilemap3DSpatialIndex(tilemap);
        }

        public bool CanPlaceTile(Tile3D tile, TilePose pose)
        {
            if (tile == null) return false;
            return _spatialIndex.HasTileWithin(tile.GetBounds(pose));
        }

        public bool AddTile(Tile3D tile, TilePose pose)
        {
            if (tile == null) return false;

            if (!_tilemap.IsInBounds(pose) || !_spatialIndex.AddTileAt(tile, pose))
            {
                return false;
            }

            if (!tile.CanBeRotated && pose.rotation != 0)
            {
                Debug.LogWarning("A tile was added with a rotation beside being marked as CanBeRotated = false.");
            }

            int index = _tiles.FindIndex((instance) => instance.tile == tile);
            if (index == -1)
            {
                var instance = new Tile3DInstances();
                instance.tile = tile;
                instance.poses = new List<TilePose>() { pose };
                _tiles.Add(instance);
            }
            else
            {
                _tiles[index].poses.Add(pose);
            }

            _tilemap.OnTilesChanged?.Invoke();

            return true;
        }

        public bool RemoveTiles(BoundsInt bounds)
        {
            var nodes = new List<Tilemap3DSpatialIndex.Node>();
            _spatialIndex.GetTilesWithin(nodes, bounds);
            if (nodes.Count == 0)
            {
                return false;
            }

            foreach (var node in nodes)
            {
                int index = _tiles.FindIndex((instance) => instance.tile == node.tile);

                if (index != -1)
                {
                    _tiles[index].poses.RemoveAll((pose) => pose.position == node.pose.position);

                    if (_tiles[index].poses.Count == 0)
                    {
                        _tiles.RemoveAt(index);
                    }
                }

                _spatialIndex.Remove(node);
            }

            _tilemap.OnTilesChanged?.Invoke();
            return true;
        }

        public bool RemoveTile(Vector3Int position)
        {
            var node = _spatialIndex.RemoveTileAt(position);
            if (node != null)
            {
                int index = _tiles.FindIndex((instance) => instance.tile == node.tile);

                if (index != -1)
                {
                    _tiles[index].poses.RemoveAll((pose) => pose.position == node.pose.position);

                    if (_tiles[index].poses.Count == 0)
                    {
                        _tiles.RemoveAt(index);
                    }

                    _tilemap.OnTilesChanged?.Invoke();
                }
                return true;
            }

            return false;
        }
    }
}

