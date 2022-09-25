using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Plane : Object
    {
        Vector3 normal;

        public Plane(Vector3 position, Vector3 normal, Color color, BooleanOP booleanOP=BooleanOP.NONE, float booleanStrength=1, bool selectable=false, String info = "") : base(position, color, new Vector3(), info, booleanOP, booleanStrength, selectable)
        {
            this.normal = Vector3.Normalize(normal);
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Plane(testPos, this.normal);
        }
    }
}
