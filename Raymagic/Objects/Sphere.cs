using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Sphere : Object
    {
        float size;

        public Sphere(Vector3 position, float size, Color color, bool staticObject = true, float boundingSize = 0, string info = "") : base(position, color, staticObject, boundingSize, info)
        {
            this.size = size;
        }

        public override float SDF(Vector3 testPos, float minDist, bool physics)
        {
            Vector3 tPos = this.staticObject ? testPos : Transform(testPos);
            if(!this.staticObject && !physics)
                if(minDist <= SDFs.Sphere(tPos, this.position, this.boundingSize)) return minDist + 1;

            float dst = SDFs.Sphere(tPos, this.position, this.size);

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
