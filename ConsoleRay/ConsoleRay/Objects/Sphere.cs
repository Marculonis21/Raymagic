using System;
using Microsoft.Xna.Framework;

namespace ConsoleRay
{
    public class Sphere : IObject
    {
        float size;

        public Sphere(Vector3 position, float size, ConsoleColor color, bool staticObject = true) : base()
        {
            this.position = position;
            this.size = size;
            this.color = color;
            this.staticObject = staticObject;

            if(!staticObject)
            {
                this.Translate(this.position);
                this.position = new Vector3();
            }
        }

        public override float SDF(Vector3 testPos)
        {
            float dst = SDFs.Sphere(this.staticObject ? testPos : Transform(testPos), this.position, this.size);

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
