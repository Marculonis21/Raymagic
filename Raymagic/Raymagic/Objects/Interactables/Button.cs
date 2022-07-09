using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Button : Interactable
    {
        public Button(Vector3 position, Vector3 facing) : base(position)
        {
            this.playerControllable = true;
            this.controlDistance = 60;

            if (facing.Z != 0)
                throw new Exception("Unable to make this kind of button - facing Z");
            // stage 1
            Object bBase = new Cylinder(position, new Vector3(0,0,1), 50, 10, Color.Gray, false);

            facing.Normalize();
            bBase.AddChildObject(new Sphere(new Vector3(facing.X, facing.Y, 2) * 4, 14, Color.Black, false, BooleanOP.SDIFFERENCE, 1), true);
            bBase.AddChildObject(new Cylinder(new Vector3(0,0,-15), new Vector3(0,0,1), 3, 10.5f, Color.Cyan, false), true);

            Object button = new Cylinder(new Vector3(0,0,-1), new Vector3(0,0,1), 7, 8, Color.DarkRed, false);
            button.Rotate(18,Vector3.Cross(facing, new Vector3(0,0,1)));
            bBase.AddChildObject(button, true);

            modelStates.Add(bBase);

            // stage 2
            Object bBase2 = new Cylinder(position, new Vector3(0,0,1), 50, 10, Color.Gray, false);
            bBase2.AddChildObject(new Sphere(new Vector3(facing.X, facing.Y, 2) * 4, 14, Color.Black, false, BooleanOP.SDIFFERENCE, 1), true);
            bBase2.AddChildObject(new Cylinder(new Vector3(0,0,-15), new Vector3(0,0,1), 3, 10.5f, Color.Cyan, false), true);

            Object button2 = new Cylinder(new Vector3(0,0,-3), new Vector3(0,0,1), 5, 8, Color.Red, false);
            button2.Rotate(18,Vector3.Cross(facing, new Vector3(0,0,1)));
            bBase2.AddChildObject(button2, true);

            modelStates.Add(bBase2);

            this.boundingBoxSize = new Vector3(12,12,50);

            this.boundingBox = new Box(position + new Vector3(0,0,-25), 
                                       this.boundingBoxSize,
                                       Color.Black);

        }

        public override void Interact()
        {
            if (this.state == 0)
            {
                base.Interact();
                Task.Delay(250).ContinueWith(t1 => 
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
