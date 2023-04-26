//-----------------------------------------------------------------
// File:         TilemapEditorTool.cs
// Description:  Base class for tilemap tools.
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         25/04/2024
//-----------------------------------------------------------------
using Til3map;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Til3mapEditor
{
    public abstract class TilemapEditorTool
    {

        protected TilemapEditor Editor { get; private set; }
        protected Vector3Int TilePosition { get; private set; }
        protected bool IsPlaceable { get; private set; }
        protected TilePose TilePose => new TilePose() { position = TilePosition, rotation = Editor.Rotation };

        public TilemapEditorTool(TilemapEditor editor)
        {
            Editor = editor;
        }

        public abstract GUIContent toolbarIcon { get; }
        public abstract void DrawPreview(CommandBuffer cmd);
        public abstract void OnSceneGUI();

        public virtual void RefreshPreview() { }
        public virtual void CancelAction() { }
        public virtual void Dispose() { }

        public void PrepareSceneGUI()
        {
            if (RaycastTilePosition(out Vector3Int position))
            {
                TilePosition = position;
                IsPlaceable = Editor.TilemapBuilder.CanPlaceTile(Editor.Tile, TilePose);
            }
        }

        private bool RaycastTilePosition(out Vector3Int position)
        {
            var tilemap = Editor.Tilemap;
            var height = Editor.Height;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane plane = new Plane(tilemap.transform.up, tilemap.transform.position + tilemap.transform.up * (0.1f + height));

            position = new Vector3Int();

            if (plane.Raycast(ray, out float distance))
            {
                var worldSpacePosition = ray.origin + ray.direction * distance;
                var localPosition = tilemap.transform.worldToLocalMatrix.MultiplyPoint3x4(worldSpacePosition);

                position.x = Mathf.FloorToInt(localPosition.x);
                position.y = Mathf.FloorToInt(localPosition.y);
                position.z = Mathf.FloorToInt(localPosition.z);
                return true;
            }
            return false;
        }

        protected void PutOrRemoveTile(Tile3D tile, TilePose tilePose, bool erase)
        {
            if (tile == null) return;
            if (!Editor.Tilemap.IsInBounds(tilePose)) return;

            var tilemapBuilder = Editor.TilemapBuilder;
            var tilemap = Editor.Tilemap;

            tilemapBuilder.RemoveTiles(tile.GetBounds(tilePose));
            if (!erase)
            {
                tilemapBuilder.AddTile(tile, tilePose);
            }

            EditorUtility.SetDirty(tilemap);
        }
    }
}