//-----------------------------------------------------------------
// File:         Tile3DArray.cs
// Description:  Represent a tile instance array.
// Module:       Til3map
// Author:       Noé Masse
// Date:         24/04/2023
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Til3map
{
    [Serializable]
    public struct Tile3DInstances
    {
        public Tile3D tile;
        public List<TilePose> poses;
    }
}

