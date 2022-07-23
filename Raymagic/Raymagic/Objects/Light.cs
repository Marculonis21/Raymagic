using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Light
    {
        public Vector3 position {get; set;}
        public float intensity {get; private set;}
        public Color color {get; private set;}

        public Light(Vector3 position, Color color, float intensity)
        {
            this.position = position;
            this.intensity = intensity;
            this.color = color;
        }

        public float DistanceFrom(Vector3 testPos)
        {
            return SDFs.Point(testPos, position);
        }
    }
}
