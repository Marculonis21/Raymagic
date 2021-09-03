using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class MapLayout
    {
        //SINGLETON
        public Dictionary<string, MapData> maps = new Dictionary<string, MapData>();

        private MapLayout()
        {
            MapData m = new MapData(new string[,] {{"GGGGGGGGG",
                                                    "GGGGGGGGG",
                                                    "GGGGGGGGG",
                                                    "GGGGGGGGG",
                                                    "GGGGGGGGG",
                                                    "GGGGGGGGG",
                                                    "GGGGGGGGG",
                                                    "GGGGGGGGG"},
                                                   {"WWWWWWWWW",
                                                    "WWWWWWWWW",
                                                    "WWWWWWWWW",
                                                    "WWGGWGWGG",
                                                    "WWG     G",
                                                    "WWG  Y  G",
                                                    "WWG     G",
                                                    "WWGGGGGGG"},
                                                   {"WWWWWWWWW",
                                                    "W       W",
                                                    "W       W",
                                                    "W   W W W",
                                                    "W       W",
                                                    "W    O  W",
                                                    "W       W",
                                                    "WWWWWWWWW"},
                                                   {"WRWWWWWWW",
                                                    "WRWWWWWWW",
                                                    "WRWWWWWWW",
                                                    "WRWWWWWWW",
                                                    "WRW     W",
                                                    "WRW  L  W",
                                                    "WRW     W",
                                                    "WWWWWWWWW"},
                                                   {"RRRRRRRRR",
                                                    "RRRRRRRRR",
                                                    "RRRRRRRRR",
                                                    "RRRRRRRRR",
                                                    "RRRRRRRRR",
                                                    "RRRRRRRRR",
                                                    "RRRRRRRRR",
                                                    "RRRRRRRRR"}},
                                    new Vector3(2,2,2));

            maps.Add("basic", m);

        }

        public static readonly MapLayout instance = new MapLayout();
    }
}
