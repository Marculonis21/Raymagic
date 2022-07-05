using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class PhysicsObject : Sphere
    {
        // Physics object based on Verlet Integration
        
        Vector3 oldPosition;
        Vector3 acceleration;
        public Vector3 oldVelocity {get; private set;}
        float size;

        public PhysicsObject(Vector3 position, float size, Color color) : base(position, size, color, false)
        {
            this.oldPosition = position;
            this.size = size;
        }
        
        public void UpdatePosition(float dt)
        {
            Vector3 velocity = this.Position - oldPosition;
            oldPosition = this.Position;
            oldVelocity = velocity;
            this.Translate(velocity + acceleration * dt * dt);

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
    }
}
