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
        protected Matrix<double> baseChangeMatrixInInverse;
        protected Matrix<double> baseChangeMatrix;
        protected Matrix<double> baseChangeMatrixInverse;

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

            Console.WriteLine($"normal {normal}, right {right}, up {up}");
            var _normal = -normal;
            var _right = -right;
            this.baseChangeMatrixIn = Matrix.Create<double>(3,3, new double[] {
                    _normal.X, _normal.Y, _normal.Z,
                    _right.X, _right.Y, _right.Z,
                    up.X, up.Y, up.Z,
                    }, 
                    Extreme.Mathematics.MatrixElementOrder.ColumnMajor);
            this.baseChangeMatrixInInverse = baseChangeMatrixIn.GetInverse();

            this.baseChangeMatrix = Matrix.Create<double>(3,3, new double[] {
                    normal.X, normal.Y, normal.Z,
                    right.X, right.Y, right.Z,
                    up.X, up.Y, up.Z,
                    }, 
                    Extreme.Mathematics.MatrixElementOrder.ColumnMajor);
            this.baseChangeMatrixInverse = baseChangeMatrix.GetInverse();

            /* var inVector = Vector3.Normalize(new Vector3(0,0,1)); */
            /* Console.WriteLine(inVector); */
            /* var v = Vector.Create<double>(inVector.X, inVector.Y, inVector.Z); */

            /* var K2B = baseChangeMatrix.Solve(v); */
            /* var B2K = baseChangeMatrixInverse.Solve(v); */
            /* Console.WriteLine($"solved {K2B}"); */

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
            }
            else if (this.type == 1 && Map.instance.portalList[0] != null)
            {
                otherPortal = Map.instance.portalList[0];
                this.otherPortal.otherPortal = this;
            }
        }

        // special testing method including ray 
        public SDFout PortalSDF(Vector3 testPos, float minDist, Ray ray, int depth, bool useBounding=true, bool physics=false)
        {
            if (Vector3.Dot(ray.direction,normal) > 0) return new SDFout(float.MaxValue, Color.Pink);

            SDFout current = new SDFout(SDFDistance(Transform(testPos)), this.color);

            Color outColor = this.color;

            if (Vector3.Distance(testPos, this.Position) < portalSize-8 && !physics)
            {
                if (this.otherPortal == null) return current;

                var otherPos = otherPortal.Position;

                Vector3 translateK = this.Position - testPos;
                Vector3 dirK = ray.direction;

                var translateB = this.baseChangeMatrixIn.Solve(Vector.Create<double>(translateK.X,translateK.Y,translateK.Z));
                var dirB = this.baseChangeMatrixIn.Solve(Vector.Create<double>(dirK.X,dirK.Y,dirK.Z));
                /* // BASIS = {NORMAL, RIGHT, UP}*/

                var _translateKNew = otherPortal.baseChangeMatrixInverse.Solve(translateB);
                var _dirKNew = otherPortal.baseChangeMatrixInverse.Solve(dirB);
                /* Console.WriteLine($"refK {reflectK} ->\n refB1 {reflectB} ->\n K {reflectKNew}"); */
                /* Console.WriteLine($"dirK {dirK} ->\n dirB {dirB} ->\n K {_dirKNew}"); */

                /* return new SDFout(float.MaxValue, Color.Pink); */

                Vector3 outDir = Vector3.Normalize(_dirKNew.ToVector3());
                Vector3 translateKNew = _translateKNew.ToVector3();

                Ray outRay = new Ray((otherPos-translateKNew)+(outDir*25), outDir);

                RayMarchingHelper.RayMarch(outRay, out float _, out outColor, depth+1);
            }

            return new SDFout(current.distance, outColor);
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Portal(testPos, this.normal, this.portalSize, this.portalDepth);
        }

        /* def points_in_cylinder(pt1, pt2, r, q): */
        /*     vec = pt2 - pt1 */
        /*     const = r * np.linalg.norm(vec) */
        /*     return np.where(np.dot(q - pt1, vec) >= 0 and np.dot(q - pt2, vec) <= 0 \ */ 
        /*         and np.linalg.norm(np.cross(q - pt1, vec)) <= const) */
        public bool PosInPortal(Vector3 testPos)
        {
            var pt1 = this.Position - normal*portalDepth;
            var pt2 = this.Position; 
            var vec = pt2 - pt1;

            bool x1 = Vector3.Dot(testPos - pt1, vec) >= 0 &&
                Vector3.Dot(testPos - pt2, vec) <= 0;

            bool x2 = Vector3.Multiply(testPos - pt1, pt2 - pt1).Length()/(pt2 - pt1).Length() <= this.portalSize;
            return x1 && x2;
        }
    }
}
