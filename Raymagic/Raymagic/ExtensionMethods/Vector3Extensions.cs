using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public static class Vector3Extensions
    {
        public static Color ToColor(this Vector3 v)
        {
            int r = (int)(v.X * 255);
            int g = (int)(v.Y * 255);
            int b = (int)(v.Z * 255);
            return new Color(r,g,b);
        }
    }
}
