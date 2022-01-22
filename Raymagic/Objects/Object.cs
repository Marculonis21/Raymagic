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

        protected Box boundingBox;
        protected Vector3 boundingBoxSize;
        protected bool boundingBoxVisible = false;

        protected bool staticObject;

        protected List<BooleanOP> booleanOp = new List<BooleanOP>();
        protected List<Object> booleanObj = new List<Object>();

        protected Matrix<double> translateMatrix = Matrix.Create<double>(4,4);
        protected Matrix<double> rotationMatrix = Matrix.Create<double>(4,4);
        protected Matrix<double> transformInverse = Matrix.Create<double>(4,4);

        public Object(Vector3 position, Color color, bool staticObject, Vector3 boundingBoxSize, string info)
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
            this.info = info;

            if(!staticObject)
            {
                this.Translate(this.position);
                this.position = new Vector3();
            }

            if(boundingBoxSize.X != 0)
            {
                this.boundingBox = new Box(position, 
                                           boundingBoxSize, 
                                           Color.Black);

                this.boundingBoxSize = boundingBoxSize;
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

                obj.rotationMatrix = this.rotationMatrix;

                obj.position = objPos;
                obj.Translate(origPos);
            }
            
            booleanObj.Add(obj);
        }

        public float SDF(Vector3 testPos, float minDist, bool useBounding=true, bool physics=false)
        {
            float dst = float.MaxValue;
            Vector3 tPos = this.staticObject ? testPos : Transform(testPos);

            /* if(!SDFBoundCheck(tPos,minDist,useBounding,physics, out dst)) return dst; */

            dst = SDFDistance(tPos);
            dst = SDFBooleans(dst, testPos, minDist, useBounding, physics);

            return dst;
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

        private float SDFBooleans(float dst, Vector3 testPos, float minDist, bool useBounding=true, bool physics=false)
        {
            for(int i = 0; i < this.booleanObj.Count; i++)
            {
                switch(this.booleanOp[i])
                {
                    case BooleanOP.DIFFERENCE:
                        dst = SDFs.BooleanDifference(dst, this.booleanObj[i].SDF(testPos,minDist,false,physics));
                        break;
                    case BooleanOP.INTERSECT:
                        dst = SDFs.BooleanIntersect(dst, this.booleanObj[i].SDF(testPos,minDist,false,physics));
                        break;
                    case BooleanOP.UNION:
                        dst = SDFs.BooleanUnion(dst, this.booleanObj[i].SDF(testPos,minDist,false,physics));
                        break;
                    case BooleanOP.SDIFFERENCE:
                        dst = SDFs.opSmoothSubtraction(dst, this.booleanObj[i].SDF(testPos,minDist,false,physics), k:0.2f);
                        break;
                    case BooleanOP.SINTERSECT:
                        dst = SDFs.opSmoothIntersection(dst, this.booleanObj[i].SDF(testPos,minDist,false,physics), k:0.2f);
                        break;
                    case BooleanOP.SUNION:
                        dst = SDFs.opSmoothUnion(dst, this.booleanObj[i].SDF(testPos,minDist,false,physics), k:20f);
                        break;
                    default: 
                        throw new Exception("Unknown boolean operation!");
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

            Vector3 normal = new Vector3(SDF(pX,float.MaxValue) - SDF(mX,float.MaxValue),
                                         SDF(pY,float.MaxValue) - SDF(mY,float.MaxValue),
                                         SDF(pZ,float.MaxValue) - SDF(mZ,float.MaxValue)); 
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
        public bool IsStatic { get => staticObject; }

        public Box BoundingBox { get => boundingBox; } 
        public Vector3 BoundingBoxSize { get => boundingBoxSize; }
    }
}
