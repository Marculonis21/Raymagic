using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class LaserSpawner : Interactable
    {
        Vector3 normal;
        Object ground;
        public LaserSpawner(Vector3 position, Vector3 normal, Object ground) : base(position, Color.Black)
        {
            this.normal = normal;
            this.ground = ground;
        }

        public override void ObjectSetup()
        {
            this.boundingBoxSize = new Vector3(30,30,50);
            this.boundingBox = new Box(this.Position - new Vector3(0,0,boundingBoxSize.Z/2),
                                       this.boundingBoxSize,
                                       Color.Black);

            CalculateLaserPathAsync();
        }

        public override SDFout SDF(Vector3 testPos, float minDist, bool physics=false)
        {
            return new SDFout(float.MaxValue, Color.Pink);
        }

        public async Task CalculateLaserPathAsync()
        {
            const int maxDepth = 3;
            Color laserColor = Color.Red;
            List<Object> newLaserList = new List<Object>();
            Vector3 startPosition = this.Position + normal*5;
            Ray laserRay = new Ray(startPosition, normal);

            while (true)
            {
                newLaserList.Clear();
                laserRay = new Ray(startPosition, normal);

                for (int i = 0; i < maxDepth; i++)
                {
                    RayMarchingHelper.PhysicsRayMarch(laserRay, 100, 1, out float _, out Vector3 hitPos, out Object hitObj, caller:this);

                    newLaserList.Add(new Line(laserRay.origin, hitPos+laserRay.direction*5, laserColor));
                    if (Portal.HitObjectIsActivePortal(hitObj))
                    {
                        laserRay = Portal.TransferRay((Portal)hitObj, laserRay, hitPos);
                        laserRay = new Ray(laserRay.origin - laserRay.direction*45, laserRay.direction);
                    }
                    else
                    {
                        break;
                    }
                }

                await Task.Delay(100).ContinueWith(t1 => { while(Screen.instance.DrawPhase) { } });
                Map.instance.laserObjectList.Clear();
                Map.instance.laserObjectList.AddRange(newLaserList);
            }
        }

        public override void Interact()
        {
        }

        public void ButtonUpAndOff()
        {
        }
    }
}
