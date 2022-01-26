using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Plane : Object
    {
        Vector3 normal;

        public Plane(Vector3 position, Vector3 normal, Color color, String info = "") : base(position, color, true, new Vector3(), info)
        {
            this.normal = normal;
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Plane(testPos, this.normal, this.position.Z);
        }
    }
}
