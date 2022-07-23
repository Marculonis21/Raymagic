using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class TestArea
    {
        public TestArea()
        {
            MapData data = new MapData();
            data.mapName = "testArea";

            data.topCorner = new Vector3(500,500,300);
            data.botCorner = new Vector3(0,0,0);

            data.playerSpawn = new Vector3(100,100,100);

            Box xBox = new Box(new Vector3(0,0,0),
                               new Vector3(500,20,20),
                               Color.Red);
            Box yBox = new Box(new Vector3(0,0,0),
                               new Vector3(20,500,20),
                               Color.Blue);
            Box zBox = new Box(new Vector3(0,0,0),
                               new Vector3(20,20,500),
                               Color.Green);

            data.staticMapObjects.Add(xBox);
            data.staticMapObjects.Add(yBox);
            data.staticMapObjects.Add(zBox);

            Plane mainPlaneW = new Plane(new Vector3(0,0,0),
                                         new Vector3(0,0,1),
                                         Color.White,
                                         selectable:true);

            data.staticMapObjects.Add(mainPlaneW);

            Plane roofPlane = new Plane(new Vector3(0,0,295),
                                        new Vector3(0,0,-1),
                                        Color.Black,
                                        selectable:true);

            data.staticMapObjects.Add(roofPlane);

            Plane plane1 = new Plane(new Vector3(2,0,0),
                                     new Vector3(1,0,0),
                                     Color.Red,
                                     selectable:true);
            Plane plane2 = new Plane(new Vector3(498,0,0),
                                     new Vector3(-1,0,0),
                                     Color.Red,
                                     selectable:true);
            Plane plane3 = new Plane(new Vector3(0,2,0),
                                     new Vector3(0,1,0),
                                     Color.Blue);
            Plane plane4 = new Plane(new Vector3(0,498,0),
                                     new Vector3(0,-1,0),
                                     Color.Blue);


            plane4.AddChildObject(new Sphere(new Vector3(250,0,50),
                                             50,
                                             Color.Yellow,
                                             booleanOP: BooleanOP.SUNION, 
                                             opStrength: 100), true);

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

            Box b = new Box(new Vector3(100,100,30),
                            new Vector3(200,200,20),
                            Color.Gray,
                            boundingBoxSize: new Vector3(210,210,300), selectable:true);

            b.AddChildObject(new Sphere(new Vector3(0,0,40),
                                        45,
                                        Color.Green,
                                        BooleanOP.SUNION, 40), true);

            b.AddChildObject(new Sphere(new Vector3(40,40,40),
                                        40,
                                        Color.Black,
                                        BooleanOP.SDIFFERENCE, 5), true);

            Box ramp = new Box(new Vector3(50,400,50),
                               new Vector3(100,200,200),
                               Color.LightGreen, selectable:true);

            ramp.AddChildObject(new Plane(new Vector3(0,0,0),
                                          new Vector3(0,-1,1),
                                          Color.Black,
                                          BooleanOP.INTERSECT), true);
            data.staticMapObjects.Add(ramp);

            data.physicsMapObjects.Add(new PhysicsObject(new Vector3(200,200,100), 25, Color.Green, Color.Gray));

            Box boxbox = new Box(new Vector3(400,200,75),
                                 new Vector3(150,150,150),
                                 Color.Gray, selectable:true);

            data.staticMapObjects.Add(boxbox);

            data.interactableObjectList.Add(new Button(new Vector3(50,50,0), new Vector3(1,0,0), Color.Blue));

            Map.instance.RegisterMap(data.mapName, data);
        }
    }
}
