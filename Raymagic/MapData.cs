using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class MapData
    {
        public List<IObject> staticMapObjects = new List<IObject>();
        public List<IObject> dynamicMapObjects = new List<IObject>();
        public List<Light> mapLights = new List<Light>();
        public Vector3 playerSpawn;

        public Vector3 topCorner;
        public Vector3 botCorner;
    }
}
