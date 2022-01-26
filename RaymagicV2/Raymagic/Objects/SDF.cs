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

/*         public static float BoxFrame(Vector3 test, Vector3 center, Vector3 size, float frameSize) */
/*         { */
/*             test = new Vector3(Math.Abs(test.X), Math.Abs(test.Y), Math.Abs(test.Z)) - size; */
/*             Vector3 q = new Vector3(Math.Abs(test.X + frameSize),Math.Abs(test.X + frameSize),Math.Abs(test.X + frameSize)) - Vector3.One * frameSize; */

/*             Vector3.Max(new Vector3(test.X,q.Y,q.Z), Vector3.Zero)) + Vector3.Min(Vector3.Max(test.X,Max(q.Y,q.Z))) */
/*             float d = x; */
/*             d = Math.Max(d,y); */
/*             d = Math.Max(d,z); */
/*             return d; */
/*         } */

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
