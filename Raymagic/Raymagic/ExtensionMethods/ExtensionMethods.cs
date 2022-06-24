using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Extreme.Mathematics;

namespace Raymagic
{
    public static class ExtensionMethods
    {
        public static Color ToColor(this Vector3 v)
        {
            int r = (int)(v.X * 255);
            int g = (int)(v.Y * 255);
            int b = (int)(v.Z * 255);
            return new Color(r,g,b);
        }

        public static Vector3 ToVector3(this Vector<double> v)
        {
            return new Vector3((float)v[0], (float)v[1], (float)v[2]);
        }
    }
}
