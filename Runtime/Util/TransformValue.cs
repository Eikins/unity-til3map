//-----------------------------------------------------------------
// File:         TransformValue.cs
// Description:  Transform but as a struct. (Value type)
// Module:       Til3map.Util
// Author:       Noé Masse
// Date:         24/04/2023
//-----------------------------------------------------------------
using System;
using UnityEngine;

namespace Til3map.Util
{
    [Serializable]
    public struct TransformValue
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public static TransformValue Default => new TransformValue()
        {
            position = Vector3.zero,
            rotation = Quaternion.identity,
            scale = Vector3.one
        };

        public Matrix4x4 ToMatrix4x4() => Matrix4x4.TRS(position, rotation, scale);
    }
}