//-----------------------------------------------------------------
// File:         Tilemap3DConvert.cs
// Description:  Tilemap3D component editor.
// Module:       Til3mapEditor
// Author:       Noé Masse
// Date:         24/04/2023
//-----------------------------------------------------------------
using System.Collections.Generic;
using Til3map;
using UnityEditor;
using UnityEngine;

namespace Til3mapEditor
{
    public static class Tilemap3DConvert
    {

        public static Mesh ToMesh(Tilemap3D tilemap)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"{tilemap.name}_mesh";

            var combineInstances = new List<CombineInstance>();
            int totalVertices = 0;
            foreach(var instances in tilemap.Tiles)
            {
                var tile = instances.tile;
                var tileMesh = tile.Mesh;

                foreach(var pose in instances.poses)
                {
                    for (int i = 0; i < tileMesh.subMeshCount; i++)
                    {
                        combineInstances.Add(new CombineInstance()
                        {
                            mesh = tileMesh,
                            subMeshIndex = i,
                            transform = pose.ToMatrix4x4() * tile.TransformMatrix
                        });
                        totalVertices += tileMesh.GetSubMesh(i).vertexCount;
                    }
                }
            }

            if (totalVertices > ushort.MaxValue)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.CombineMeshes(combineInstances.ToArray());
            return mesh;
        }

        [MenuItem("Edit/Til3map/Convert selected tilemaps to meshes.", priority = 51)]
        private static void ConvertTilemapCommand()
        {
            var parentGo = new GameObject("Tilemap Meshes");
            foreach (var go in Selection.gameObjects)
            {
                var tilemap = go.GetComponent<Tilemap3D>();
                if (tilemap)
                {
                    GameObject meshGO = new GameObject($"{go.name} (Mesh)");
                    GameObjectUtility.SetParentAndAlign(meshGO, parentGo);

                    var filter = meshGO.AddComponent<MeshFilter>();
                    filter.sharedMesh = ToMesh(tilemap);
                    var renderer = meshGO.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");

                    Undo.RegisterCreatedObjectUndo(meshGO, "Create " + meshGO.name);
                    Selection.activeObject = meshGO;
                }
            }
        }
    }
}