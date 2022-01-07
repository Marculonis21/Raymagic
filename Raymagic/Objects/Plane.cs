using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Plane : Object
    {
        Vector3 normal;

        public Plane(Vector3 position, Vector3 normal, Color color, String info = "") : base(position, color, true, 0, info)
        {
            this.normal = normal;
        }

        public override float SDF(Vector3 testPos, float minDist, bool physics)
        {
            float dst = SDFs.Plane(testPos, this.normal, this.position.Z);

            for(int i = 0; i < this.booleanObj.Count; i++)
            {
                switch(this.booleanOp[i])
                {
                    case BooleanOP.DIFFERENCE:
                        dst = SDFs.BooleanDifference(dst, this.booleanObj[i].SDF(testPos,minDist));
                        break;
                    case BooleanOP.INTERSECT:
                        dst = SDFs.BooleanIntersect(dst, this.booleanObj[i].SDF(testPos,minDist));
                        break;
                    case BooleanOP.UNION:
                        dst = SDFs.BooleanUnion(dst, this.booleanObj[i].SDF(testPos,minDist));
                        break;
                    default: 
                        throw new Exception("Unknown boolean operation!");
                }
            }

            return dst;
        }
    }
}
