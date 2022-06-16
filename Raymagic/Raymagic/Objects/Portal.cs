using System;
using Microsoft.Xna.Framework;
using Extreme.Mathematics;
using Matrix = Extreme.Mathematics.Matrix;

namespace Raymagic
{
    public class Portal : Object
    {
        protected Vector3 normal;
        protected Vector3 up;
        protected Vector3 right;

        protected Matrix<double> baseChangeMatrix;
        protected Matrix<double> baseChangeMatrixInverse;

        int type;
        float portalSize = 50; 

        public Portal(Vector3 center, Vector3 normal, int type) : base(center, Color.Black, false, new Vector3(), "", BooleanOP.NONE, 0, false)
        {
            this.normal = normal;

            if (normal == new Vector3(0,0,1) || normal == new Vector3(0,0,-1))
            {
                this.right = new Vector3(1,0,0);
                this.up = new Vector3(0,1,0);
            }
            else
            {
                this.right = Vector3.Normalize(Vector3.Cross(new Vector3(0,0,1), normal));
                this.up = Vector3.Normalize(Vector3.Cross(normal, right));
            }

            Console.WriteLine($"normal {normal}, right {right}, up {up}");
            this.baseChangeMatrix = Matrix.Create<double>(3,3, new double[] {
                    normal.X, normal.Y, normal.Z,
                    up.X, up.Y, up.Z,
                    right.X, right.Y, right.Z,
                    }, 
                    Extreme.Mathematics.MatrixElementOrder.ColumnMajor);
            this.baseChangeMatrixInverse = baseChangeMatrix.GetInverse();

            /* var v = Vector.Create<double>(inVector.X, inVector.Y, inVector.Z); */

            /* var k2B = baseChangeMatrix.Solve(v); */
            /* var B2k = baseChangeMatrixInverse.Solve(k2B); */

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
                if (this.type == 0 && Map.instance.portalList[1] != null)
                {
                    var otherPortal = Map.instance.portalList[1];
                    var otherPos = otherPortal.Position;
                    var otherNormal = otherPortal.normal;
                    var translate = this.Position - testPos;

                    Vector3 reflectDir = Vector3.Reflect(ray.direction, this.normal);

                    /* /1* Console.WriteLine($"ro: {reflectDir}"); *1/ */
                    var reflectB1 = this.baseChangeMatrix.Solve(Vector.Create<double>(reflectDir.X,reflectDir.Y,reflectDir.Z));
                    /* /1* Console.WriteLine($"rn: {reflectB1}"); *1/ */
                    /* // BASIS = {NORMAL, UP, RIGTH} */

                    /* var _n = otherPortal.normal; */
                    /* var _r = otherPortal.right; */
                    /* var _u = otherPortal.up; */
                    /* var vector = Vector3.Normalize(_n * (float)reflectB1[0] + _r * (float)reflectB1[1] + _u * (float)reflectB1[2]); */
                    /* var reflectKanon = otherPortal.baseChangeMatrixInverse.Solve(Vector.Create<double>(vector.X,vector.Y,vector.Z)); */
                    /* Console.WriteLine($"ref {reflectDir} -> B1 {reflectB1} -> vector {vector} -> K {reflectKanon}"); */
                    return new SDFout(float.MaxValue, Color.Pink);

                    /* Vector3 outDir = Vector3.Normalize(new Vector3((float)reflectKanon[0], (float)reflectKanon[1], (float)reflectKanon[2])); */

                    /* Ray outRay = new Ray((otherPos-translate)+outDir*25, outDir); */

                    /* RayMarchingHelper.RayMarch(outRay, out float _, out outColor, depth+1); */
                }
                else if (this.type == 1 && Map.instance.portalList[0] != null)
                {
                    var otherPortal = Map.instance.portalList[0];
                    var otherPos = otherPortal.Position;
                    var otherNormal = otherPortal.normal;
                    var translate = this.Position - testPos;

                    Vector3 reflectDir = Vector3.Reflect(ray.direction, this.normal);

                    /* var reflectB1 = this.baseChangeMatrix.Solve(Vector.Create<double>(reflectDir.X,reflectDir.Y,reflectDir.Z)); */
                    /* // BASIS = {NORMAL, RIGHT, UP} */
                    /* var _n = otherPortal.normal; */
                    /* var _r = otherPortal.right; */
                    /* var _u = otherPortal.up; */
                    /* var vector = Vector3.Normalize(_n * (float)reflectB1[0] + _r * (float)reflectB1[1] + _u * (float)reflectB1[2]); */
                    /* var reflectKanon = otherPortal.baseChangeMatrixInverse.Solve(Vector.Create<double>(vector.X,vector.Y,vector.Z)); */

                    /* Vector3 outDir = Vector3.Normalize(new Vector3((float)reflectKanon[0], (float)reflectKanon[1], (float)reflectKanon[2])); */

                    /* Ray outRay = new Ray((otherPos-translate)+outDir*25, outDir); */

                    /* RayMarchingHelper.RayMarch(outRay, out float _, out outColor, depth+1); */

                    /* Vector3 outDir = Vector3.Reflect(ray.direction, this.normal); */
                    /* Ray outRay = new Ray((otherPos-translate)+outDir*25, outDir); */

                    /* Ray outRay = new Ray((otherPos-translate)+outDir*25, outDir); */

                    /* RayMarchingHelper.RayMarch(outRay, out float _, out outColor, depth+1); */
                }
            }

            return new SDFout(current.distance, outColor);
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Portal(testPos, this.normal, this.portalSize);
        }
    }
}
