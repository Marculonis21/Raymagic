using Microsoft.Xna.Framework;

namespace Raymagic
{
    public delegate void CollisionTrigger(IPortalable obj, PhysicsTrigger trigger);

    public class PhysicsTrigger : PhysicsObject
    {
        public event CollisionTrigger onCollisionEnter;
        public event CollisionTrigger onCollisionExit;

        public List<IPortalable> inTriggerList {get; private set;} // IPortalable are all the objects worth triggering

        public bool isEnabled = true;

        public PhysicsTrigger(Vector3 position, float size, bool startEnabled=true) : base(position, size, Color.Black, Color.Black)
        {
            this.isTrigger = true;
            this.isEnabled = startEnabled;
            inTriggerList = new List<IPortalable>();
        }

        public new bool FindCollision(out Vector3 axis, out float length, out Object hitObj)
        {
            length = float.MaxValue;
            hitObj = null;
            axis = new Vector3();

            List<IPortalable> toRemove = new List<IPortalable>();
            foreach (var obj in inTriggerList)
            {
                if (Vector3.Distance(this.position, obj.position) >= this.size)
                {
                    toRemove.Add(obj);
                    // invoke after removeItems
                }
            }
            inTriggerList.RemoveAll(x => toRemove.Contains(x));  
            if (toRemove.Count > 0) 
            {
                foreach (var obj in toRemove)
                {
                    onCollisionExit?.Invoke(obj, this);
                }
            }

            foreach (var obj in Map.instance.portalableObjectList)
            {
                if (inTriggerList.Contains(obj)) continue;

                if (Vector3.Distance(this.position, obj.position) <= this.size)
                {
                    inTriggerList.Add(obj);
                    onCollisionEnter?.Invoke(obj, this);
                }
            }

            return false;
        }
    }
}
