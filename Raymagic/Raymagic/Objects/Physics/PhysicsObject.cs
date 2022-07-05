using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class PhysicsObject : Sphere, IPortalable
    {
        // Physics object based on Verlet Integration
        
        Vector3 acceleration;
        public Vector3 velocity {get; private set;}

        public Vector3 position {get; private set;}
        public Vector3 lookDir {get; private set;}
        public Object model {get; private set;}

        float size;

        public PhysicsObject(Vector3 position, float size, Color color) : base(position, size, color, false)
        {
            this.position = position;
            this.size = size;

            this.model = this;
            this.lookDir = new Vector3(1,0,0);
        }
        
        public void UpdatePosition(float dt)
        {
            Vector3 newVelocity = this.Position - position;
            this.position = this.Position;
            this.velocity = newVelocity;
            this.Translate(newVelocity + acceleration * dt * dt);

            ClearForces();
        }

        public void ApplyForce(Vector3 force)
        {
            this.acceleration += force;
        }

        public void ClearForces()
        {
            this.acceleration = new Vector3();
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
            this.position = newPosition;
            this.TranslateAbsolute(newPosition,true);
        }

        public void SetVelocity(Vector3 newVelocity)
        {
            // velocity is calculated from (actuall pos - last pos)
            // to change velocity you need to properly change last pos after
            // translation
        
            Console.WriteLine(newVelocity);
            this.position += -newVelocity;
        }
    }
}
