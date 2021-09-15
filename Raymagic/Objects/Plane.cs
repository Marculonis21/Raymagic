using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Plane : IObject
    {
        Vector3 normal;

        public Plane(Vector3 position, Vector3 normal, Color color) : base()
        {
            this.position = position;
            this.normal = normal;
            this.color = color;
        }

        public override float SDF(Vector3 testPos)
        {
            float dst = SDFs.Plane(testPos, this.normal, this.position.Z);

            for(int i = 0; i < this.booleanObj.Count; i++)
            {
                switch(this.booleanOp[i])
                {
                    case BooleanOP.DIFFERENCE:
                        dst = SDFs.BooleanDifference(dst, this.booleanObj[i].SDF(testPos));
                        break;
                    case BooleanOP.INTERSECT:
                        dst = SDFs.BooleanIntersect(dst, this.booleanObj[i].SDF(testPos));
                        break;
                    case BooleanOP.UNION:
                        dst = SDFs.BooleanUnion(dst, this.booleanObj[i].SDF(testPos));
                        break;
                    default: 
                        throw new Exception("Unknown boolean operation!");
                }
            }

            return dst;
        }
    }
}
