//-----------------------------------------------------------------
// File:         Tilemap3DConvert.cs
// Description:  Tilemap3D component editor.
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         24/04/2023
//-----------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Til3map;
using UnityEditor;
using UnityEngine;

namespace Til3mapEditor
{
    public class Tilemap3DConverter
    {

        #region Menu Commands
        [MenuItem("Edit/Til3map/Convert selected tilemaps to MeshRenderer.", priority = 3001)]
        private static void ConvertTilesToMeshRendererCommand()
        {
            var mergedTilemapTiles = new List<Tile3DInstances>();
            foreach (var go in Selection.gameObjects)
            {
                var tilemap = go.GetComponent<Tilemap3D>();
                if (tilemap)
                {
                    mergedTilemapTiles.AddRange(tilemap.Tiles);
                }
            }

            if (mergedTilemapTiles.Count > 0)
            {
                GameObject meshRendererGameObject = new GameObject($"Tilemap Mesh");

                var converter = new Tilemap3DConverter();
                meshRendererGameObject.AddComponent<MeshFilter>();
                var renderer = meshRendererGameObject.AddComponent<MeshRenderer>();
                converter.ConvertTilesToMeshRenderer(renderer, mergedTilemapTiles);
                Undo.RegisterCreatedObjectUndo(meshRendererGameObject, "Create " + meshRendererGameObject.name);
                Selection.activeObject = meshRendererGameObject;
            }
        }

        [MenuItem("Edit/Til3map/Convert selected tilemaps to MeshCollider.", priority = 3002)]
        private static void ConvertTilesToMeshColliderCommand()
        {
            var mergedTilemapTiles = new List<Tile3DInstances>();
            foreach (var go in Selection.gameObjects)
            {
                var tilemap = go.GetComponent<Tilemap3D>();
                if (tilemap)
                {
                    mergedTilemapTiles.AddRange(tilemap.Tiles);
                }
            }

            if (mergedTilemapTiles.Count > 0)
            {
                GameObject meshColliderGameObject = new GameObject($"Tilemap Collider");

                var converter = new Tilemap3DConverter();
                var collider = meshColliderGameObject.AddComponent<MeshCollider>();
                converter.ConvertTilesToMeshCollider(collider, mergedTilemapTiles);
                Undo.RegisterCreatedObjectUndo(meshColliderGameObject, "Create " + meshColliderGameObject.name);
                Selection.activeObject = meshColliderGameObject;
            }
        }
        #endregion

        private class MeshCombineInstance
        {
            public Material material;
            public Mesh mesh;
            public List<CombineInstance> instances;
            public int totalVertexCount;

            public MeshCombineInstance(Material material)
            {
                this.material = material;
                mesh = new Mesh();
                instances = new List<CombineInstance>();
                totalVertexCount = 0;
            }

            public void Build()
            {
                if (totalVertexCount > ushort.MaxValue)
                {
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                }
                mesh.CombineMeshes(instances.ToArray(), true, true);
            }
        }

        public void ConvertTilesToMeshCollider(MeshCollider meshCollider, List<Tile3DInstances> tiles)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"{meshCollider.gameObject.name}_collidermesh";

            var combineInstances = new List<CombineInstance>();

            int totalVertexCount = 0;
            foreach (var instances in tiles)
            {
                var tile = instances.tile;
                var tileMesh = tile.Mesh;

                foreach (var pose in instances.poses)
                {
                    for (int i = 0; i < tileMesh.subMeshCount; i++)
                    {
                        combineInstances.Add(new CombineInstance()
                        {
                            mesh = tileMesh,
                            subMeshIndex = i,
                            transform = pose.ToMatrix4x4()
                        });
                        totalVertexCount += tileMesh.GetSubMesh(i).vertexCount;
                    }
                }
            }

            if (totalVertexCount > ushort.MaxValue)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.CombineMeshes(combineInstances.ToArray(), true, true);

            meshCollider.sharedMesh = mesh;
        }

        public void ConvertTilesToMeshRenderer(MeshRenderer renderer, List<Tile3DInstances> tiles)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"{renderer.gameObject.name}_mesh";

            var meshCombineInstances = new List<MeshCombineInstance>();

            foreach (var instances in tiles)
            {
                var tile = instances.tile;
                var tileMesh = tile.Mesh;

                foreach (var pose in instances.poses)
                {
                    for (int i = 0; i < tileMesh.subMeshCount; i++)
                    {
                        int combineInstanceIndex = meshCombineInstances.FindIndex((instance) => instance.material == tile.Materials[i]);
                        if (combineInstanceIndex == -1)
                        {
                            meshCombineInstances.Add(new MeshCombineInstance(tile.Materials[i]));
                            combineInstanceIndex = meshCombineInstances.Count - 1;
                        }

                        var meshCombineInstance = meshCombineInstances[combineInstanceIndex];
                        meshCombineInstance.instances.Add(new CombineInstance()
                        {
                            mesh = tileMesh,
                            subMeshIndex = i,
                            transform = pose.ToMatrix4x4() * tile.TransformMatrix
                        });
                        meshCombineInstance.totalVertexCount += tileMesh.GetSubMesh(i).vertexCount;
                    }
                }
            }

            int totalVertexCount = 0;
            var combineInstances = new List<CombineInstance>(meshCombineInstances.Count);
            foreach (var meshCombineInstance in meshCombineInstances)
            {
                meshCombineInstance.Build();
                totalVertexCount += meshCombineInstance.totalVertexCount;
                combineInstances.Add(new CombineInstance()
                {
                    mesh = meshCombineInstance.mesh
                });
            }

            if (totalVertexCount > ushort.MaxValue)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.CombineMeshes(combineInstances.ToArray(), false, false);

            renderer.GetComponent<MeshFilter>().sharedMesh = mesh;
            renderer.sharedMaterials = meshCombineInstances.Select((instance) => instance.material).ToArray();
        }
    }
}