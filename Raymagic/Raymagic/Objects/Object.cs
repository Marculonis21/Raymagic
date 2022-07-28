using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Extreme.Mathematics;
using Matrix = Extreme.Mathematics.Matrix;

namespace Raymagic
{
    public abstract class Object
    {
        protected Matrix<double> transformMatrix = Matrix.Create<double>(4,4);

        // protected Matrix<double> transformInverse = Matrix.Create<double>(4,4);
        // transformInverseMatrix put to an array (for some reason those matrices have slower indexing) 
        // -> faster Transform()
        protected double[] inverse = new double[12]; 

        protected Vector3 childRelativePos;

        protected Color color;
        protected string info;

        protected Box boundingBox;
        protected Vector3 boundingBoxSize;
        protected bool boundingBoxVisible = false;

        public List<Object> childObjects {get; protected set;}
        protected BooleanOP booleanOP;
        protected float booleanStrength;

        protected bool selectable; 

        protected bool repetitionEnabled = false;
        protected Vector3 repetitionLimit = new Vector3();
        protected float repetitionDistance= 1;

        public Object(Vector3 position, Color color, Vector3 boundingBoxSize, string info, BooleanOP booleanOP, float opStrength, bool selectable)
        {
            this.transformMatrix[0,0] = 1;
            this.transformMatrix[1,1] = 1;
            this.transformMatrix[2,2] = 1;
            this.transformMatrix[3,3] = 1;

            this.color = color;
            this.selectable = selectable;
            this.info = info;
            this.childObjects = new List<Object>();

            this.Translate(position);
            this.childRelativePos = position;
            this.booleanOP = booleanOP;
            this.booleanStrength = opStrength;

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
                child.transformMatrix = this.transformMatrix.Clone();
                child.inverse = TransformHelper.GetInverse(child.transformMatrix);
                child.Translate(child.childRelativePos);
            }

            childObjects.Add(child);
        }

        public void SetRepetition(Vector3 repetitionLimit, float repetitionDistance)
        {
            this.repetitionEnabled = true;
            this.repetitionLimit = repetitionLimit;
            this.repetitionDistance = repetitionDistance;

            foreach (Object child in childObjects)
            {
                child.SetRepetition(repetitionLimit, repetitionDistance);
            }
        }

        public virtual SDFout SDF(Vector3 testPos, float minDist, bool physics=false)
        {
            Vector3 transformedTestPos = Transform(testPos);

            if (repetitionEnabled)
            {
                transformedTestPos = TransformHelper.RepeatLimit(transformedTestPos, repetitionDistance, repetitionLimit);
            }

            SDFout current = new SDFout(SDFDistance(transformedTestPos), this.color);

            foreach (Object child in childObjects)
            {
                // need to pass the original (not transformed) testPos
                
                var childOut = child.SDF(testPos, minDist, physics);
                var _out = SDFs.Combine(current.distance, childOut.distance, current.color, childOut.color, child.booleanOP, child.booleanStrength);

                current = _out;
            }

            return current;
        }

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

        public void TranslateAbsolute(Vector3 newPosition, bool propagateToChildren=true)
        {
            Vector3 diff = newPosition - this.Position;

            this.transformMatrix = TransformHelper.Translate(transformMatrix, diff);
            this.inverse = TransformHelper.GetInverse(this.transformMatrix);

            if (propagateToChildren)
            {
                foreach(Object obj in childObjects)
                {
                    obj.TranslateAbsolute(newPosition+obj.childRelativePos, propagateToChildren);
                }
            }
        }

        public void Translate(Vector3 translation, bool propagateToChildren=true)
        {
            this.transformMatrix = TransformHelper.Translate(transformMatrix, translation);
            this.inverse = TransformHelper.GetInverse(transformMatrix);

            if (propagateToChildren)
            {
                foreach(Object obj in childObjects)
                {
                    obj.Translate(translation, propagateToChildren);
                }
            }
        }

        public void Rotate(float angle, string axis, Vector3 pivotPosition)
        {
            if (pivotPosition == Vector3.Zero)
            {
                this.transformMatrix = TransformHelper.Rotate(this.transformMatrix, angle, axis);
            }
            else
            {
                Translate(-pivotPosition);
                this.transformMatrix = TransformHelper.Rotate(this.transformMatrix, angle, axis);
                Translate(pivotPosition);
            }
            this.inverse = TransformHelper.GetInverse(transformMatrix);

            foreach(Object obj in childObjects)
            {
                obj.Rotate(angle, axis, pivotPosition);
            }
        }

        public void Rotate(float angle, Vector3 axis, Vector3 pivotPosition)
        {
            if (pivotPosition == Vector3.Zero)
            {
                this.transformMatrix = TransformHelper.Rotate(this.transformMatrix, angle, axis);
            }
            else
            {
                Translate(-pivotPosition);
                this.transformMatrix = TransformHelper.Rotate(this.transformMatrix, angle, axis);
                Translate(pivotPosition);
            }
            this.inverse = TransformHelper.GetInverse(transformMatrix);

            foreach(Object obj in childObjects)
            {
                obj.Rotate(angle, axis, pivotPosition);
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
                return new Vector3((float)this.transformMatrix[3,0], 
                                   (float)this.transformMatrix[3,1], 
                                   (float)this.transformMatrix[3,2]);
            }
        }

        public string Info { get => info; }
        public bool IsSelectable { get => selectable; }
        public Box BoundingBox { get => boundingBox; } 
        public Vector3 BoundingBoxSize { get => boundingBoxSize; }
    }
}
