using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class PhysicsSolver
    {
        List<PhysicsObject> objects;
        Vector3 gravity = new Vector3(0,0,-2500f); // weird af gravity values

        public void Solve(float dt, List<PhysicsObject> objects)
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
                Informer.instance.AddInfo("sadfasdfasdfadsfasdf", obj.Position.ToString());
            }
        }

        public void SolveCollisions()
        {
            foreach (var obj in objects)
            {
                if (obj.FindCollision(out Vector3 hitAxis, out float length, out Object collisionObject))
                {
                    if (objects.Contains(collisionObject))
                    {
                        obj.Translate(hitAxis*length);
                        collisionObject.Translate(-hitAxis*length);
                    }
                    else
                    {
                        obj.Translate(hitAxis*length);
                    }
                }
            }
        }
    }
}
