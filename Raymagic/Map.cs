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

        public void SetMap(string id)
        {
            data = layout.maps[id];
            map = data.map;
        }

        public Vector3 GetPlayerStart()
        {
            Vector3 spawn = data.playerSpawn;
            return new Vector3(spawn.X*100 + 50, spawn.Y*100 + 50, spawn.Z*100 + 50);
        }

        public bool Collide(Vector3 testPos, out Color color)
        {
            color = Color.Blue;

            int x = (int)Math.Floor(testPos.X / this.blockSize);
            int y = (int)Math.Floor(testPos.Y / this.blockSize);
            int z = (int)Math.Floor(testPos.Z / this.blockSize);
            
            char col = map[z,x].ToCharArray()[y];
            if(col == ' ')
                return false;
            else
            {
                switch(col)
                {
                    case 'W':
                        color = Color.White;
                        return true;
                    case 'R':
                        color = Color.DarkRed;
                        return true;
                    case 'G':
                        color = Color.Green;
                        return true;
                }
                return true;
            }
        }
    }
}
