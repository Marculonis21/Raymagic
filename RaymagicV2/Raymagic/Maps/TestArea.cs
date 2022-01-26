using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class TestArea
    {
        public TestArea()
        {
            MapData data = new MapData();

            Vector3 topCorner = new Vector3(500,500,300);
            Vector3 botCorner = new Vector3(0,0,0);

            data.topCorner = topCorner;
            data.botCorner = botCorner;

            Plane mainPlaneW = new Plane(new Vector3(0,0,0),
                                         new Vector3(0,0,1),
                                         Color.White);

            data.staticMapObjects.Add(mainPlaneW);
            int checkerSize = 50;

            for (int y = 0; y < (topCorner.Y-botCorner.Y)/checkerSize; y++)
                for (int x = 0; x < (topCorner.X-botCorner.X)/checkerSize; x++)
                {
                    if((x+y)%2 == 0)
                        data.staticMapObjects.Add(new Box(new Vector3(checkerSize/2 + x*checkerSize, y/2 + y*checkerSize, -10),
                                                          new Vector3(checkerSize,checkerSize,21),
                                                          Color.Black));
                }


            Plane roofPlane = new Plane(new Vector3(0,0,295),
                                        new Vector3(0,0,-1),
                                        Color.Black);

            data.staticMapObjects.Add(roofPlane);

            Plane plane1 = new Plane(new Vector3(0,0,495),
                                     new Vector3(-1,0,0),
                                     Color.Black);
            Plane plane2 = new Plane(new Vector3(0,0,495),
                                     new Vector3(0,-1,0),
                                     Color.Black);
            Plane plane3 = new Plane(new Vector3(0,495,0),
                                     new Vector3(0,1,0),
                                     Color.Black);
            Plane plane4 = new Plane(new Vector3(0,495,0),
                                     new Vector3(1,0,0),
                                     Color.Black);

            data.staticMapObjects.Add(plane1);
            data.staticMapObjects.Add(plane2);
            data.staticMapObjects.Add(plane3);
            data.staticMapObjects.Add(plane4);

            Light light = new Light(new Vector3(300,300,200),
                                    100);

            data.mapLights.Add(light);

            data.playerSpawn = new Vector3(2,2,1);

            Sphere S1 = new Sphere(new Vector3(280,280,60),
                                   25,
                                   Color.Purple,
                                   false,
                                   new Vector3(50,50,50));
            

            data.dynamicMapObjects.Add(S1);

            Sphere S2 = new Sphere(new Vector3(210,290,35),
                                   15,
                                   Color.DarkRed,
                                   false,
                                   new Vector3(40,40,40),
                                   info:"center");


            Box b = new Box(new Vector3(100,100,30),
                            new Vector3(100,100,20),
                            Color.Gray,
                            false,
                            new Vector3(110,110,200));

            b.AddBoolean(BooleanOP.SUNION,
                         new Sphere(new Vector3(0,0,20),
                                    40,
                                    Color.Black,
                                    false));

            data.dynamicMapObjects.Add(b);

            data.dynamicMapObjects.Add(S2);

            Map.instance.AddMap("testArea", data);
        }
    }
}
