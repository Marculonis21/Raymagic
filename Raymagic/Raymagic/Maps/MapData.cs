using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class MapData
    {
        public string mapName;
        public bool isCompiled = false;
        public string path;

        public List<Object> staticMapObjects = new List<Object>();
        public List<Object> dynamicMapObjects = new List<Object>();
        public List<PhysicsObject> physicsMapObjects = new List<PhysicsObject>();
        public List<Interactable> interactableObjectList = new List<Interactable>();
        public List<Light> mapLights = new List<Light>();
        public Vector3 playerSpawn;

        public Vector3 topCorner;
        public Vector3 botCorner;

        public Vector3 levelStartAnchor;
        public Vector3 levelEndAnchor;
    }
}
