using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class MapData
    {
        public string mapName;
       
        // FOR COMPILED ONLY
            public bool isCompiled = false;
            public string path;
        //

        public Vector3 topCorner;
        public Vector3 botCorner;

        public Vector3 playerSpawn;

        public Vector3 levelStartAnchor;
        public Vector3 levelEndAnchor;
        public Door2 inDoor;
        public Door2 outDoor;

        public string nextLevelID;
        public float nextLevelDetail;
        public Color nextLevelInColor;

        public List<Object> staticMapObjects = new List<Object>();
        public List<Object> dynamicMapObjects = new List<Object>();
        public List<PhysicsObject> physicsMapObjects = new List<PhysicsObject>();
        public List<Interactable> interactableObjectList = new List<Interactable>();
        public List<Light> mapLights = new List<Light>();
    }
}
