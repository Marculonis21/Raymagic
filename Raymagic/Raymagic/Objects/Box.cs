using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Box : Object
    {
        Vector3 size;

        public Box(Vector3 position, Vector3 size, Color color, bool staticObject = true, Vector3 boundingBoxSize = new Vector3(), string info="") : base(position, color, staticObject, boundingBoxSize, info)
        {
            this.size = size;
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Box(testPos, this.position, this.size);
        }
    }
}
