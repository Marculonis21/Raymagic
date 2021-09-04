using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class MapData
    {
        public List<IObject> mapObjects = new List<IObject>();
        public Vector3 playerSpawn;
    }
}
