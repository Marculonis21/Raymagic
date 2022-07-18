using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Button : Interactable
    {
        Vector3 facing;
        public Button(Vector3 position, Vector3 facing, Color secondaryColor) : base(position + new Vector3(0,0,50), secondaryColor)
        {
            this.playerControllable = true;
            this.controlDistance = 60;

            this.stateCount = 2;
            this.facing = facing;

            if (facing.Z != 0)
                throw new Exception("Unable to make this kind of button - facing Z");

        }

        public override void ObjectSetup()
        {
            // offset for easier modelling 
            Vector3 position = this.Position + new Vector3(0,0,50) ;

            Object bBase = new Cylinder(position, new Vector3(0,0,1), 50, 10, Color.Gray, false);

            facing.Normalize();
            bBase.AddChildObject(new Sphere(new Vector3(facing.X, facing.Y, 2) * 4, 14, Color.Black, false, BooleanOP.SDIFFERENCE, 1), true);
            bBase.AddChildObject(new Cylinder(new Vector3(0,0,-15), new Vector3(0,0,1), 3, 10.5f, this.secondaryColor, false), true);

            Map.instance.staticObjectList.Add(bBase);


            Object button1 = new Cylinder(position + new Vector3(0,0,-1), new Vector3(0,0,1), 7, 8, Color.DarkRed, false);
            button1.Rotate(18,Vector3.Cross(facing, new Vector3(0,0,1)));

            Object button2 = new Cylinder(position + new Vector3(0,0,-3), new Vector3(0,0,1), 5, 8, Color.Red, false);
            button2.Rotate(18,Vector3.Cross(facing, new Vector3(0,0,1)));

            modelStates.Add(button1);
            modelStates.Add(button2);

            this.boundingBoxSize = new Vector3(10,10,10);
            this.boundingBox = new Box(position,
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
