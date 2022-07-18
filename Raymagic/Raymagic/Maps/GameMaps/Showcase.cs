using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Showcase
    {
        public Showcase()
        {
            MapData data = new MapData();

            data.topCorner = new Vector3(300,1000,200);
            data.botCorner = new Vector3(0,0,0);

            data.playerSpawn = new Vector3(250,50,100);

            Plane floor = new Plane(new Vector3(),
                                    new Vector3(0,0,1),
                                    Color.Beige);
            Plane roof = new Plane(new Vector3(0,0,198),
                                   new Vector3(0,0,-1),
                                   Color.Beige);

            data.staticMapObjects.Add(floor);
            data.staticMapObjects.Add(roof);

            Plane wall1 = new Plane(new Vector3(2,0,0),
                                    new Vector3(1,0,0),
                                    Color.Beige);
            Plane wall2 = new Plane(new Vector3(298,0,0),
                                    new Vector3(-1,0,0),
                                    Color.Beige);
            Plane wall3 = new Plane(new Vector3(0,2,0),
                                    new Vector3(0,1,0),
                                    Color.Beige);
            Plane wall4 = new Plane(new Vector3(0,998,0),
                                    new Vector3(0,-1,0),
                                    Color.Beige);

            data.staticMapObjects.Add(wall1);
            data.staticMapObjects.Add(wall2);
            data.staticMapObjects.Add(wall3);
            data.staticMapObjects.Add(wall4);

            Box xBox = new Box(new Vector3(0,0,0),
                               new Vector3(100,20,20),
                               Color.Red);
            Box yBox = new Box(new Vector3(0,0,0),
                               new Vector3(20,100,20),
                               Color.Blue);
            Box zBox = new Box(new Vector3(0,0,0),
                               new Vector3(20,20,100),
                               Color.Green);

            data.staticMapObjects.Add(xBox);
            data.staticMapObjects.Add(yBox);
            data.staticMapObjects.Add(zBox);

            Light light1 = new Light(new Vector3(125,100,175),
                                    25);

            Light light2 = new Light(new Vector3(125,300,175),
                                    25);

            Light light3 = new Light(new Vector3(125,500,175),
                                    25);

            Light light4 = new Light(new Vector3(125,700,175),
                                    25);

            Light light5 = new Light(new Vector3(125,900,175),
                                    25);

            data.mapLights.Add(light1);
            data.mapLights.Add(light2);
            data.mapLights.Add(light3);
            data.mapLights.Add(light4);
            data.mapLights.Add(light5);

            Box boxWall1 = new Box(new Vector3(75, 200, 70), new Vector3(150, 10, 300), Color.DarkGray);
            Box boxWall2 = new Box(new Vector3(75, 400, 70), new Vector3(150, 10, 300), Color.DarkGray);
            Box boxWall3 = new Box(new Vector3(75, 600, 70), new Vector3(150, 10, 300), Color.DarkGray);
            Box boxWall4 = new Box(new Vector3(75, 800, 70), new Vector3(150, 10, 300), Color.DarkGray);
            data.staticMapObjects.Add(boxWall1);
            data.staticMapObjects.Add(boxWall2);
            data.staticMapObjects.Add(boxWall3);
            data.staticMapObjects.Add(boxWall4);

            Vector3[] testChamberCenterPoint = new Vector3[] {
                new Vector3(75, 100, 100),
                new Vector3(75, 300, 100),
                new Vector3(75, 500, 100),
                new Vector3(75, 700, 100),
                new Vector3(75, 900, 100),
            };

            PhysicsObject ball1 = new PhysicsObject(testChamberCenterPoint[0]-new Vector3(20,0,0), 25, Color.MistyRose, Color.Pink);
            data.physicsMapObjects.Add(ball1);

            Door door1 = new Door(testChamberCenterPoint[1] - new Vector3(-30,-30,100), new Vector3(1,0,0), Color.Azure);
            Door door2 = new Door(testChamberCenterPoint[1] - new Vector3(  0,  0,100), new Vector3(1,0,0), Color.Blue);
            Door door3 = new Door(testChamberCenterPoint[1] - new Vector3( 30, 30,100), new Vector3(1,0,0), Color.Violet);

            data.interactableObjectList.Add(door1);
            data.interactableObjectList.Add(door2);
            data.interactableObjectList.Add(door3);

            Map.instance.RegisterMap("showcase", data);
        }
    }
}
