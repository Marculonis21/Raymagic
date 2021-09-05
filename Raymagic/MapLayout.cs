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
            /* MapData m = new MapData(new string[,] {{"GGGGGGGGG", */
            /*                                         "GGGGGGGGG", */ 
            /*                                         "GGGGGGGGG", */
            /*                                         "GGGGGGGGG", */
            /*                                         "GGGGGGGGG", */
            /*                                         "GGGGGGGGG", */
            /*                                         "GGGGGGGGG", */
            /*                                         "GGGGGGGGG"}, */
            /*                                        {"WWWWWWWWW", */
            /*                                         "WWWWWWWWW", */
            /*                                         "WWWWWWWWW", */
            /*                                         "WWGGWGWGG", */
            /*                                         "WWG     G", */
            /*                                         "WWG  Y  G", */
            /*                                         "WWG     G", */
            /*                                         "WWGGGGGGG"}, */
            /*                                        {"WWWWWWWWW", */
            /*                                         "W       W", */
            /*                                         "W       W", */
            /*                                         "W   W W W", */
            /*                                         "W       W", */
            /*                                         "W    O  W", */
            /*                                         "W       W", */
            /*                                         "WWWWWWWWW"}, */
            /*                                        {"WRWWWWWWW", */
            /*                                         "WRWWWWWWW", */
            /*                                         "WRWWWWWWW", */
            /*                                         "WRWWWWWWW", */
            /*                                         "WRW     W", */
            /*                                         "WRW  L  W", */
            /*                                         "WRW     W", */
            /*                                         "WWWWWWWWW"}, */
            /*                                        {"RRRRRRRRR", */
            /*                                         "RRRRRRRRR", */
            /*                                         "RRRRRRRRR", */
            /*                                         "RRRRRRRRR", */
            /*                                         "RRRRRRRRR", */
            /*                                         "RRRRRRRRR", */
            /*                                         "RRRRRRRRR", */
            /*                                         "RRRRRRRRR"}}, */
            /*                         new Vector3(2,2,2)); */

            MapData m = new MapData();

            Box area = new Box(new Vector3(400,400,250),
                               new Vector3(800,800,500),
                               Color.White);

            //floor height 50
            area.booleanOp.Add("difference");
            area.booleanObj.Add(new Box(new Vector3(400,400,250),
                                        new Vector3(600,600,400),
                                        Color.Black));
            area.booleanOp.Add("difference");
            area.booleanObj.Add(new Box(new Vector3(550,550,0),
                                        new Vector3(201,201,400),
                                        Color.Black));
            
            m.mapObjects.Add(area);

            Box pillar = new Box(new Vector3(400,400,250),
                                 new Vector3(10, 10, 500),
                                 Color.Red);

            m.mapObjects.Add(pillar);

            Box dip = new Box(new Vector3(550,550,-50),
                              new Vector3(300,300,220),
                              Color.Blue);

            dip.booleanOp.Add("difference");
            dip.booleanObj.Add(new Box(new Vector3(550,550,0),
                                       new Vector3(200,200,200),
                                       Color.Black));

            m.mapObjects.Add(dip);

            Box tunnel = new Box(new Vector3(550,400,50),
                                 new Vector3(300,100,800),
                                 Color.Gray);

            tunnel.booleanOp.Add("difference");
            tunnel.booleanObj.Add(new Box(new Vector3(550,400,50),
                                          new Vector3(30,140,200),
                                          Color.Black));


            m.mapObjects.Add(tunnel);

            Box tunnel2 = new Box(new Vector3(400,550,50),
                                  new Vector3(100,400,800),
                                  Color.Gray);

            tunnel2.booleanOp.Add("union");
            tunnel2.booleanObj.Add(new Sphere(new Vector3(400,550,100),
                                              80,
                                              Color.Black));

            m.mapObjects.Add(tunnel2);

            m.playerSpawn = new Vector3(2,2,1);

            maps.Add("basic", m);
        }

        public static readonly MapLayout instance = new MapLayout();
    }
}
