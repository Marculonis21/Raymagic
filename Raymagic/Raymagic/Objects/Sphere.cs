using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Sphere : Object
    {
        float size;

        public Sphere(Vector3 position, float size, Color color, bool staticObject = true, Vector3 boundingBoxSize = new Vector3(), string info = "") : base(position, color, staticObject, boundingBoxSize, info)
        {
            this.size = size;
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Sphere(testPos, new Vector3(), this.size);
        }
    }
}
