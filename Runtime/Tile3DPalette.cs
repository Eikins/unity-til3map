//-----------------------------------------------------------------
// File:         Tile3DPalette.cs
// Description:  List of tile assets used when painting.
// Module:       Til3map
// Author:       Noé Masse
// Date:         24/04/2023
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Til3map
{
    [CreateAssetMenu(fileName = "New Palette", menuName = "Til3map/Palette", order = 1001)]
    public class Tile3DPalette : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<Tile3D> _tiles = new List<Tile3D>();
        [SerializeField] private List<Vector2Int> _positions = new List<Vector2Int>();

        private Dictionary<Vector2Int, Tile3D> _tileDictionary = new Dictionary<Vector2Int, Tile3D>();

        public Dictionary<Vector2Int, Tile3D> Tiles => _tileDictionary;

        private Vector2Int NextAvailablePosition
        {
            get
            {
                if (_positions.Count == 0) return Vector2Int.zero;
                var lastPos = _positions[_positions.Count - 1];
                lastPos.x++;
                return lastPos;
            }
        }

        public Tile3D this[int index]
        {
            get
            {
                return _tiles[index];
            }
            set
            {
                _tiles[index] = value;
            }
        }

        public int Count => _tiles.Count;

        public bool TryAddTile(Tile3D tile)
        {
            if (tile == null || !tile.IsValid() || _tileDictionary.ContainsValue(tile))
            {
                return false;
            }

            _tileDictionary.Add(NextAvailablePosition, tile);
            return true;
        }

        public List<Tile3D> Filter(Func<Tile3D, bool> predicate)
        {
            var result = new List<Tile3D>(_tiles.Count / 4);
            
            foreach (var tile in _tileDictionary.Values)
            {
                if (predicate(tile)) result.Add(tile);
            }

            return result;
        }

        public void OnBeforeSerialize()
        {
            _tiles.Clear();
            _positions.Clear();
            _tiles.Capacity = _tileDictionary.Count;
            _positions.Capacity = _tileDictionary.Count;

            foreach (var tileEntry in _tileDictionary)
            {
                _tiles.Add(tileEntry.Value);
                _positions.Add(tileEntry.Key);
            }
        }

        public void OnAfterDeserialize()
        {
            _tileDictionary.Clear();
            _tileDictionary.EnsureCapacity(_tiles.Count);

            for (int i = 0; i < _tiles.Count; i++)
            {
                _tileDictionary.Add(_positions[i], _tiles[i]);
            }
        }
    }
}