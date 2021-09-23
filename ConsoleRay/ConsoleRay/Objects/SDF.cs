using System;
using Microsoft.Xna.Framework;

namespace ConsoleRay
{
    public enum BooleanOP
    {
        DIFFERENCE,
        INTERSECT,
        UNION
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
    }
}
