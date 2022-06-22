using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class PlayerModel : Object
    {
        Vector3 size;

        Object body;

        public PlayerModel(float height, float radius) : base(new Vector3(), Color.White, false, new Vector3(), "", BooleanOP.NONE, 0, false)
        {
            body = new Capsule(new Vector3(400,400,0),
                                       75/2, 
                                       25, 
                                       Color.Orange,
                                       false, 
                                       boundingBoxSize: new Vector3(35,35,75));

            Capsule topPart = new Capsule(new Vector3(400,400,0),
                                          75/2,
                                          26,
                                          Color.White,
                                          false,
                                          boundingBoxSize: new Vector3(35,35,75));

            topPart.AddChildObject(new Plane(new Vector3(0,0,75/4),
                                             new Vector3(0,0,-1),
                                             Color.Black,
                                             booleanOP: BooleanOP.INTERSECT), true);

            body.AddChildObject(topPart, false);
        }

        public override SDFout SDF(Vector3 testPos, float minDist, bool useBounding=true, bool physics=false)
        {
            SDFout current = body.SDF(testPos, minDist, useBounding, physics);

            return current;
        }
    }
}
