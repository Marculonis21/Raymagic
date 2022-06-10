using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Portal : Object
    {
        protected Vector3 normal;
        int type;

        public Portal(Vector3 center, Vector3 normal, int type) : base(center, Color.Black, false, new Vector3(), "", BooleanOP.NONE, 0, false)
        {
            this.normal = normal;
            this.type = type;

            if (type == 0)
            {
                this.color = Color.Orange;
            }
            else
            {
                this.color = Color.Blue;
            }
        }

        // special testing method including ray 
        public SDFout PortalSDF(Vector3 testPos, float minDist, Ray ray, int depth, bool useBounding=true, bool physics=false)
        {
            SDFout current = new SDFout(SDFDistance(Transform(testPos)), this.color);

            Color outColor = this.color;
            if (Vector3.Distance(testPos, this.Position) < 40)
            {
                if (this.type == 0 && Map.instance.portalList[1] != null)
                {
                    var otherPortal = Map.instance.portalList[1];
                    var otherPos = otherPortal.Position;
                    var otherNormal = otherPortal.normal;
                    var translate = this.Position - testPos;

                    Vector3 outDir = Vector3.Normalize(ray.direction - 2*Vector3.Dot(ray.direction, this.normal)*this.normal);
                    Ray outRay = new Ray((otherPos-translate)+outDir*25, outDir);

                    RayMarchingHelper.RayMarch(outRay, out float _, out outColor, depth+1);
                }
                else if (this.type == 1 && Map.instance.portalList[0] != null)
                {
                    var otherPortal = Map.instance.portalList[0];
                    var otherPos = otherPortal.Position;
                    var otherNormal = otherPortal.normal;
                    var translate = this.Position - testPos;

                    Vector3 outDir = Vector3.Normalize(ray.direction - 2*Vector3.Dot(ray.direction, this.normal)*this.normal);
                    Ray outRay = new Ray((otherPos-translate)+outDir*25, outDir);

                    RayMarchingHelper.RayMarch(outRay, out float _, out outColor, depth+1);
                }
            }

            return new SDFout(current.distance, outColor);
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return SDFs.Portal(testPos, this.normal);
        }
    }
}
