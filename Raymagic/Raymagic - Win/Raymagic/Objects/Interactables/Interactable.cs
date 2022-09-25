using Microsoft.Xna.Framework;

namespace Raymagic
{
    public delegate void OnStateChangeEvent(Interactable obj, int state);

    public abstract class Interactable : Object
    {
        public List<Object> modelStates {get; protected set;}
        public int state {get; protected set;}
        public int stateCount {get; protected set;}
        public event OnStateChangeEvent stateChangeEvent;

        protected bool playerControllable = false;
        protected float controlDistance = float.MaxValue;

        public Color secondaryColor {get; protected set;}

        public Interactable(Vector3 position, Color secondaryColor) : base(position, Color.Black, new Vector3(), null, BooleanOP.NONE, 0, false)
        {
            this.modelStates = new List<Object>();
            this.state = 0;
            this.stateCount = 0;

            this.secondaryColor = secondaryColor;
            
            // !! have to create boudning box for the object !!
        }

        public abstract void ObjectSetup(ref List<Object> staticObjectList, ref List<Object> dynamicObjectList, ref List<PhysicsObject> physicsObjectsList);

        public virtual void ObjectStartup()
        {
        }

        public override SDFout SDF(Vector3 testPos, float minDist, out bool IsTransparent)
        {
            return modelStates[state].SDF(testPos, minDist, out IsTransparent);
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return 0;
        }

        public virtual void Interact()
        {
            state = (state + 1) % stateCount;
            
            OnStateChange(this, state);
        }

        protected virtual void OnStateChange(Interactable obj, int state)
        {
            stateChangeEvent?.Invoke(obj, state);
        }

        public virtual void EventListener(Interactable obj, int state) 
        { 
            Interact();
        }

        public static void PlayerInteract(Vector3 playerPos)
        {
            foreach (var obj in Map.instance.interactableObjectList)
            {
                if (obj.playerControllable && Vector3.Distance(playerPos, obj.Position) < obj.controlDistance)
                {
                    obj.Interact();
                }
            }
        }
    }
}
