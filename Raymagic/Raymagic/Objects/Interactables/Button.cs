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

        public override void ObjectSetup(ref List<Object> staticObjectList, ref List<Object> dynamicObjectList, ref List<PhysicsObject> physicsObjectsList)
        {
            // offset for easier modelling 
            Vector3 position = this.Position;
            facing.Normalize();

            Object bBase1 = new Cylinder(position, new Vector3(0,0,1), 50, 10, Color.Gray);

            bBase1.AddChildObject(new Sphere(new Vector3(facing.X, facing.Y, 2) * 4, 14, Color.Black, BooleanOP.SDIFFERENCE, 1), true);
            bBase1.AddChildObject(new Cylinder(new Vector3(0,0,-15), new Vector3(0,0,1), 3, 12f, this.secondaryColor), true);

            Object bBase2 = new Cylinder(position, new Vector3(0,0,1), 50, 10, Color.Gray);

            bBase2.AddChildObject(new Sphere(new Vector3(facing.X, facing.Y, 2) * 4, 14, Color.Black, BooleanOP.SDIFFERENCE, 1), true);
            bBase2.AddChildObject(new Cylinder(new Vector3(0,0,-15), new Vector3(0,0,1), 3, 12f, this.secondaryColor), true);

            Object button1 = new Cylinder(position + new Vector3(0,0,2) + facing*1, new Vector3(0,0,1), 7, 7, new Color(50,0,0));
            button1.Rotate(18,Vector3.Cross(facing, new Vector3(0,0,1)),button1.Position);
            /* bBase1.AddChildObject(button1, false); */

            Object button2 = new Cylinder(position + new Vector3(0,0,-1) + facing*1, new Vector3(0,0,1), 5, 7, Color.Red);
            button2.Rotate(18,Vector3.Cross(facing, new Vector3(0,0,1)), button2.Position);
            /* bBase2.AddChildObject(button2, false); */
            staticObjectList.Add(bBase1);

            modelStates.Add(button1);
            modelStates.Add(button2);

            this.boundingBoxSize = new Vector3(30,30,50);
            this.boundingBox = new Box(this.Position - new Vector3(0,0,boundingBoxSize.Z/2),
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
