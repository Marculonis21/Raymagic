using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Map
    {
        //SINGLETON
        Dictionary<string, MapData> maps = new Dictionary<string, MapData>();
        MapData data;

        private Map()
        {
        }

        public static readonly Map instance = new Map();

        public void AddMap(string id, MapData data)
        {
            maps.Add(id, data);
        }

        public void LoadMaps()
        {
            new Basic();
        }

        public void SetMap(string id, MainGame game)
        {
            data = maps[id];
            game.objectList = data.mapObjects;
        }

        public Vector3 GetPlayerStart()
        {
            Vector3 spawn = data.playerSpawn;
            return new Vector3(spawn.X*100, spawn.Y*100, spawn.Z*100);
        }
    }
}
