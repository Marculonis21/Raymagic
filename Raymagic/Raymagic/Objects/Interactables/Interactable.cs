using Microsoft.Xna.Framework;

namespace Raymagic
{
    public delegate void OnStateChange(int state);

    public class Interactable : Object
    {
        public List<Object> modelStates {get; protected set;}
        public int state {get; protected set;}
        public OnStateChange stateChange;

        public Interactable(Vector3 position) : base(position, Color.Black, false, new Vector3(), null, BooleanOP.NONE, 0, false)
        {
            this.modelStates = new List<Object>();
            this.state = 0;
            
            // !! have to create boudning box for the object !!
        }

        public override SDFout SDF(Vector3 testPos, float minDist, bool physics=false)
        {
            return modelStates[state].SDF(testPos, minDist, physics);
        }

        public override float SDFDistance(Vector3 testPos)
        {
            return 0;
        }

        public virtual void Interact()
        {
            state = (state + 1) % modelStates.Count;
            
            stateChange?.Invoke(state);
        }
    }
}
