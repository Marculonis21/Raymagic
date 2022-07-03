using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class PhysicsSolver
    {
        List<PhysicsObject> objects;
        Vector3 gravity = new Vector3(0,0,-1);

        public void Update(float dt, List<PhysicsObject> objects)
        {
            this.objects = objects;

            ApplyGravity();
            SolveCollisions();
            UpdatePositions(dt);
        }

        public void ApplyGravity()
        {
            foreach (var obj in objects)
            {
                obj.ApplyForce(gravity);
            }
        }

        public void UpdatePositions(float dt)
        {
            foreach (var obj in objects)
            {
                obj.UpdatePosition(dt);
            }
        }

        public void SolveCollisions()
        {
            foreach (var obj in objects)
            {
                if (obj.FindCollision(out Vector3 hitAxis, out float length))
                {
                    obj.Translate(hitAxis*length);
                }
            }
        }
    }
}
