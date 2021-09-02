using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class MapLayout
    {
        //SINGLETON
        public Dictionary<string, string[,]> maps = new Dictionary<string, string[,]>();

        private MapLayout()
        {
            maps.Add("basic", new string[,] {{"FFFFFFFFF",
                                              "FFFFFFFFF",
                                              "FFFFFFFFF",
                                              "FFFFFFFFF",
                                              "FFFFFFFFF",
                                              "FFFFFFFFF",
                                              "FFFFFFFFF",
                                              "FFFFFFFFF"},
                                             {"WWWWWWWWW",
                                              "W       W",
                                              "W O     W",
                                              "W       W",
                                              "W       W",
                                              "W    W  W",
                                              "W       W",
                                              "WWWWWWWWW"},
                                             {"FFFFFFFFF",
                                              "FFFFFFFFF",
                                              "FFFFFFFFF",
                                              "FFFFFFFFF",
                                              "FFFFFFFFF",
                                              "FFFFFFFFF",
                                              "FFFFFFFFF",
                                              "FFFFFFFFF"}});
        }

        public static readonly MapLayout instance = new MapLayout();
    }
}
