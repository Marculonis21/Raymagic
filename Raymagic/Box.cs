using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class Box : IObject
    {
        Vector3 position;
        Vector3 size;
        Color color;

        public List<string> booleanOp = new List<string>();
        public List<IObject> booleanObj = new List<IObject>();

        public Box(Vector3 position, Vector3 size, Color color)
        {
            this.position = position;
            this.size = size;
            this.color = color;
        }

        public float SDF(Vector3 testPos)
        {
            float dst = SDFs.Box(testPos, this.position, this.size);

            for(int i = 0; i < booleanObj.Count; i++)
            {
                switch(booleanOp[i])
                {
                    case "difference":
                        dst = SDFs.BooleanDifference(dst, booleanObj[i].SDF(testPos));
                        break;
                    case "intersect":
                        dst = SDFs.BooleanIntersect(dst, booleanObj[i].SDF(testPos));
                        break;
                    case "union":
                        dst = SDFs.BooleanUnion(dst, booleanObj[i].SDF(testPos));
                        break;
                }
            }

            return dst;
        }

        public Color GetColor()
        {
            return color;
        }
    }
}
