using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Light
    {
        public Vector3 position {get; private set;}
        public float intensity {get; private set;}
        /* public Color color {get; private set;} */

        public List<IObject> dObjVisible = new List<IObject>();

        public Light(Vector3 position, float intensity)
        {
            this.position = position;
            this.intensity = intensity;
            /* this.color = color; */
        }

        public float SDF(Vector3 testPos)
        {
            return SDFs.Point(testPos, position);
        }
    }
}
