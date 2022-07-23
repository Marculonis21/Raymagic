using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Lifter : Interactable
    {
        Vector3 facing;
        public Lifter(Vector3 position, Color secondaryColor) : base(position, secondaryColor)
        {
            this.stateCount = 2;
        }

        public override void ObjectSetup()
        {
            this.boundingBoxSize = new Vector3(30,30,100);
            this.boundingBox = new Box(this.Position,
                                       this.boundingBoxSize,
                                       Color.Black);
        }

        public override void Interact()
        {
            if (this.state == 0)
            {
                base.Interact();
                // while avoids tearing
                Task.Delay(100).ContinueWith(t1 => 
                                             { while(Screen.instance.DrawPhase) { } }).ContinueWith( 
                                             t2 => ButtonUpAndOff());
            }
        }

        public void ButtonUpAndOff()
        {
            this.state = 0;
        }
    }
}
