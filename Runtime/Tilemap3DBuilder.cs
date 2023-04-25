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
        private Dictionary<Vector3Int, Tile3D> _positions;
        private List<Tile3DInstances> _tiles;

        public Tilemap3DBuilder(Tilemap3D tilemap)
        {
            _tilemap = tilemap;
            _tiles = tilemap.Tiles;
            _positions = new Dictionary<Vector3Int, Tile3D>();
            ReadPositionsFromTilemap();
        }

        private void ReadPositionsFromTilemap()
        {
            foreach (var instances in _tiles)
            {
                for (int j = 0; j < instances.poses.Count; j++)
                {
                    _positions.Add(instances.poses[j].position, instances.tile);
                }
            }
        }

        public bool HasTile(Vector3Int position)
        {
            return _positions.ContainsKey(position);
        }

        public bool AddTile(Tile3D tile, TilePose pose)
        {
            if (tile == null) return false;

            if (!_tilemap.IsInBounds(pose) || HasTile(pose.position))
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

            _positions.Add(pose.position, tile);
            _tilemap.OnTilesChanged?.Invoke();

            return true;
        }

        public bool RemoveTile(Vector3Int position)
        {
            if (_positions.TryGetValue(position, out var tile))
            {
                int index = _tiles.FindIndex((instance) => instance.tile == tile);

                if (index != -1)
                {
                    _tiles[index].poses.RemoveAll((pose) => pose.position == position);

                    if (_tiles[index].poses.Count == 0)
                    {
                        _tiles.RemoveAt(index);
                    }

                    _positions.Remove(position);
                    _tilemap.OnTilesChanged?.Invoke();
                }

                return true;
            }

            return false;
        }
    }
}

