using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class MapData
    {
        public List<IObject> mapObjects = new List<IObject>();
        public List<Light> mapLights = new List<Light>();
        public Vector3 playerSpawn;
    }
}
