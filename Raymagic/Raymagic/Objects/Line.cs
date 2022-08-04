using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Line : Object
    {
        Vector3 a;
        Vector3 b;

        public Line(Vector3 position, Vector3 end, Color color, BooleanOP booleanOP=BooleanOP.NONE, float opStrength=1, Vector3 boundingBoxSize = new Vector3(), bool selectable=false, string info="") : base(position, color, boundingBoxSize, info, booleanOP, opStrength, selectable)
        {
            // this is weird, probably transformation does some magic
            // a is half of actual start position a'/2=a, b is actual translation from a to end 
            //
            // a = (50,50,50) -> a' = (100,100,100) = start
            // b = (100,200,200) -> actual end = a+b = (150,250,250)
            //
            
            this.a = new Vector3();
            this.b = end-position;
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Line(testPos, this.a, this.b, 2);
        }
    }
}
