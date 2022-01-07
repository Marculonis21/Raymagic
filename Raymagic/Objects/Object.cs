using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Extreme.Mathematics;
using Matrix = Extreme.Mathematics.Matrix;

namespace Raymagic
{
    public abstract class Object
    {
        protected Vector3 position;
        protected Color color;
        protected string info;

        protected float boundingSize;
        protected bool staticObject;

        protected List<BooleanOP> booleanOp = new List<BooleanOP>();
        protected List<Object> booleanObj = new List<Object>();

        protected Matrix<double> translateMatrix = Matrix.Create<double>(4,4);
        protected Matrix<double> rotationMatrix = Matrix.Create<double>(4,4);
        protected Matrix<double> transformInverse = Matrix.Create<double>(4,4);

        public Object(Vector3 position, Color color, bool staticObject, float boundingSize, string info)
        {
            this.translateMatrix[0,0] = 1;
            this.translateMatrix[1,1] = 1;
            this.translateMatrix[2,2] = 1;
            this.translateMatrix[3,3] = 1;

            this.rotationMatrix[0,0] = 1;
            this.rotationMatrix[1,1] = 1;
            this.rotationMatrix[2,2] = 1;
            this.rotationMatrix[3,3] = 1;

            this.position = position;
            this.color = color;
            this.staticObject = staticObject;
            this.boundingSize = boundingSize;
            this.info = info;

            if(!staticObject)
            {
                this.Translate(this.position);
                this.position = new Vector3();
            }
        }

        public void AddBoolean(BooleanOP op, Object obj)
        {
            booleanOp.Add(op);
            if(this.staticObject)
                obj.position += this.position;
            else
            {
                Vector3 origPos = new Vector3((float)this.translateMatrix[3,0],
                                              (float)this.translateMatrix[3,1],
                                              (float)this.translateMatrix[3,2]);

                Vector3 objPos = new Vector3((float)obj.translateMatrix[3,0],
                                             (float)obj.translateMatrix[3,1],
                                             (float)obj.translateMatrix[3,2]);
                obj.translateMatrix[3,0] = 0;
                obj.translateMatrix[3,1] = 0;
                obj.translateMatrix[3,2] = 0;

                obj.position = objPos;
                obj.Translate(origPos);
            }
            
            booleanObj.Add(obj);
        }

        public abstract float SDF(Vector3 testPos, float minDist, bool physics=false);

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

            Vector3 normal = new Vector3(SDF(pX,float.MaxValue) - SDF(mX,float.MaxValue),
                                         SDF(pY,float.MaxValue) - SDF(mY,float.MaxValue),
                                         SDF(pZ,float.MaxValue) - SDF(mZ,float.MaxValue)); 
            normal.Normalize();

            return normal;
        }

        public void Translate(Vector3 translation)
        {
            this.translateMatrix[3,0] += translation.X;
            this.translateMatrix[3,1] += translation.Y;
            this.translateMatrix[3,2] += translation.Z;

            this.transformInverse = (this.rotationMatrix * this.translateMatrix).GetInverse();

            foreach(Object obj in booleanObj)
            {
                obj.Translate(translation);
            }
        }

        public void Rotate(float angle, string axis)
        {
            float c = (float)Math.Cos(angle*(float)Math.PI/180);
            float s = (float)Math.Sin(angle*(float)Math.PI/180);

            Matrix<double> rotM;
            switch(axis.ToLower())
            {
                case "x":
                    rotM = Matrix.Create<double>(new double [,] {
                            {1, 0,0,0},
                            {0, c,s,0},
                            {0,-s,c,0},
                            {0, 0,0,1}});
                    break;
                case "y":
                    rotM = Matrix.Create<double>(new double [,] {
                            { c,0,s,0},
                            { 0,1,0,0},
                            {-s,0,c,0},
                            { 0,0,0,1}});
                    break;
                case "z":
                    rotM = Matrix.Create<double>(new double [,] {
                            { c,s,0,0},
                            {-s,c,0,0},
                            { 0,0,1,0},
                            { 0,0,0,1}});
                    break;

                default:
                    throw new Exception("Undefined rotation axis");
            }

            this.rotationMatrix *= rotM;
            this.transformInverse = (this.rotationMatrix * this.translateMatrix).GetInverse();

            foreach(Object obj in booleanObj)
            {
                obj.Rotate(angle, axis);
            }
        }
        
        protected Vector3 Transform(Vector3 orig)
        {
            /* //todo transform of dynamic objects */

            // úplně normálně se v Threadu může přidat orig z jiného vektoru
            /* this.testMatrix[0,0] = orig.X; */
            /* this.testMatrix[0,1] = orig.Y; */
            /* this.testMatrix[0,2] = orig.Z; */
            /* this.testMatrix[0,3] = 1; */

            Vector3 _output = new Vector3((float)((orig.X*this.transformInverse[0,0]) + (orig.Y*this.transformInverse[1,0]) + (orig.Z*this.transformInverse[2,0]) + (1*this.transformInverse[3,0])),
                                          (float)((orig.X*this.transformInverse[0,1]) + (orig.Y*this.transformInverse[1,1]) + (orig.Z*this.transformInverse[2,1]) + (1*this.transformInverse[3,1])),
                                          (float)((orig.X*this.transformInverse[0,2]) + (orig.Y*this.transformInverse[1,2]) + (orig.Z*this.transformInverse[2,2]) + (1*this.transformInverse[3,2])));

            return _output;

        }

        public Color Color { get => color; }
        public Vector3 Position
        {
            get 
            {
                if(this.staticObject)
                    return this.position;
                else
                    return new Vector3((float)this.translateMatrix[3,0], 
                                       (float)this.translateMatrix[3,1], 
                                       (float)this.translateMatrix[3,2]);
            }
        }

        public string Info { get => info; }
        public bool Static { get => staticObject; }
    }
}
