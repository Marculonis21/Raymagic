using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class Sphere : IObject
    {
        Vector3 position;
        float size;
        Color color;

        public Sphere(Vector3 position, float size, Color color)
        {
            this.position = position;
            this.size = size;
            this.color = color;
        }

        public float SDF(Vector3 testPos)
        {
            return SDFs.Sphere(testPos, this.position, this.size);
        }

        public Color GetColor()
        {
            return color;
        }
    }
}
