using System;
using Microsoft.Xna.Framework;
using Extreme.Mathematics;
using Matrix = Extreme.Mathematics.Matrix;

namespace Raymagic
{
    public class Portal : Object
    {
        protected Vector3 fNormal = new Vector3(1,0,0);
        protected Vector3 fRight =  new Vector3(0,1,0);
        protected Vector3 fUp =     new Vector3(0,0,1);

        protected Vector3 normal;
        protected Vector3 right;
        protected Vector3 up;

        protected Matrix<double> baseChangeMatrix;
        protected Matrix<double> baseChangeMatrixInverse;

        int type;
        float portalSize = 50; 

        public Portal(Vector3 center, Vector3 normal, int type) : base(center, Color.Black, false, new Vector3(), "", BooleanOP.NONE, 0, false)
        {
            this.normal = normal;

            if (normal == new Vector3(0,0,1) || normal == new Vector3(0,0,-1))
            {
                this.up = normal.Z * Vector3.Normalize(Player.instance.lookDir * new Vector3(1,1,0));
                this.right = Vector3.Normalize(Vector3.Cross(up,normal));
            }
            else
            {
                this.right = Vector3.Normalize(Vector3.Cross(new Vector3(0,0,1), normal));
                this.up = Vector3.Normalize(Vector3.Cross(normal, right));
            }

            Console.WriteLine($"normal {normal}, right {right}, up {up}");
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
        }

        // special testing method including ray 
        public SDFout PortalSDF(Vector3 testPos, float minDist, Ray ray, int depth, bool useBounding=true, bool physics=false)
        {
            if (Vector3.Dot(ray.direction,normal) > 0) return new SDFout(float.MaxValue, Color.Pink);

            SDFout current = new SDFout(SDFDistance(Transform(testPos)), this.color);

            Color outColor = this.color;
            if (Vector3.Distance(testPos, this.Position) < portalSize-8)
            {

                Portal otherPortal = null;

                if (this.type == 0 && Map.instance.portalList[1] != null)
                {
                    otherPortal = Map.instance.portalList[1];
                }
                else if (this.type == 1 && Map.instance.portalList[0] != null)
                {
                    otherPortal = Map.instance.portalList[0];
                }

                if (otherPortal == null) return current;

                var otherPos = otherPortal.Position;

                Vector3 translateK = this.Position - testPos;
                Vector3 reflectK = Vector3.Normalize(Vector3.Reflect(ray.direction, this.normal));

                var reflectB = this.baseChangeMatrix.Solve(Vector.Create<double>(reflectK.X,reflectK.Y,reflectK.Z));
                var translateB = this.baseChangeMatrix.Solve(Vector.Create<double>(translateK.X,translateK.Y,translateK.Z));
                /* // BASIS = {NORMAL, RIGHT, UP}*/

                var reflectKNew = otherPortal.baseChangeMatrixInverse.Solve(reflectB);
                var _translateKNew = otherPortal.baseChangeMatrixInverse.Solve(translateB);
                /* Console.WriteLine($"refK {reflectK} ->\n refB1 {reflectB} ->\n K {reflectKNew}"); */

                /* return new SDFout(float.MaxValue, Color.Pink); */

                Vector3 outDir = Vector3.Normalize(new Vector3((float)reflectKNew[0], (float)reflectKNew[1], (float)reflectKNew[2]));
                Vector3 translateKNew = new Vector3((float)_translateKNew[0], (float)_translateKNew[1], (float)_translateKNew[2]);

                /* Ray outRay = new Ray(otherPos-translate+(outDir*25), outDir); */
                Ray outRay = new Ray((otherPos-translateKNew)+(outDir*25), outDir);
                RayMarchingHelper.RayMarch(outRay, out float _, out outColor, depth+1);
            }

            return new SDFout(current.distance, outColor);
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Portal(testPos, this.normal, this.portalSize);
        }
    }
}
