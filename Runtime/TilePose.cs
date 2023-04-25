//-----------------------------------------------------------------
// File:         TilePose.cs
// Description:  Describe the position and the rotation of a Tile.
// Module:       Til3map
// Author:       Noé Masse
// Date:         24/04/2023
//-----------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Til3map
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct TilePose
    {
        public const int ByteSize = 3 * sizeof(int) + sizeof(int);
        
        public Vector3Int position;
        public int rotation;

        public Matrix4x4 ToMatrix4x4() => Matrix4x4.TRS(position + new Vector3(0.5f, 0.0f, 0.5f), Quaternion.AngleAxis(90f * rotation, Vector3.up), Vector3.one);
    }
}