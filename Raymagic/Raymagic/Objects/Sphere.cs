using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Sphere : Object
    {
        float size;

        public Sphere(Vector3 position, float size, Color color, BooleanOP booleanOP=BooleanOP.NONE, float opStrength=1, Vector3 boundingBoxSize = new Vector3(), bool selectable=false, string info = "") : base(position, color, boundingBoxSize, info, booleanOP, opStrength, selectable)
        {
            this.size = size;
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Sphere(testPos, this.size);
        }
    }
}
