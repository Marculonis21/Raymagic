using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MathNet.Numerics.LinearAlgebra;

namespace Raymagic
{
    public abstract class IObject
    {
        protected Vector3 position;
        protected Color color;

        protected bool staticObject;

        protected List<BooleanOP> booleanOp = new List<BooleanOP>();
        protected List<IObject> booleanObj = new List<IObject>();

        protected Matrix<float> translateMatrix = Matrix<float>.Build.Dense(4,4,0);
        protected Matrix<float> rotationMatrix = Matrix<float>.Build.Dense(4,4,0);
        protected Matrix<float> testMatrix = Matrix<float>.Build.Dense(1,4,1);

        public IObject()
        {
            Console.WriteLine("you better be there");
            this.translateMatrix += Matrix<float>.Build.Diagonal(4,4,1);
            this.rotationMatrix += Matrix<float>.Build.Diagonal(4,4,1);
        }

        public void AddBoolean(BooleanOP op, IObject obj)
        {
            booleanOp.Add(op);
            obj.position += this.position;
            booleanObj.Add(obj);
        }

        public abstract float SDF(Vector3 testPos);

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

        public void Translate(Vector3 position)
        {
            this.translateMatrix[3,0] += position.X;
            this.translateMatrix[3,1] += position.Y;
            this.translateMatrix[3,2] += position.Z;

            /* Console.WriteLine(this.translateMatrix.ToString()); */
        }

        public void Rotate(float angle, string axis)
        {
            float c = (float)Math.Cos(angle*(float)Math.PI/180);
            float s = (float)Math.Sin(angle*(float)Math.PI/180);

            switch(axis.ToLower())
            {
                case "x":
                    this.rotationMatrix[1,1] += c;
                    this.rotationMatrix[1,2] += s;
                    this.rotationMatrix[2,1] += -s;
                    this.rotationMatrix[2,2] += c;
                    break;
                case "y":
                    this.rotationMatrix[0,0] += c;
                    this.rotationMatrix[0,2] += s;
                    this.rotationMatrix[2,0] += -s;
                    this.rotationMatrix[2,2] += c;
                    break;
                case "z":
                    this.rotationMatrix[0,0] = c;
                    this.rotationMatrix[0,1] = s;
                    this.rotationMatrix[1,0] = -s;
                    this.rotationMatrix[1,1] = c;
                    break;

                default:
                    throw new Exception("Undefined rotation axis");
            }
            /* Console.WriteLine(this.rotationMatrix.ToString()); */
        }
        
        protected Matrix<float> _testMatrix = Matrix<float>.Build.Dense(1,4,1);
        protected Matrix<float> _translateMatrix = Matrix<float>.Build.Dense(4,4,0);
        protected Matrix<float> _rotationMatrix = Matrix<float>.Build.Dense(4,4,0);

        protected Vector3 Transform(Vector3 orig)
        {
            //todo transform of dynamic objects
            this._testMatrix[0,0] = orig.X;
            this._testMatrix[0,1] = orig.Y;
            this._testMatrix[0,2] = orig.Z;
            this._testMatrix[0,3] = 1;

            this._translateMatrix[0,0] = 1;
            this._translateMatrix[1,1] = 1;
            this._translateMatrix[2,2] = 1;
            this._translateMatrix[3,3] = 1;

            this._translateMatrix[3,0] = 400;
            this._translateMatrix[3,1] = 200; 
            this._translateMatrix[3,2] = 75;

            float c = (float)Math.Cos(45*(float)Math.PI/180);
            float s = (float)Math.Sin(45*(float)Math.PI/180);

            this._rotationMatrix[0,0] = c;
            this._rotationMatrix[0,1] = s;
            this._rotationMatrix[1,0] = -s;
            this._rotationMatrix[1,1] = c;

            this._rotationMatrix[2,2] = 1;
            this._rotationMatrix[3,3] = 1;

            Matrix<float> output = this._testMatrix * (this._rotationMatrix * this._translateMatrix).Inverse();
            Vector3 outV = new Vector3(output[0,0],output[0,1],output[0,2]);

            return outV;
        }

        public Color GetColor()
        {
            return color;
        }
    }
}
