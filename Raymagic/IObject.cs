using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public interface IObject
    {
        public float SDF(Vector3 testPos);
        public Vector3 SDF_normal(Vector3 testPos);
        public Color GetColor();
    }
}
