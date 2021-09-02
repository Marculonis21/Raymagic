using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class MapData
    {
        public string[,] map;
        public Vector3 playerSpawn; //use ints pls

        public MapData(string[,] map, Vector3 playerSpawn)
        {
            this.map = map;
            this.playerSpawn = playerSpawn;
        }
    }
}
