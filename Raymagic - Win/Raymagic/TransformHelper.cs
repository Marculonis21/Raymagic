using System;
using Microsoft.Xna.Framework;
using Extreme.Mathematics;
using Matrix = Extreme.Mathematics.Matrix;

namespace Raymagic
{
    public static class TransformHelper
    {
        public static Matrix<double> Translate(Matrix<double> matrix, Vector3 translation)
        {
            matrix[3,0] += translation.X;
            matrix[3,1] += translation.Y;
            matrix[3,2] += translation.Z;

            return matrix;
        }

        public static Matrix<double> Rotate(Matrix<double> matrix, float angle, string axis)
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

            matrix *= rotM;
            return matrix;
        }

        // https://stackoverflow.com/questions/6721544/circular-rotation-around-an-arbitrary-axis
        public static Matrix<double> Rotate(Matrix<double> matrix, float angle, Vector3 axis)
        {
            float c = (float)Math.Cos(angle*(float)Math.PI/180);
            float s = (float)Math.Sin(angle*(float)Math.PI/180);

            Vector3.Normalize(axis).Deconstruct(out float uX, out float uY, out float uZ);
            var uX2 = uX * uX; 
            var uY2 = uY * uY; 
            var uZ2 = uZ * uZ; 

            Matrix<double> rotM = Matrix.Create<double>(4,4, new double[] {
                    c + uX2*(1 - c),      uX*uY*(1 - c) - uZ*s, uX*uZ*(1 - c) + uY*s, 0,
                    uY*uX*(1 - c) + uZ*s, c + uY2*(1 - c),      uY*uZ*(1 - c) - uX*s, 0,
                    uZ*uX*(1 - c) - uY*s, uZ*uY*(1 - c) + uX*s, c + uZ2*(1 - c),      0,
                    0                   , 0                   , 0              ,      1
                    }, Extreme.Mathematics.MatrixElementOrder.RowMajor);

            matrix *= rotM;
            return matrix;
        }

        public static double[] GetInverse(Matrix<double> transformMatrix)
        {
            /* return (rotationMatrix * translationMatrix).GetInverse(); */
            Matrix<double> inverse = transformMatrix.GetInverse();
            return new double[12] { // XYZ
                inverse[0,0],
                inverse[0,1],
                inverse[0,2],
                inverse[1,0],
                inverse[1,1],
                inverse[1,2],
                inverse[2,0],
                inverse[2,1],
                inverse[2,2],
                inverse[3,0],
                inverse[3,1],
                inverse[3,2]
            };

        }
        
        public static Vector3 RepeatLimit(Vector3 pos, float c, Vector3 limits)
        {
            Vector3 _v = pos/c;
            Vector3 _out = new Vector3((float)Math.Clamp(Math.Round(_v.X), (-limits).X, limits.X),
                                       (float)Math.Clamp(Math.Round(_v.Y), (-limits).Y, limits.Y),
                                       (float)Math.Clamp(Math.Round(_v.Z), (-limits).Z, limits.Z));
            return pos-c*_out;
        }

        public static Vector3 GetRotationFromTransform(Matrix<double> transformMatrix)
        {
            var matrix = new Microsoft.Xna.Framework.Matrix((float)transformMatrix[0,0], (float)transformMatrix[0,1], (float)transformMatrix[0,2], (float)transformMatrix[0,3],
                                                            (float)transformMatrix[1,0], (float)transformMatrix[1,1], (float)transformMatrix[1,2], (float)transformMatrix[1,3],
                                                            (float)transformMatrix[2,0], (float)transformMatrix[2,1], (float)transformMatrix[2,2], (float)transformMatrix[2,3],
                                                            (float)transformMatrix[3,0], (float)transformMatrix[3,1], (float)transformMatrix[3,2], (float)transformMatrix[3,3]);

            var q = Microsoft.Xna.Framework.Quaternion.CreateFromRotationMatrix(matrix);

            /* https://stackoverflow.com/questions/70462758/c-sharp-how-to-convert-quaternions-to-euler-angles-xyz */

            float X = 0;
            float Y = 0;
            float Z = 0;

            // X
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // Y
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                Y = (float)Math.Asin(sinp);
            }

            // Z
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return new Vector3((float)(X*180/Math.PI),
                               (float)(Y*180/Math.PI),
                               (float)(Z*180/Math.PI));
            /* return new Vector3(X, */
            /*                    Y, */
            /*                    Z); */
        }

        public static Vector3 Transform(Vector3 orig, double[] transformInverse)
        {
            return new Vector3((float)((orig.X*transformInverse[0]) + (orig.Y*transformInverse[3]) + (orig.Z*transformInverse[6]) + (1*transformInverse[9])),
                               (float)((orig.X*transformInverse[1]) + (orig.Y*transformInverse[4]) + (orig.Z*transformInverse[7]) + (1*transformInverse[10])),
                               (float)((orig.X*transformInverse[2]) + (orig.Y*transformInverse[5]) + (orig.Z*transformInverse[8]) + (1*transformInverse[11])));
        }
    }
}
