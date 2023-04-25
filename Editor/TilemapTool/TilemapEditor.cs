//-----------------------------------------------------------------
// File:         TilemapEditor.cs
// Description:  Internal class for tilemap editing
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         25/04/2024
//-----------------------------------------------------------------
using Til3map;
using UnityEngine;

namespace Til3mapEditor
{
    public class TilemapEditor
    {
        // Components
        private Tile3DPalette _Palette;
        private Tilemap3D _Tilemap3D;
        private Tilemap3DBuilder _tilemapBuilder;

        // Internal State
        private Vector2Int _tileIndex2D;
        private Tile3D _tile = null;
        private int _rotation = 0;
        private int _height = 0;
        private bool _isEraserEnabled = false;

        // Accessors
        public Tilemap3D Tilemap => _Tilemap3D;
        public Tilemap3DBuilder TilemapBuilder => _tilemapBuilder;

        public Tile3D Tile => _tile;
        public int Rotation => _tile != null && _tile.CanBeRotated ? _rotation : 0;
        public int Height => _height;
        public bool IsEraserEnabled => _isEraserEnabled;


        public void SetPalette(Tile3DPalette palette)
        {
            _Palette = palette;
            SyncTile();
        }

        public void SetTilemap(Tilemap3D tilemap)
        {
            _Tilemap3D = tilemap;
            _tilemapBuilder = tilemap != null ? new Tilemap3DBuilder(_Tilemap3D) : null;
        }

        public void RotateTile() { _rotation = (_rotation + 1) % 4; }
        public void IncreaseHeight() { _height++; }
        public void DecreaseHeight() { _height--; }
        public void ToggleEraser() { _isEraserEnabled = !_isEraserEnabled; }

        public void SetSelectedTilePosition(Vector2Int tilePosition)
        {
            _tileIndex2D = tilePosition;
            SyncTile();
        }

        private void SyncTile()
        {
            if (_Palette != null)
            {
                if (_Palette.Tiles.TryGetValue(_tileIndex2D, out var tile))
                {
                    _tile = tile;
                }
                else
                {
                    _tile = null;
                }
            }
        }
    }
}