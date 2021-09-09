using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class TestArea
    {
        public TestArea()
        {
            MapData data = new MapData();

            data.topCorner = new Vector3(500,500,300);
            data.botCorner = new Vector3(0,0,0);

            Plane mainPlain = new Plane(new Vector3(0,0,0),
                                        new Vector3(0,0,1),
                                        Color.White);

            data.staticMapObjects.Add(mainPlain);

            Plane plain1 = new Plane(new Vector3(0,0,495),
                                     new Vector3(-1,0,0),
                                     Color.White);
            Plane plain2 = new Plane(new Vector3(0,0,495),
                                     new Vector3(0,-1,0),
                                     Color.White);
            Plane plain3 = new Plane(new Vector3(0,0,450),
                                     new Vector3(-0.5f,-0.5f,0),
                                     Color.White);

            data.staticMapObjects.Add(plain1);
            data.staticMapObjects.Add(plain2);
            data.staticMapObjects.Add(plain3);

            Light light = new Light(new Vector3(300,300,200),
                                    100);

            data.mapLights.Add(light);

            data.playerSpawn = new Vector3(2,2,1);

            Map.instance.AddMap("testArea", data);
        }
    }
}
