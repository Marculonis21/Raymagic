using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Light
    {
        public Vector3 position {get; private set;}
        public float intensity {get; private set;}
        /* public Color color {get; private set;} */

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

        public void ChangePosition(Vector3 position)
        {
            this.position = position;
        }

        public void Translate(Vector3 relativePosition)
        {
            this.position += position;
        }
    }
}
