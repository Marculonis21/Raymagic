using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class Map
    {
        //SINGLETON
        public string[,] map;

        MapLayout layout = MapLayout.instance;

        private Map()
        {
        }

        public static readonly Map instance = new Map();

        public void SetMap(string id)
        {
            map = layout.maps[id];
        }

        public Vector3 GetPlayerStart()
        {
            for(int floor = 0; floor < map.Length; floor++)
            {
                for(int x = 0; x < map[floor, 0].Length; x++)
                {
                    char[] chr = map[floor, x].ToCharArray();
                    for(int y = 0; y < chr.Length; y++)
                    {
                        if(chr[y] == 'O')
                            return new Vector3(x*100 + 50, y*100 + 50, floor*100 + 50);
                    }
                }
            }

            throw new Exception("Player start not found");
        }
    }
}
