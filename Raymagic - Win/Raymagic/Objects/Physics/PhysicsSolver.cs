using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class PhysicsSolver
    {
        List<PhysicsObject> objects;
        Map map = Map.instance;

        public void Solve(float dt, List<PhysicsObject> objects)
        {
            this.objects = objects;

            /* int subSteps = 4; */
            /* float subDt = dt/subSteps; */

            /* for (int i = 0; i < subSteps; i++) */
            /* { */
                ApplyGravity();
                SolveCollisions();
                UpdatePositions(dt);
            /* } */
        }

        void ApplyGravity()
        {
            var gravityVector = (Map.instance.gravity*new Vector3(0,0,-1));
            foreach (var obj in objects)
            {
                if (obj.isTrigger || !obj.physicsEnabled) continue;

                obj.ApplyForce(gravityVector);
            }
        }

        void UpdatePositions(float dt)
        {
            foreach (var obj in objects)
            {
                if (obj.isTrigger || !obj.physicsEnabled) continue;

                obj.UpdatePosition(dt);
            }
        }

        public void ForceSolveCollision()
        {
            SolveCollisions();
        }

        void SolveCollisions()
        {
            foreach (var obj in objects)
            {
                if (obj.isTrigger && (obj as PhysicsTrigger).isEnabled)
                {
                    ((PhysicsTrigger)obj).FindCollision(out Vector3 _, out float _, out Object _);
                    continue;
                }

                if (obj.FindCollision(out Vector3 hitAxis, out float length, out Object collisionObject))
                {
                    if ((collisionObject == map.portalList[0] && map.portalList[0].otherPortal != null) || 
                        (collisionObject == map.portalList[1] && map.portalList[1].otherPortal != null)) continue;

                    if (objects.Contains(collisionObject))
                    {
                        obj.Translate(hitAxis*length);
                        collisionObject.Translate(-hitAxis*length);
                    }
                    else if (collisionObject == Player.instance.model && obj.physicsEnabled)
                    {
                        obj.Translate(hitAxis*length*0.15f);
                        Player.instance.TranslateAbsolute(Player.instance.position - hitAxis*length*0.85f);
                    }
                    else
                    {
                        obj.Translate(hitAxis*length);

                        if (!obj.physicsEnabled) continue;

                        obj.UpdateRotation();
                        
                        // touching ground - apply env forces - friction
                        
                        float N =  Map.instance.gravity * 1;
                        /* float frictionForce = N*0.15f; */
                        float frictionForce = N*0.075f;

                        Vector3 velDir = Vector3.Normalize(obj.velocity);
                        if (float.IsNaN(velDir.X)) continue;
                        obj.ApplyForce(-velDir*frictionForce);
                    }
                }
            }
        }
    }
}
