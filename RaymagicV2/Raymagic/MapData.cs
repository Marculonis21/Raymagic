using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class MapData
    {
        public List<Object> staticMapObjects = new List<Object>();
        public List<Object> dynamicMapObjects = new List<Object>();
        public List<Light> mapLights = new List<Light>();
        public Vector3 playerSpawn;

        public Vector3 topCorner;
        public Vector3 botCorner;
    }
}
