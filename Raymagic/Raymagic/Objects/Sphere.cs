using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Sphere : Object
    {
        float size;

        public Sphere(Vector3 position, float size, Color color, bool staticObject = true, BooleanOP booleanOP=BooleanOP.NONE, float booleanStrength=1, Vector3 boundingBoxSize = new Vector3(), string info = "") : base(position, color, staticObject, boundingBoxSize, info, booleanOP, booleanStrength)
        {
            this.size = size;
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Sphere(testPos, this.size);
        }
    }
}
