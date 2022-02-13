using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class TestArea
    {
        public TestArea()
        {
            MapData data = new MapData();

            Vector3 topCorner = new Vector3(512,512,512);
            Vector3 botCorner = new Vector3(0,0,0);

            data.topCorner = topCorner;
            data.botCorner = botCorner;

            Plane mainPlaneW = new Plane(new Vector3(0,0,0),
                                         new Vector3(0,0,1),
                                         Color.White);

            data.staticMapObjects.Add(mainPlaneW);
            int checkerSize = 50;

            /* for (int y = 0; y < (topCorner.Y-botCorner.Y)/checkerSize; y++) */
            /*     for (int x = 0; x < (topCorner.X-botCorner.X)/checkerSize; x++) */
            /*     { */
            /*         if((x+y)%2 == 0) */
            /*             data.staticMapObjects.Add(new Box(new Vector3(checkerSize/2 + x*checkerSize, y/2 + y*checkerSize, -10), */
            /*                                               new Vector3(checkerSize,checkerSize,21), */
            /*                                               Color.Black)); */
            /*     } */


            Plane roofPlane = new Plane(new Vector3(0,0,295),
                                        new Vector3(0,0,-1),
                                        Color.Black);

            data.staticMapObjects.Add(roofPlane);

            Plane plane1 = new Plane(new Vector3(2,0,0),
                                     new Vector3(1,0,0),
                                     Color.Red);
            Plane plane2 = new Plane(new Vector3(498,0,0),
                                     new Vector3(-1,0,0),
                                     Color.Red);
            Plane plane3 = new Plane(new Vector3(0,2,0),
                                     new Vector3(0,1,0),
                                     Color.Blue);
            Plane plane4 = new Plane(new Vector3(0,498,0),
                                     new Vector3(0,-1,0),
                                     Color.Blue);


            plane4.AddChildObject(new Sphere(new Vector3(250,0,50),
                                             50,
                                             Color.Yellow,
                                             true,
                                             BooleanOP.SUNION, 100), true);
            data.staticMapObjects.Add(plane1);
            data.staticMapObjects.Add(plane2);
            data.staticMapObjects.Add(plane3);
            data.staticMapObjects.Add(plane4);

            Box _b1 = new Box(new Vector3(250,250,50),
                              new Vector3(20,20,20),
                              Color.Gray);
            BoxFrame _b1Frame = new BoxFrame(new Vector3(250,250,50),
                                             new Vector3(40,40,40),
                                             10,
                                             Color.Green);

            Box _b2 = new Box(new Vector3(300,300,60),
                              new Vector3(20,20,20),
                              Color.Blue);

            BoxFrame _b2Frame = new BoxFrame(new Vector3(300,300,60),
                                             new Vector3(40,40,40),
                                             10,
                                             Color.DarkRed);

            Box _b3 = new Box(new Vector3(200,300,60),
                              new Vector3(20,20,20),
                              Color.Gold);

            BoxFrame _b3Frame = new BoxFrame(new Vector3(200,300,60),
                                             new Vector3(40,40,40),
                                             10,
                                             Color.DarkGoldenrod);

            data.staticMapObjects.Add(_b1);
            data.staticMapObjects.Add(_b2);
            data.staticMapObjects.Add(_b3);
            data.staticMapObjects.Add(_b1Frame);
            data.staticMapObjects.Add(_b2Frame);
            data.staticMapObjects.Add(_b3Frame);


            Light light = new Light(new Vector3(300,300,200),
                                    100);

            data.mapLights.Add(light);

            data.playerSpawn = new Vector3(2,2,1);

            Sphere S1 = new Sphere(new Vector3(280,280,60),
                                   25,
                                   Color.Purple,
                                   false,
                                   boundingBoxSize: new Vector3(50,50,50));
            

            /* data.dynamicMapObjects.Add(S1); */

            Sphere S2 = new Sphere(new Vector3(210,290,35),
                                   15,
                                   Color.DarkRed,
                                   false,
                                   boundingBoxSize: new Vector3(40,40,40),
                                   info:"center");


            Box b = new Box(new Vector3(100,100,30),
                            new Vector3(200,200,20),
                            Color.Gray,
                            false,
                            boundingBoxSize: new Vector3(210,210,300));

            b.AddChildObject(new Sphere(new Vector3(0,0,40),
                                        45,
                                        Color.Green,
                                        false,
                                        BooleanOP.SUNION, 40), true);

            b.AddChildObject(new Sphere(new Vector3(40,40,40),
                                        40,
                                        Color.Black,
                                        false, BooleanOP.SDIFFERENCE, 5), true);


            data.dynamicMapObjects.Add(b);

            /* data.dynamicMapObjects.Add(S2); */


            Map.instance.AddMap("testArea", data);
        }
    }
}
