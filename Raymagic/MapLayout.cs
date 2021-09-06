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

            Box pillar = new Box(new Vector3(300,300,250),
                                 new Vector3(30, 30, 500),
                                 Color.Red);

            m.mapObjects.Add(pillar);

            Box dip = new Box(new Vector3(550,550,-50),
                              new Vector3(300,300,201),
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
                                          new Vector3(40,140,200),
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

            Box platform = new Box(new Vector3(200,200,50),
                                   new Vector3(20,100,25),
                                   Color.Orange);
            Box platform2 = new Box(new Vector3(220,200,50),
                                    new Vector3(20,100,50),
                                    Color.Orange);
            Box platform3 = new Box(new Vector3(240,200,50),
                                    new Vector3(20,100,75),
                                    Color.Orange);
            Box platform4 = new Box(new Vector3(260,200,50),
                                    new Vector3(20,100,100),
                                    Color.Orange);
            Box platform5 = new Box(new Vector3(280,200,50),
                                    new Vector3(20,100,125),
                                    Color.Orange);

            m.mapObjects.Add(platform);
            m.mapObjects.Add(platform2);
            m.mapObjects.Add(platform3);
            m.mapObjects.Add(platform4);
            m.mapObjects.Add(platform5);
            m.playerSpawn = new Vector3(2,2,2);

            maps.Add("basic", m);
        }

        public static readonly MapLayout instance = new MapLayout();
    }
}
