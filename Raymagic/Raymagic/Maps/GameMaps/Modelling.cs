using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Modelling
    {
        public Modelling()
        {
            MapData data = new MapData();

            data.topCorner = new Vector3(300,300,200);
            data.botCorner = new Vector3(0,0,0);

            data.playerSpawn = new Vector3(100,100,100);

            Plane floor = new Plane(new Vector3(),
                                    new Vector3(0,0,1),
                                    Color.Beige);
            Plane roof = new Plane(new Vector3(0,0,195),
                                   new Vector3(0,0,-1),
                                   Color.Beige);

            data.staticMapObjects.Add(floor);
            data.staticMapObjects.Add(roof);

            Plane wall1 = new Plane(new Vector3(2,0,0),
                                    new Vector3(1,0,0),
                                    Color.Red);
            Plane wall2 = new Plane(new Vector3(198,0,0),
                                    new Vector3(-1,0,0),
                                    Color.Red);
            Plane wall3 = new Plane(new Vector3(0,2,0),
                                    new Vector3(0,1,0),
                                    Color.Blue);
            Plane wall4 = new Plane(new Vector3(0,198,0),
                                    new Vector3(0,-1,0),
                                    Color.Blue);

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

            Light light1 = new Light(new Vector3(290,290,190),
                                    50);
            
            Light light2 = new Light(new Vector3(10,10,190),
                                    50);


            data.mapLights.Add(light1);
            data.mapLights.Add(light2);

            FloorButton floorButton = new FloorButton(new Vector3(200,200,0));
            data.interactableObjectList.Add(floorButton);
            /* data.physicsMapObjects.Add(new PhysicsObject(new Vector3(200,200,100), 25, Color.Pink, Color.MistyRose)); */

            Door door = new Door(new Vector3(200,100,0), new Vector3(0,1,0));
            floorButton.stateChangeEvent += door.EventListener;
            data.interactableObjectList.Add(door);



            Map.instance.RegisterMap("modeling", data);
        }
    }
}
