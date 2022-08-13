using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class LaserSpawner : Interactable
    {
        Vector3 normal;
        Object ground;
        Object objBase;
        public LaserSpawner(Vector3 position, Vector3 normal, Object ground) : base(position, Color.Black)
        {
            this.normal = Vector3.Normalize(normal);
            this.ground = ground;
        }

        public override void ObjectSetup(ref List<Object> staticObjectList, ref List<Object> dynamicObjectList, ref List<PhysicsObject> physicsObjectsList)
        {

            Vector3 right = Vector3.Cross(normal, new Vector3(0,0,1));
            Vector3 up = Vector3.Cross(normal, -right);

            Box empty = new Box(this.Position-normal*9, up*35 + right*35 + normal*20, Color.Black, BooleanOP.DIFFERENCE);
            this.ground.AddChildObject(empty, false);

            Box inside = new Box(this.Position - normal*5, up*35 + right*35 + normal*10, Color.Gray)  ;
            inside.AddChildObject(new Box(normal*4, up*31 + right*31 + normal*10, Color.Black, BooleanOP.DIFFERENCE), true);

            Box wiring1 = new Box(this.Position - normal*5, right*40 + up*4 + normal*4, Color.Black);
            Box wiring2 = new Box(this.Position - normal*5, right*4 + up*40 + normal*4, Color.Black);
            wiring1.Rotate(45, normal, wiring1.Position);
            wiring2.Rotate(45, normal, wiring2.Position);

            objBase = new Cylinder(this.Position + normal*2, normal, 15, 10, Color.White); 
            objBase.AddChildObject(new Sphere(normal*-5, 10, Color.Gray), true);
            objBase.AddChildObject(new Plane(normal*2, normal, Color.Black, BooleanOP.INTERSECT), true);
            objBase.AddChildObject(new Sphere(normal*-4,  9, Color.White), true);

            Sphere top2 = new Sphere(this.Position - normal*3, 10, Color.Gray);
            top2.AddChildObject(new Plane(normal*8, -normal, Color.Black, BooleanOP.INTERSECT), true);

            objBase.AddChildObject(top2, false);

            objBase.AddChildObject(new Cylinder(new Vector3(), -normal, 6, 2, Color.DarkRed), true);
            objBase.AddChildObject(new Cylinder(new Vector3(), -normal, 9, 1, Color.Red), true);

            inside.AddChildObject(wiring1, false);
            inside.AddChildObject(wiring2, false);

            staticObjectList.Add(inside);

            this.boundingBoxSize = up*15 + right*15 + normal*30;
            this.boundingBox = new Box(this.Position,
                                       this.boundingBoxSize,
                                       Color.Lime);

            CalculateLaserPathAsync();
        }

        public override SDFout SDF(Vector3 testPos, float minDist, out bool IsTransparent)
        {
            return objBase.SDF(testPos, minDist, out IsTransparent);
        }

        LaserCatcher hitCatcher = null;
        public async Task CalculateLaserPathAsync()
        {
            const int maxDepth = 3;
            Color laserColor = Color.Red;
            List<Object> newLaserList = new List<Object>();
            Vector3 startPosition = this.Position + normal*15;
            Ray laserRay = new Ray(startPosition, normal);
            Object hitObj = null;
            Object caller = this;

            while (true)
            {
                newLaserList.Clear();
                laserRay = new Ray(startPosition, normal);
                caller = this;

                for (int i = 0; i < maxDepth; i++)
                {
                    RayMarchingHelper.PhysicsRayMarch(laserRay, 100, 1, out float _, out Vector3 hitPos, out hitObj, caller:caller);

                    newLaserList.Add(new Line(laserRay.origin, hitPos+laserRay.direction*5, laserColor));
                    if (Portal.HitObjectIsActivePortal(hitObj))
                    {
                        laserRay = Portal.TransferRay((Portal)hitObj, laserRay, hitPos);
                        laserRay = new Ray(laserRay.origin - laserRay.direction*45, laserRay.direction);
                    }
                    else if (hitObj.GetType() == typeof(MirrorBall))
                    {
                        var mb = hitObj as MirrorBall;
                        newLaserList.RemoveAt(newLaserList.Count - 1);
                        newLaserList.Add(new Line(laserRay.origin, hitPos + laserRay.direction*(mb.size/2 + 5), laserColor));
                        laserRay = new Ray(hitPos + laserRay.direction*(mb.size/2 + 5), mb.outDir);
                        caller = hitObj;
                    }
                    else
                    {
                        break;
                    }
                }

                if (hitObj.GetType() == typeof(LaserCatcher))
                {
                    if (hitCatcher == null)
                    {
                        hitCatcher = hitObj as LaserCatcher;
                        hitCatcher.Interact();
                    }
                }
                else
                {
                    if (hitCatcher != null)
                    {
                        hitCatcher.Interact();
                        hitCatcher = null;
                    }
                }

                await Task.Delay(100).ContinueWith(t1 => { while(Screen.instance.DrawPhase) { } });
                Map.instance.laserObjectList.Clear();
                Map.instance.laserObjectList.AddRange(newLaserList);
            }
        }
    }
}
