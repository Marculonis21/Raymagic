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

        protected bool transparent = false;

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

        public bool symmetryEnabled = false;
        public string symmetryOptions = "";
        public Vector3 symmetryPlaneOffset = new Vector3();

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
            else
            {
                child.childRelativePos = this.Position - child.Position;
            }

            childObjects.Add(child);
        }

        public virtual SDFout SDF(Vector3 testPos, float minDist, out bool IsTransparent)
        {
            IsTransparent = this.transparent;
            Vector3 transformedTestPos = Transform(testPos);    

            if (repetitionEnabled)
            {
                transformedTestPos = TransformHelper.RepeatLimit(transformedTestPos, repetitionDistance, repetitionLimit);
            }

            if (symmetryEnabled)
            {
                transformedTestPos.Deconstruct(out float x, out float y, out float z);
                if (symmetryOptions.Contains("X")) x = Math.Abs(x) - symmetryPlaneOffset.X;
                if (symmetryOptions.Contains("Y")) y = Math.Abs(y) - symmetryPlaneOffset.Y;
                if (symmetryOptions.Contains("Z")) z = Math.Abs(z) - symmetryPlaneOffset.Z;

                transformedTestPos = new Vector3(x,y,z);
            }

            SDFout current = new SDFout(SDFDistance(transformedTestPos), this.color);

            foreach (Object child in childObjects)
            {
                // need to pass the original (not transformed) testPos
                var childOut = child.SDF(testPos, minDist, out bool childIsTransparent);

                var _out = SDFs.Combine(current.distance, childOut.distance, current.color, childOut.color, child.booleanOP, child.booleanStrength);
                if (childOut.distance == _out.distance)
                {
                    IsTransparent = childIsTransparent;
                }

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

            Vector3 normal = new Vector3(SDF(pX,float.MaxValue, out _).distance - SDF(mX,float.MaxValue,out _).distance,
                                         SDF(pY,float.MaxValue, out _).distance - SDF(mY,float.MaxValue,out _).distance,
                                         SDF(pZ,float.MaxValue, out _).distance - SDF(mZ,float.MaxValue,out _).distance); 
            normal.Normalize();

            return normal;
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

        public void SetSymmetry(string symmetryOptions, Vector3 symmetryPlaneOffset)
        {
            this.symmetryEnabled = true;
            this.symmetryOptions = symmetryOptions;
            this.symmetryPlaneOffset = symmetryPlaneOffset;

            foreach (Object child in childObjects)
            {
                child.SetSymmetry(symmetryOptions, symmetryPlaneOffset);
            }
        }

        public void SetTransparent(bool transparent)
        {
            this.transparent = transparent;
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
        public Vector3 Rotation
        {
            get
            {
                return TransformHelper.GetRotationFromTransform(transformMatrix);
            }
        }

        public string Info { get => info; }
        public bool IsSelectable { get => selectable; }
        public Box BoundingBox { get => boundingBox; } 
        public Vector3 BoundingBoxSize { get => boundingBoxSize; }
        public bool IsTransparent { get => transparent; }
    }
}
