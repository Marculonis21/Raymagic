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

        public Vector3 SDF_normal(Vector3 testPos)
        {
            const float EPS = 0.001f;
            Vector3 p = testPos;
            Vector3 pX = new Vector3(p.X + EPS, p.Y, p.Z);
            Vector3 mX = new Vector3(p.X - EPS, p.Y, p.Z);

            Vector3 pY = new Vector3(p.X, p.Y + EPS, p.Z);
            Vector3 mY = new Vector3(p.X, p.Y - EPS, p.Z);

            Vector3 pZ = new Vector3(p.X, p.Y, p.Z + EPS);
            Vector3 mZ = new Vector3(p.X, p.Y, p.Z - EPS);

            Vector3 normal = new Vector3(SDF(pX) - SDF(mX),
                                         SDF(pY) - SDF(mY),
                                         SDF(pZ) - SDF(mZ)); 
            normal.Normalize();

            return normal;
        }

        public Color GetColor()
        {
            return color;
        }
    }
}
