using System;
using Microsoft.Xna.Framework;
using Extreme.Mathematics;
using Matrix = Extreme.Mathematics.Matrix;

namespace Raymagic
{
    public partial class Portal : Object
    {
        protected Vector3 fNormal = new Vector3(1,0,0);
        protected Vector3 fRight =  new Vector3(0,1,0);
        protected Vector3 fUp =     new Vector3(0,0,1);

        protected Vector3 normal;
        protected Vector3 right;
        protected Vector3 up;

        protected Matrix<double> baseChangeMatrixIn;
        /* protected Matrix<double> baseChangeMatrixInInverse; */
        /* protected Matrix<double> baseChangeMatrixOut; */
        protected Matrix<double> baseChangeMatrixOutInverse;

        protected Matrix<double> baseChangeTransformation;

        int type;
        float portalSize = 50; 
        float portalDepth = 80; 

        public Portal otherPortal {get; protected set;}

        protected enum State
        {
            READY,
            REACENTLYUSED,
        }
        protected State portalState;
        public int cooldownCounter;

        public Portal(Vector3 center, Vector3 normal, int type) : base(center, Color.Black, new Vector3(), "", BooleanOP.NONE, 0, false)
        {
            this.normal = normal;
            this.portalState = State.READY;

            if (normal == new Vector3(0,0,1) || normal == new Vector3(0,0,-1))
            {
                this.up = Vector3.Normalize(Player.instance.lookDir * new Vector3(1,1,0));
                this.right = Vector3.Normalize(Vector3.Cross(up, normal));
            }
            else
            {
                this.right = Vector3.Normalize(Vector3.Cross(new Vector3(0,0,1), normal));
                this.up = Vector3.Normalize(Vector3.Cross(normal, right));
            }

            var _normal = -normal;
            var _right = -right;
            this.baseChangeMatrixIn = Matrix.Create<double>(3,3, new double[] {
                    _normal.X, _normal.Y, _normal.Z,
                    _right.X, _right.Y, _right.Z,
                    up.X, up.Y, up.Z,
                    }, 
                    Extreme.Mathematics.MatrixElementOrder.ColumnMajor);

            var baseChangeMatrixOut = Matrix.Create<double>(3,3, new double[] {
                    normal.X, normal.Y, normal.Z,
                    right.X, right.Y, right.Z,
                    up.X, up.Y, up.Z,
                    }, 
                    Extreme.Mathematics.MatrixElementOrder.ColumnMajor);
            this.baseChangeMatrixOutInverse = baseChangeMatrixOut.GetInverse();

            this.type = type;

            if (type == 0)
            {
                this.color = Color.Orange;
            }
            else
            {
                this.color = Color.Blue;
            }

            if (this.type == 0 && Map.instance.portalList[1] != null)
            {
                this.otherPortal = Map.instance.portalList[1];
                this.otherPortal.otherPortal = this;

                this.baseChangeTransformation = this.baseChangeMatrixIn*this.otherPortal.baseChangeMatrixOutInverse;
                this.otherPortal.baseChangeTransformation = this.otherPortal.baseChangeMatrixIn*this.baseChangeMatrixOutInverse;
            }
            else if (this.type == 1 && Map.instance.portalList[0] != null)
            {
                otherPortal = Map.instance.portalList[0];
                this.otherPortal.otherPortal = this;

                this.baseChangeTransformation = this.baseChangeMatrixIn*this.otherPortal.baseChangeMatrixOutInverse;

                this.otherPortal.baseChangeTransformation = this.otherPortal.baseChangeMatrixIn*this.baseChangeMatrixOutInverse;
            }
        }

        // special SDF method including ray through portal propagation
        public SDFout PortalSDF(Vector3 testPos, float minDist, Ray ray, int depth, bool physics=false)
        {
            if (Vector3.Dot(ray.direction,normal) > 0) return new SDFout(float.MaxValue, Color.Pink);

            SDFout current = new SDFout(SDFDistance(Transform(testPos)), this.color);

            Color outColor = this.color;

            if (Vector3.Distance(testPos, this.Position) < portalSize-8 && !physics)
            {
                if (this.otherPortal == null || Map.instance.portalList[this.type == 0 ? 1 : 0] == null) return current;

                var otherPos = otherPortal.Position;

                Vector3 translateK = this.Position - testPos;
                Vector3 dirK = ray.direction;

                // solving vector space base change
                /* // BASIS = {NORMAL, RIGHT, UP}*/
                
                //// V1
                /* var translateB = this.baseChangeMatrixIn.Solve(Vector.Create<double>(translateK.X,translateK.Y,translateK.Z)); */
                /* var dirB = this.baseChangeMatrixIn.Solve(Vector.Create<double>(dirK.X,dirK.Y,dirK.Z)); */

                /* var _translateKNew = otherPortal.baseChangeMatrixOutInverse.Solve(translateB); */
                /* var _dirKNew = otherPortal.baseChangeMatrixOutInverse.Solve(dirB); */

                /* Vector3 outDir = Vector3.Normalize(_dirKNew.ToVector3()); */
                /* Vector3 translateKNew = _translateKNew.ToVector3(); */

                //// V2
                Vector3 outDir = this.baseChangeTransformation.Solve(Vector.Create<double>(dirK.X, dirK.Y, dirK.Z)).ToVector3();
                Vector3 translateKNew = this.baseChangeTransformation.Solve(Vector.Create<double>(translateK.X,translateK.Y,translateK.Z)).ToVector3();

                /* return new SDFout(float.MaxValue, Color.Pink); */

                Ray outRay = new Ray((otherPos-translateKNew)+(outDir*25), outDir);

                RayMarchingHelper.RayMarch(outRay, out float _, out outColor, depth+1);
            }

            return new SDFout(current.distance, outColor);
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Portal(testPos, this.normal, this.portalSize, this.portalDepth);
        }
    }
}
