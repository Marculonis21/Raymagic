using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public enum BooleanOP
    {
        NONE,
        DIFFERENCE,
        INTERSECT,
        UNION,
        SDIFFERENCE,
        SINTERSECT,
        SUNION
    }

    public struct SDFout
    {
        public float distance {get; private set;}
        public Color color {get; private set;}

        public SDFout(float distance, Color color)
        {
            this.distance = distance;
            this.color = color;
        }
    }

    public class SDFs
    {
        public static SDFout Combine(float objA, float objB, Color colorA, Color colorB, BooleanOP op, float opStrength)
        {
            float dst = objA;
            Color color = colorA;

            switch (op)
            {
                case BooleanOP.NONE:
                    if(objB < objA)
                    {
                        dst = objB;
                        color = colorB;
                    }
                    break;

                case BooleanOP.DIFFERENCE:
                    dst = Difference(objA, objB);
                    break;

                case BooleanOP.INTERSECT:
                    dst = Intersect(objA, objB);
                    break;

                case BooleanOP.UNION:
                    dst = Union(objA, objB);

                    if(dst == objB)
                    {
                        color = colorB;
                    }
                    break;

                case BooleanOP.SDIFFERENCE:
                    dst = SmoothDifference(objA, objB, opStrength);
                    break;

                case BooleanOP.SINTERSECT:
                    dst = SmoothIntersect(objA, objB, opStrength);
                    break;

                case BooleanOP.SUNION:
                    SDFout output = SmoothUnion(objA, objB, colorA, colorB, opStrength);
                    dst = output.distance;
                    color = output.color;

                    break;

                default:
                    throw new NotImplementedException("Unknown boolean operation wanted!");
            }

            return new SDFout(dst, color);
        }

        public static float Box(Vector3 test, Vector3 size)
        {
            float x = Math.Max
            (   
                test.X - new Vector3(size.X / 2.0f, 0, 0).Length(),
                - test.X - new Vector3(size.X / 2.0f, 0, 0).Length()
            );
            float y = Math.Max
            (   test.Y - new Vector3(size.Y / 2.0f, 0, 0).Length(),
                - test.Y - new Vector3(size.Y / 2.0f, 0, 0).Length()
            );
            
            float z = Math.Max
            (   test.Z - new Vector3(size.Z / 2.0f, 0, 0).Length(),
                - test.Z - new Vector3(size.Z / 2.0f, 0, 0).Length()
            );
            float d = x;
            d = Math.Max(d,y);
            d = Math.Max(d,z);
            return d;
        }

        public static float BoxFrame(Vector3 test, Vector3 size, float frameSize)
        {
            //not tested
            float box = Box(test, size);

            float boxDiff1 = Box(test, new Vector3(size.X + 10,       size.Y - frameSize,size.Z - frameSize));
            float boxDiff2 = Box(test, new Vector3(size.X - frameSize,size.Y + 10,       size.Z - frameSize));
            float boxDiff3 = Box(test, new Vector3(size.X - frameSize,size.Y - frameSize,size.Z + 10));

            box = Difference(box, boxDiff1);
            box = Difference(box, boxDiff2);
            box = Difference(box, boxDiff3);

            return box;
        }

        public static float Sphere(Vector3 test, float size)
        {
            /* return (center - test).Length() - size; */
            return (test).Length() - size;
        }

        public static float Plane(Vector3 test, Vector3 normal)
        {
            return Vector3.Dot(test, normal);
        }

        public static float Point(Vector3 test, Vector3 center)
        {
            return (center - test).Length();
        }

        public static float Difference(float ORIG, float DIFF)
        {
            return Math.Max(ORIG, -DIFF);
        }
        public static float Intersect(float OBJ1, float OBJ2)
        {
            return Math.Max(OBJ1, OBJ2);
        }
        public static float Union(float OBJ1, float OBJ2)
        {
            return Math.Min(OBJ1, OBJ2);
        }

        public static float SmoothDifference(float OBJ1, float OBJ2, float k)
        {
            float h = Math.Max(k-Math.Abs(-OBJ1-OBJ2),0.0f);
            return (float)(Math.Max(OBJ1, -OBJ2) + h*h*0.25f/k);
        }

        public static float SmoothIntersect(float OBJ1, float OBJ2, float k)
        {
            float h = Math.Max(k-Math.Abs(OBJ1-OBJ2),0.0f);
            return (float)(Math.Max(OBJ1, OBJ2) + h*h*0.25f/k);
        }

        // needs to work with color too
        public static SDFout SmoothUnion(float OBJ1, float OBJ2, Color color1, Color color2, float k)
        {
            float h = Math.Max(k-Math.Abs(OBJ2-OBJ1),0.0f);
            float dst = (float)(Math.Min(OBJ1, OBJ2) - h*h*0.25/k);

            float interpolation = Math.Clamp(0.5f + 0.5f * (OBJ2 - OBJ1)/k, 0.0f, 1.0f);
            Color color = Color.Lerp(color1, color2, interpolation);

            return new SDFout(dst, color);
        }
    }
}
