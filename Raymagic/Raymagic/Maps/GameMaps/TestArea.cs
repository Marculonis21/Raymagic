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

            data.topCorner = topCorner;
            data.botCorner = botCorner;

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

            data.playerSpawn = new Vector3(2,2,1);

            Box b = new Box(new Vector3(100,100,30),
                            new Vector3(200,200,20),
                            Color.Gray,
                            false,
                            boundingBoxSize: new Vector3(210,210,300), selectable:true);

            b.AddChildObject(new Sphere(new Vector3(0,0,40),
                                        45,
                                        Color.Green,
                                        false,
                                        BooleanOP.SUNION, 40), true);

            b.AddChildObject(new Sphere(new Vector3(40,40,40),
                                        40,
                                        Color.Black,
                                        false, BooleanOP.SDIFFERENCE, 5), true);

            /* data.dynamicMapObjects.Add(b); */

            Box ramp = new Box(new Vector3(50,400,50),
                               new Vector3(100,200,200),
                               Color.LightGreen,
                               false, boundingBoxSize:new Vector3(100,200,200), selectable:true);

            ramp.AddChildObject(new Plane(new Vector3(0,0,0),
                                          new Vector3(0,-1,1),
                                          Color.Black,
                                          BooleanOP.INTERSECT), true);
            data.dynamicMapObjects.Add(ramp);

            data.interactableObjectList.Add(new Button(new Vector3(200,200,50), new Vector3(0, -1, 0)));
            /* data.physicsMapObjects.Add(new PhysicsObject(new Vector3(50,450,250), 25, Color.Green, Color.Gray)); */
            /* data.physicsMapObjects.Add(new PhysicsTrigger(new Vector3(100,100,100), 100)); */
            /* data.physicsMapObjects.Add(new PhysicsObject(new Vector3(200,200,200), 25, Color.Violet)); */
            /* data.physicsMapObjects.Add(new PhysicsObject(new Vector3(60,200,200), 25, Color.Orange)); */
            /* data.physicsMapObjects.Add(new PhysicsObject(new Vector3(50,100,200), 25, Color.Aqua)); */

            Box boxbox = new Box(new Vector3(400,200,75),
                                 new Vector3(150,150,150),
                                 Color.Gray, selectable:true);

            data.staticMapObjects.Add(boxbox);

            /* Capsule body = new Capsule(new Vector3(400,400,0), */
            /*                         75/2, */ 
            /*                         25, */ 
            /*                         Color.Orange, */
            /*                         false, */ 
            /*                         boundingBoxSize: new Vector3(35,35,75)); */

            /* Capsule topPart = new Capsule(new Vector3(0,0,-25), */
            /*                               75/2, */
            /*                               26, */
            /*                               Color.White, */
            /*                               false, */
            /*                               boundingBoxSize: new Vector3(35,35,75)); */

            /* topPart.AddChildObject(new Plane(new Vector3(0,0,75/4), */
            /*                                  new Vector3(0,0,-1), */
            /*                                  Color.Black, */
            /*                                  booleanOP: BooleanOP.INTERSECT), true); */

            /* body.AddChildObject(topPart, true); */
                                  
            /* data.dynamicMapObjects.Add(body); */

            /* Cylinder c = new Cylinder(new Vector3(400,400,50), new Vector3(0,0,1), */
            /*                           200, 50, Color.Orange); */
                                      
            /* data.staticMapObjects.Add(c); */

            Map.instance.RegisterMap("testArea", data);
        }
    }
}
