using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Level1
    {
        public Level1()
        {
            MapData data = new MapData();
            data.mapName = "_lvl1cs";

            Vector3 ZERO = new Vector3(0,0,0);

            data.topCorner = new Vector3(300,400,200);
            data.botCorner = new Vector3(0,0,0);
            data.levelStartAnchor = new Vector3(0,0,75);
            data.levelEndAnchor   = new Vector3(0,0,75);

            data.playerSpawn = new Vector3(150,100,100);

            // ############## OUTSIDEWALLS #################

            Plane floor = new Plane(ZERO + new Vector3(0,0,0),
                                    new Vector3(0,0,1),
                                    Color.Beige);

            Plane roof = new Plane(ZERO + new Vector3(0,0,198),
                                   new Vector3(0,0,-1),
                                   Color.Beige);

            Plane wall1 = new Plane(ZERO + new Vector3(2,0,0),
                                    new Vector3(1,0,0),
                                    Color.Beige);
            Plane wall2 = new Plane(ZERO + new Vector3(298,0,0),
                                    new Vector3(-1,0,0),
                                    Color.Beige);
            Plane wall3 = new Plane(ZERO + new Vector3(0,2,0),
                                    new Vector3(0,1,0),
                                    Color.Beige);
            Plane wall4 = new Plane(ZERO + new Vector3(0,398,0),
                                    new Vector3(0,-1,0),
                                    Color.Beige);

            data.staticMapObjects.Add(floor);
            data.staticMapObjects.Add(roof);

            data.staticMapObjects.Add(wall1);
            data.staticMapObjects.Add(wall2);
            data.staticMapObjects.Add(wall3);
            data.staticMapObjects.Add(wall4);

            // ############## LIGHTS #################
            
            data.mapLights.Add(new Light(ZERO + new Vector3(150,200,170), Color.White, 20000, ZERO, ZERO+data.topCorner));
        
            // ############## INSIDE #################

            Box glassPlane = new Box(ZERO + new Vector3(225,150,100), 
                                     new Vector3(148,6,198),
                                     Color.Gray);
            glassPlane.SetTransparent(true);

            data.staticMapObjects.Add(glassPlane);

            Box glassFrame = new Box(ZERO + new Vector3(225,150,100),
                                     new Vector3(150,10,200),
                                     Color.Black, 
                                     boundingBoxSize:new Vector3(155,15,205));
            glassFrame.AddChildObject(new Box(new Vector3(), new Vector3(140,100,190), Color.Black,BooleanOP.DIFFERENCE), true);
            data.staticMapObjects.Add(glassFrame);

            FloorButton floorButton = new FloorButton(new Vector3(225,75,0), Color.Aqua);
            Door outDoor = new Door(ZERO + new Vector3(80,400,0), new Vector3(0,-1,0), wall4, Color.Aqua);
            floorButton.stateChangeEvent += outDoor.EventListener;

            Door inDoor = new Door(ZERO + new Vector3(0,100,0), new Vector3(1,0,0), wall4, Color.Gray);

            data.interactableObjectList.Add(floorButton);
            data.interactableObjectList.Add(outDoor);

            data.physicsMapObjects.Add(new PhysicsObject(ZERO + new Vector3(225,325,50), 25, Color.Gray, Color.Green));

            Map.instance.RegisterMap(data.mapName, data);
        }
    }
}
