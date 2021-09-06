using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public abstract class IObject
    {
        protected Vector3 position;
        protected Color color;

        protected List<BooleanOP> booleanOp = new List<BooleanOP>();
        protected List<IObject> booleanObj = new List<IObject>();

        public void AddBoolean(BooleanOP op, IObject obj)
        {
            booleanOp.Add(op);
            obj.position += this.position;
            booleanObj.Add(obj);
        }

        public abstract float SDF(Vector3 testPos);

        public Vector3 SDF_normal(Vector3 testPos)
        {
            const float EPS = 0.0001f;
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
