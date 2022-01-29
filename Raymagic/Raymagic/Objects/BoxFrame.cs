using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class BoxFrame : Object
    {
        Vector3 size;
        float frameSize;

        public BoxFrame(Vector3 position, Vector3 size, float frameSize, Color color, bool staticObject = true, Vector3 boundingBoxSize = new Vector3(), string info="") : base(position, color, staticObject, boundingBoxSize, info)
        {
            this.size = size;
            this.frameSize = frameSize;
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.BoxFrame(testPos, this.position, this.size, this.frameSize);
        }
    }
}
