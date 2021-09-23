using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ConsoleRay
{
    public class Light
    {
        public Vector3 position {get; private set;}
        public float intensity {get; private set;}

        public Light(Vector3 position, float intensity)
        {
            this.position = position;
            this.intensity = intensity;
        }

        public float SDF(Vector3 testPos)
        {
            return SDFs.Point(testPos, position);
        }
    }
}
