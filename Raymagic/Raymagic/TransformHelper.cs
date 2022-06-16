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

        public static double[] GetInverse(Matrix<double> translationMatrix, Matrix<double> rotationMatrix)
        {
            /* return (rotationMatrix * translationMatrix).GetInverse(); */
            Matrix<double> inverse = (rotationMatrix * translationMatrix).GetInverse();
            return new double[12] {
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

        public static Vector3 Transform(Vector3 orig, double[] transformInverse)
        {
            /* return new Vector3((float)((orig.X*transformInverse[0,0]) + (orig.Y*transformInverse[1,0]) + (orig.Z*transformInverse[2,0]) + (1*transformInverse[3,0])), */
            /*                    (float)((orig.X*transformInverse[0,1]) + (orig.Y*transformInverse[1,1]) + (orig.Z*transformInverse[2,1]) + (1*transformInverse[3,1])), */
            /*                    (float)((orig.X*transformInverse[0,2]) + (orig.Y*transformInverse[1,2]) + (orig.Z*transformInverse[2,2]) + (1*transformInverse[3,2]))); */

            return new Vector3((float)((orig.X*transformInverse[0]) + (orig.Y*transformInverse[3]) + (orig.Z*transformInverse[6]) + (1*transformInverse[9])),
                               (float)((orig.X*transformInverse[1]) + (orig.Y*transformInverse[4]) + (orig.Z*transformInverse[7]) + (1*transformInverse[10])),
                               (float)((orig.X*transformInverse[2]) + (orig.Y*transformInverse[5]) + (orig.Z*transformInverse[8]) + (1*transformInverse[11])));
        }
    }
}
