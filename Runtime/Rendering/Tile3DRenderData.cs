//-----------------------------------------------------------------
// File:         Tile3DRenderData.cs
// Description:  Contains data needed for tile rendering
// Module:       Til3map
// Author:       Noé Masse
// Date:         24/04/2023
//-----------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace Til3map
{
    public struct Tile3DRenderData
    {
        public Mesh mesh;
        public Material[] materials;
        public InstanceBatcher<Matrix4x4> instanceBatcher;
    }
}