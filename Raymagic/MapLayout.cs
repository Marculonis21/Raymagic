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
                                                    "WWGGGGGGG",
                                                    "WWG     G",
                                                    "WWG  W  G",
                                                    "WWG     G",
                                                    "WWGGGGGGG"},
                                                   {"WWWWWWWWW",
                                                    "W       W",
                                                    "W       W",
                                                    "W       W",
                                                    "W       W",
                                                    "W    W  W",
                                                    "W       W",
                                                    "WWWWWWWWW"},
                                                   {"WWWWWWWWW",
                                                    "WWWWWWWWW",
                                                    "WWWWWWWWW",
                                                    "WWWWWWWWW",
                                                    "WWW     W",
                                                    "WWW  W  W",
                                                    "WWW     W",
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
