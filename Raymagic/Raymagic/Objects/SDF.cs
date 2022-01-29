using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public enum BooleanOP
    {
        DIFFERENCE,
        INTERSECT,
        UNION,
        SDIFFERENCE,
        SINTERSECT,
        SUNION
    }

    public class SDFs
    {
        public static float Box(Vector3 test, Vector3 center, Vector3 size)
        {
            float x = Math.Max
            (   
                test.X - center.X - new Vector3(size.X / 2.0f, 0, 0).Length(),
                center.X - test.X - new Vector3(size.X / 2.0f, 0, 0).Length()
            );
            float y = Math.Max
            (   test.Y - center.Y - new Vector3(size.Y / 2.0f, 0, 0).Length(),
                center.Y - test.Y - new Vector3(size.Y / 2.0f, 0, 0).Length()
            );
            
            float z = Math.Max
            (   test.Z - center.Z - new Vector3(size.Z / 2.0f, 0, 0).Length(),
                center.Z - test.Z - new Vector3(size.Z / 2.0f, 0, 0).Length()
            );
            float d = x;
            d = Math.Max(d,y);
            d = Math.Max(d,z);
            return d;
        }

        public static float BoxFrame(Vector3 test, Vector3 center, Vector3 size, float frameSize)
        {
            //not tested
            float box = Box(test, center, size);

            float boxDiff1 = Box(test, center, new Vector3(size.X + 10,       size.Y - frameSize,size.Z - frameSize));
            float boxDiff2 = Box(test, center, new Vector3(size.X - frameSize,size.Y + 10,       size.Z - frameSize));
            float boxDiff3 = Box(test, center, new Vector3(size.X - frameSize,size.Y - frameSize,size.Z + 10));

            box = BooleanDifference(box, boxDiff1);
            box = BooleanDifference(box, boxDiff2);
            box = BooleanDifference(box, boxDiff3);

            return box;
        }

        public static float Sphere(Vector3 test, Vector3 center, float size)
        {
            return (center - test).Length() - size;
        }

        public static float Plane(Vector3 test, Vector3 normal, float height)
        {
            return Vector3.Dot(test, normal) + height;
        }

        public static float Point(Vector3 test, Vector3 center)
        {
            return (center - test).Length();
        }

        public static float BooleanDifference(float ORIG, float DIFF)
        {
            return Math.Max(ORIG, -DIFF);
        }
        public static float BooleanIntersect(float OBJ1, float OBJ2)
        {
            return Math.Max(OBJ1, OBJ2);
        }
        public static float BooleanUnion(float OBJ1, float OBJ2)
        {
            return Math.Min(OBJ1, OBJ2);
        }

        public static float opSmoothUnion(float OBJ1, float OBJ2, float k)
        {
            float h = Math.Max(k-Math.Abs(OBJ1-OBJ2),0.0f);
            return (float)(Math.Min(OBJ1, OBJ2) - h*h*0.25/k);
        }

        public static float opSmoothSubtraction(float OBJ1, float OBJ2, float k)
        {
            float h = Math.Max(k-Math.Abs(-OBJ1-OBJ2),0.0f);
            return (float)(Math.Max(-OBJ1, OBJ2) + h*h*0.25f/k);
        }

        public static float opSmoothIntersection(float OBJ1, float OBJ2, float k)
        {
            float h = Math.Max(k-Math.Abs(OBJ1-OBJ2),0.0f);
            return (float)(Math.Max(OBJ1, OBJ2) + h*h*0.25f/k);
        }
    }
}
