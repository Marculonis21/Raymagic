using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Level2
    {
        public Level2()
        {
            MapData data = new MapData();
            data.mapName = "_lvl2cs";

            Vector3 ZERO = new Vector3(0,0,0);

            data.topCorner = new Vector3(500,700,300);
            data.botCorner = new Vector3(0,0,0);
            data.levelStartAnchor = new Vector3(0,0,75);
            data.levelEndAnchor   = new Vector3(0,0,75);

            data.playerSpawn = new Vector3(250,50,150);

            // ############## OUTSIDEWALLS #################

            Plane floor = new Plane(ZERO + new Vector3(0,0,0),
                                    new Vector3(0,0,1),
                                    Color.Beige);

            Plane roof = new Plane(ZERO + new Vector3(0,0,298),
                                   new Vector3(0,0,-1),
                                   Color.Beige);

            Plane wall1 = new Plane(ZERO + new Vector3(2,0,0),
                                    new Vector3(1,0,0),
                                    Color.Beige);
            Plane wall2 = new Plane(ZERO + new Vector3(498,0,0),
                                    new Vector3(-1,0,0),
                                    Color.Beige);
            Plane wall3 = new Plane(ZERO + new Vector3(0,2,0),
                                    new Vector3(0,1,0),
                                    Color.Beige);
            Plane wall4 = new Plane(ZERO + new Vector3(0,698,0),
                                    new Vector3(0,-1,0),
                                    Color.Beige);

            data.staticMapObjects.Add(floor);
            data.staticMapObjects.Add(roof);

            data.staticMapObjects.Add(wall1);
            data.staticMapObjects.Add(wall2);
            data.staticMapObjects.Add(wall3);
            data.staticMapObjects.Add(wall4);

            Box endWall = new Box(ZERO + new Vector3(250,700,150),
                                  new Vector3(500,250,300),
                                  Color.Beige);
            endWall.AddChildObject(new Box(new Vector3(0,-20,-75),new Vector3(250,300,150),Color.Black), true);
            data.staticMapObjects.Add(endWall);

            Box glassEnd = new Box(ZERO + new Vector3(250,590,75),
                                  new Vector3(250,6,150),
                                  Color.Gray);
            glassEnd.SetTransparent(true);

            // ############## LIGHTS #################
            
            data.mapLights.Add(new Light(ZERO + new Vector3(250,250,200), Color.White, 20000, ZERO, ZERO+data.topCorner));
        
            // ############## INSIDE #################

            Map.instance.RegisterMap(data.mapName, data);
        }
    }
}
