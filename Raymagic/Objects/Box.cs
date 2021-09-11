using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Box : IObject
    {
        Vector3 size;

        public Box(Vector3 position, Vector3 size, Color color)
        {
            this.position = position;
            this.size = size;
            this.color = color;
        }

        public override float SDF(Vector3 testPos)
        {
            float dst = SDFs.Box(testPos, this.position, this.size);

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

        public override void ChangePosition(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public override void Translate(Vector3 relativePosition)
        {
            throw new NotImplementedException();
        }
    }
}
