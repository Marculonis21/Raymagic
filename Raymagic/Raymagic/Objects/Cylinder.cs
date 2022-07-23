using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Cylinder : Object
    {
        Vector3 a;
        Vector3 b;
        float radius;

        public Cylinder(Vector3 position, Vector3 baseNormal, float height, float radius, Color color, BooleanOP booleanOP=BooleanOP.NONE, float opStrength=1, Vector3 boundingBoxSize = new Vector3(), bool selectable=false, string info="") : base(position, color, boundingBoxSize, info, booleanOP, opStrength, selectable)
        {
            this.a = new Vector3();
            this.b = a - (baseNormal*height);
            this.radius = radius;
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.CCylinder(testPos, this.a, this.b, radius);
        }
    }
}
