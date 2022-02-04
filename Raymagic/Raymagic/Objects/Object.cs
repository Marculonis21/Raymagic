using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Extreme.Mathematics;
using Matrix = Extreme.Mathematics.Matrix;

namespace Raymagic
{
    public abstract class Object
    {
        protected Matrix<double> translationMatrix = Matrix.Create<double>(4,4);
        protected Matrix<double> rotationMatrix = Matrix.Create<double>(4,4);
        protected Matrix<double> transformInverse = Matrix.Create<double>(4,4);

        // transformInverseMatrix put to an array (for some reason those matrices have slower indexing) 
        // -> faster Transform()
        protected double[] inverse = new double[12]; 

        protected Color color;
        protected string info;

        protected Box boundingBox;
        protected Vector3 boundingBoxSize;
        protected bool boundingBoxVisible = false;

        protected bool staticObject;

        /* protected List<BooleanOP> booleanOp = new List<BooleanOP>(); */
        /* protected List<Object> booleanObj = new List<Object>(); */

        protected List<Object> childObjects = new List<Object>();
        protected BooleanOP booleanOP;
        protected float booleanStrength;

        public Object(Vector3 position, Color color, bool staticObject, Vector3 boundingBoxSize, string info, BooleanOP booleanOP, float strength)
        {
            this.translationMatrix[0,0] = 1;
            this.translationMatrix[1,1] = 1;
            this.translationMatrix[2,2] = 1;
            this.translationMatrix[3,3] = 1;

            this.rotationMatrix[0,0] = 1;
            this.rotationMatrix[1,1] = 1;
            this.rotationMatrix[2,2] = 1;
            this.rotationMatrix[3,3] = 1;

            this.color = color;
            this.staticObject = staticObject;
            this.info = info;

            this.Translate(position);
            this.booleanOP = booleanOP;
            this.booleanStrength = strength;

            if(boundingBoxSize.X != 0)
            {
                this.boundingBox = new Box(position, 
                                           boundingBoxSize, 
                                           Color.Black);

                this.boundingBoxSize = boundingBoxSize;
            }
        }

        public void AddChildObject(Object child, bool relativeTransform=false)
        {
            if(relativeTransform)
            {
                child.Translate(this.Position);
                child.rotationMatrix = this.rotationMatrix;
            }

            childObjects.Add(child);
        }

        public SDFout SDF(Vector3 testPos, float minDist, bool useBounding=true, bool physics=false)
        {
            /* float dst = float.MaxValue; */
            /* Color color = Color.Pink; */
            SDFout current = new SDFout(SDFDistance(Transform(testPos)), this.color);

            foreach (Object child in childObjects)
            {
                // need to pass the original (not transformed) testPos
                var childOut = child.SDF(testPos, minDist, useBounding, physics);
                var _out = SDFs.Combine(current.distance, childOut.distance, current.color, childOut.color, child.booleanOP, child.booleanStrength);

                current = _out;
            }

            return current;
        }

        /* private bool SDFBoundCheck(Vector3 testPos, float minDist, bool useBounding, bool physics, out float dst) */
        /* { */
        /*     dst = 0; */
        /*     if(useBounding && !this.staticObject && !physics) */
        /*     { */
        /*         if(minDist <= SDFs.Box(testPos, this.position, this.boundingBoxSize)) */
        /*         { */
        /*             dst = minDist + 1; */
        /*             return false; */
        /*         } */
        /*     } */

        /*     return true; */
        /* } */

        public abstract float SDFDistance(Vector3 testPos);

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

            Vector3 normal = new Vector3(SDF(pX,float.MaxValue).distance - SDF(mX,float.MaxValue).distance,
                                         SDF(pY,float.MaxValue).distance - SDF(mY,float.MaxValue).distance,
                                         SDF(pZ,float.MaxValue).distance - SDF(mZ,float.MaxValue).distance); 
            normal.Normalize();

            return normal;
        }

        public void DisplayBoundingBox()
        {
            if(this.boundingBoxVisible) return;
            if(this.boundingBoxSize.X == 0) return;

            this.boundingBoxVisible = true;

            Map.instance.infoObjectList.Add(this.boundingBox);
        }

        public void HideBoundingBox()
        {
            if(!this.boundingBoxVisible) return;
            if(this.boundingBoxSize.X == 0) return;

            this.boundingBoxVisible = false;
            Map.instance.infoObjectList.Remove(this.boundingBox);
        }

        public void Translate(Vector3 translation)
        {
            this.translationMatrix = TransformHelper.Translate(translationMatrix, translation);
            this.inverse = TransformHelper.GetInverse(translationMatrix, rotationMatrix);

            foreach(Object obj in childObjects)
            {
                obj.Translate(translation);
            }
        }

        public void Rotate(float angle, string axis)
        {
            this.rotationMatrix = TransformHelper.Rotate(this.rotationMatrix, angle, axis);
            this.inverse = TransformHelper.GetInverse(translationMatrix, rotationMatrix);

            foreach(Object obj in childObjects)
            {
                obj.Rotate(angle, axis);
            }
        }
        
        protected Vector3 Transform(Vector3 orig)
        {
            return TransformHelper.Transform(orig, inverse);
        }

        public Color Color { get => color; }
        public Vector3 Position
        {
            get 
            {
                return new Vector3((float)this.translationMatrix[3,0], 
                                   (float)this.translationMatrix[3,1], 
                                   (float)this.translationMatrix[3,2]);
            }
        }

        public string Info { get => info; }
        public bool IsStatic { get => staticObject; }
        public Box BoundingBox { get => boundingBox; } 
        public Vector3 BoundingBoxSize { get => boundingBoxSize; }
    }
}