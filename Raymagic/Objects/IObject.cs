using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
/* using MathNet.Numerics.LinearAlgebra; */
/* using MathNet.Numerics.LinearAlgebra.Storage; */
using Extreme.Mathematics;
using Matrix = Extreme.Mathematics.Matrix;

namespace Raymagic
{
    public abstract class IObject
    {
        protected Vector3 position;
        protected Color color;

        protected bool staticObject;

        protected List<BooleanOP> booleanOp = new List<BooleanOP>();
        protected List<IObject> booleanObj = new List<IObject>();

        protected Matrix<double> translateMatrix = Matrix.Create<double>(4,4);
        protected Matrix<double> rotationMatrix = Matrix.Create<double>(4,4);
        protected Matrix<double> transformInverse = Matrix.Create<double>(4,4);

        public IObject()
        {
            this.translateMatrix[0,0] = 1;
            this.translateMatrix[1,1] = 1;
            this.translateMatrix[2,2] = 1;
            this.translateMatrix[3,3] = 1;

            this.rotationMatrix[0,0] = 1;
            this.rotationMatrix[1,1] = 1;
            this.rotationMatrix[2,2] = 1;
            this.rotationMatrix[3,3] = 1;
        }

        public void AddBoolean(BooleanOP op, IObject obj)
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

        public void Translate(Vector3 translation)
        {
            this.translateMatrix[3,0] += translation.X;
            this.translateMatrix[3,1] += translation.Y;
            this.translateMatrix[3,2] += translation.Z;

            this.transformInverse = (this.rotationMatrix * this.translateMatrix).GetInverse();

            foreach(IObject obj in booleanObj)
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

            foreach(IObject obj in booleanObj)
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
            
            /* Matrix<float> _orig = Matrix<float>.Build.Dense(1,4,new float[] {orig.X, orig.Y, orig.Z, 1}); */
            var _orig = Matrix.Create<double>(new double[,]{{orig.X, orig.Y, orig.Z, 1}});

            /* Matrix<double> output = Matrix.Multiply(_orig, this.transformInverse); */

            Vector3 _output = new Vector3((float)((_orig[0,0]*this.transformInverse[0,0]) + (_orig[0,1]*this.transformInverse[1,0]) + (_orig[0,2]*this.transformInverse[2,0]) + (_orig[0,3]*this.transformInverse[3,0])),
                                          (float)((_orig[0,0]*this.transformInverse[0,1]) + (_orig[0,1]*this.transformInverse[1,1]) + (_orig[0,2]*this.transformInverse[2,1]) + (_orig[0,3]*this.transformInverse[3,1])),
                                          (float)((_orig[0,0]*this.transformInverse[0,2]) + (_orig[0,1]*this.transformInverse[1,2]) + (_orig[0,2]*this.transformInverse[2,2]) + (_orig[0,3]*this.transformInverse[3,2])));

            /* Console.WriteLine($"FIRST: {output}\nSECOND: {_output}"); */
            
            return _output;

            /* return new Vector3((float)output[0,0],(float)output[0,1],(float)output[0,2]); */
        }

        public Color GetColor()
        {
            return color;
        }
    }
}
