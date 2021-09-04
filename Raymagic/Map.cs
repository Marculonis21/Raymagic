using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class Map
    {
        //SINGLETON
        MapData data;
        public string[,] map;

        MapLayout layout = MapLayout.instance;

        int blockSize = 100;

        private Map()
        {}

        public static readonly Map instance = new Map();

        public void SetMap(string id, MainGame game)
        {
            data = layout.maps[id];
            game.objectList = data.mapObjects;
        }

        public Vector3 GetPlayerStart()
        {
            Vector3 spawn = data.playerSpawn;
            return new Vector3(spawn.X*100, spawn.Y*100, spawn.Z*100);
        }
    }
}
