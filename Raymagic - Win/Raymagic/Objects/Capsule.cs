using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Capsule : Object
    {
        float height;
        float radius;

        public Capsule(Vector3 position, float height, float radius, Color color, BooleanOP booleanOP=BooleanOP.NONE, float opStrength=1, Vector3 boundingBoxSize = new Vector3(), bool selectable=false, string info="") : base(new Vector3(position.X, position.Y, position.Z + radius), color, boundingBoxSize, info, booleanOP, opStrength, selectable)
        {
            this.height = height;
            this.radius = radius;
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Capsule(testPos, this.height, this.radius);
        }
    }
}
