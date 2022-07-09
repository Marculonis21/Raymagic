using Microsoft.Xna.Framework;

namespace Raymagic
{
    public delegate void TriggerOnCollision(Object obj);

    public class PhysicsTrigger : PhysicsObject
    {
        public event TriggerOnCollision onCollisionEvent;

        public PhysicsTrigger(Vector3 position, float size) : base(position, size, Color.Black, Color.Black)
        {
            this.isTrigger = true;
        }

        public new bool FindCollision(out Vector3 axis, out float length, out Object hitObj)
        {
            Ray testRay = new Ray(this.Position, new Vector3());
            axis = new Vector3();

            RayMarchingHelper.PhysicsRayMarch(testRay, 1, size, out length, out Vector3 _, out hitObj, caller:this);

            if (length <= this.size)
            {
                if (hitObj is PhysicsObject) 
                {
                    length = size - length;

                    // trigger collision event
                    onCollisionEvent?.Invoke(hitObj);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
