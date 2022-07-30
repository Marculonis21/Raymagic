using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class PhysicsObject : Sphere, IPortalable
    {
        public bool isTrigger {get; protected set;}
        public bool physicsEnabled {get; set;}

        // Physics object based on Verlet Integration
        Vector3 acceleration;
        public Vector3 velocity {get; private set;}

        public Vector3 position {get; private set;} // old pos
        public Vector3 lastBeforeTranslate = new Vector3(); // for keeping velocities with player driven translates (grab)
        public Vector3 lookDir {get; private set;}
        public Object model {get; private set;}

        public float size {get; private set;}

        public PhysicsObject(Vector3 position, float size, Color color1, Color color2) : base(position, size, color1)
        {
            this.isTrigger = false;
            this.physicsEnabled = true;

            this.position = position;
            this.size = size;

            this.model = this;
            this.lookDir = new Vector3(1,0,0);

            this.AddChildObject(new Plane(new Vector3(),
                                          new Vector3(1,0,0),
                                          Color.Black,
                                          BooleanOP.INTERSECT), true);

            Sphere sphere2 = new Sphere(new Vector3(), 
                                        size, 
                                        color2);

            sphere2.AddChildObject(new Plane(new Vector3(),
                                             new Vector3(-1,0,0),
                                             Color.Black,
                                             BooleanOP.INTERSECT), true);

            this.AddChildObject(sphere2, true);
        }

        public void UpdatePosition(float dt)
        {
            Vector3 newVelocity = this.Position - position;

            this.position = this.Position;
            this.velocity = newVelocity;
            this.Translate(newVelocity + acceleration * dt * dt);

            ClearForces();
        }

        public void UpdateRotation()
        {
            var movementDir = Vector3.Normalize(velocity);
            var upVector = new Vector3(0,0,1);
            if (!float.IsNaN(movementDir.X))
            {
                var rightDir = Vector3.Normalize(Vector3.Cross(movementDir, upVector));
                if (!float.IsNaN(rightDir.X))
                {
                    float angle = (velocity.Length() / this.size) * (float)(180f / Math.PI);

                    this.Rotate(angle, rightDir, this.Position);
                }
            }
        }

        public void ApplyForce(Vector3 force)
        {
            this.acceleration += force;
        }

        public void ClearForces()
        {
            this.acceleration = new Vector3();
        }
        
        public void SetVelocity(Vector3 newVelocity)
        {
            // velocity is calculated from (actuall pos - last pos)
            // to change velocity you need to properly change last pos after
            // translation
        
            this.position += -newVelocity;
        }

        public void ClearVelocity()
        {
            this.position = this.Position;
        }

        public bool FindCollision(out Vector3 axis, out float length, out Object hitObj)
        {
            Ray testRay = new Ray(this.Position, new Vector3());
            axis = new Vector3();

            RayMarchingHelper.PhysicsRayMarch(testRay, 1, size, out length, out Vector3 hit, out hitObj, caller:this);

            if (length <= this.size)
            {
                axis = hitObj.SDF_normal(hit);
                length = size - length;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RotateAbsolute(Vector3 rot)
        {
        }

        public void TranslateAbsolute(Vector3 newPosition)
        {
            lastBeforeTranslate = new Vector3(this.Position.X,this.Position.Y,this.Position.Z);
            this.position = newPosition;
            this.TranslateAbsolute(newPosition, true);
        }
    }
}
