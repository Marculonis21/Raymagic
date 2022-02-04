using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Plane : Object
    {
        Vector3 normal;

        public Plane(Vector3 position, Vector3 normal, Color color, BooleanOP booleanOP=BooleanOP.NONE, float booleanStrength=1, String info = "") : base(position, color, true, new Vector3(), info, booleanOP, booleanStrength)
        {
            this.normal = normal;
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Plane(testPos, this.normal);
        }
    }
}
