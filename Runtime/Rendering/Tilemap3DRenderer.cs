//-----------------------------------------------------------------
// File:         Tilemap3DRenderer.cs
// Description:  Tilemap Renderer
// Module:       Til3map
// Author:       Noé Masse
// Date:         24/04/2023
//-----------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Til3map
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Tilemap3D))]
    public class Tilemap3DRenderer : MonoBehaviour
    {
        private Tilemap3D _Tilemap3D = null;
        private List<Tile3DRenderData> _renderDataList;
        private bool _isDirty = true;

        private void OnEnable()
        {
            _Tilemap3D = GetComponent<Tilemap3D>();
            _Tilemap3D.OnTilesChanged += SetDirty;
            _isDirty = true;
        }

        private void Update()
        {
            if (_isDirty || transform.hasChanged)
            {
                BuildTileRenderLists();
            }

            foreach (var renderData in _renderDataList)
            {
                for (int materialIndex = 0; materialIndex < renderData.materials.Length; materialIndex++)
                {
                    var material = renderData.materials[materialIndex];
                    if (material.enableInstancing)
                    {
                        foreach (var batch in renderData.instanceBatcher.Batches)
                        {
                            Graphics.DrawMeshInstanced(renderData.mesh, materialIndex, material, batch);
                        }
                    }
                    else
                    {
                        var matrices = renderData.instanceBatcher.Instances;
                        for (int i = 0; i < matrices.Count; i++)
                        {
                            Graphics.DrawMesh(renderData.mesh, matrices[i], material, gameObject.layer, null, materialIndex);
                        }
                    }
                }
            }
        }

        private void OnDisable()
        {
            ClearTileRenderDataLists();
        }

        private void OnDestroy()
        {
            ClearTileRenderDataLists();
        }

        public void SetDirty()
        {
            _isDirty = true;
        }

        private void ClearTileRenderDataLists()
        {
            if (_renderDataList != null)
            {
                _renderDataList.Clear();
            }
        }

        private void BuildTileRenderLists()
        {
            _isDirty = false;
            var tileList = _Tilemap3D.Tiles;
            if (tileList == null) return;

            if (_renderDataList == null)
            {
                _renderDataList = new List<Tile3DRenderData>(tileList.Count);
            }
            else
            {
                ClearTileRenderDataLists();
            }

            foreach (var instances in tileList)
            {
                Tile3D tile = instances.tile;
                List<TilePose> poses = instances.poses;
                Matrix4x4 tileMatrix4x4 = tile.TransformMatrix;

                if (poses.Count == 0) continue;
                if (tile.Materials == null || tile.Mesh == null) continue;

                List<Matrix4x4> matrices = new List<Matrix4x4>(poses.Count);
                foreach (var pose in poses)
                {
                    matrices.Add(transform.localToWorldMatrix * pose.ToMatrix4x4() * tileMatrix4x4);
                }

                Tile3DRenderData renderData = new Tile3DRenderData()
                {
                    materials = tile.Materials, 
                    mesh = tile.Mesh,
                    instanceBatcher = new InstanceBatcher<Matrix4x4>(matrices, true)
                };

                _renderDataList.Add(renderData);
            }
        }
    }
}