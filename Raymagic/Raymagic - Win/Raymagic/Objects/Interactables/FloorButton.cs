using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class FloorButton : Interactable
    {
        PhysicsTrigger buttonBallTrigger;
        PhysicsTrigger buttonPlayerTrigger;

        public FloorButton(Vector3 position, Color secondaryColor) : base(position, secondaryColor)
        {
            this.stateCount = 2;
        }

        public override void ObjectSetup(ref List<Object> staticObjectList, ref List<Object> dynamicObjectList, ref List<PhysicsObject> physicsObjectsList)
        {
            // stage 1
            Cylinder body = new Cylinder(this.Position + new Vector3(0,0,10), new Vector3(0,0,1), 8, 30, Color.White);

            body.AddChildObject(new Cylinder(this.Position + new Vector3(0,0,2), new Vector3(0,0,1), 2, 50, Color.White, BooleanOP.SUNION, 40), false);
            body.AddChildObject(new Cylinder(this.Position + new Vector3(0,0,10), new Vector3(0,0,1), 20, 55, Color.Black, BooleanOP.INTERSECT), false); 
            body.AddChildObject(new Cylinder(this.Position + new Vector3(0,0,100), new Vector3(0,0,1), 102, 40, Color.Black, BooleanOP.DIFFERENCE), false); 
            body.AddChildObject(new Cylinder(this.Position + new Vector3(0,0,3), new Vector3(0,0,1), 3, 40, Color.DarkGray), false); 

            Sphere indicator = new Sphere(this.Position, 8, this.secondaryColor);
            indicator.SetSymmetry("XY", new Vector3(35,35,0));
            body.AddChildObject(indicator);

            staticObjectList.Add(body);
            
            Cylinder button1 = new Cylinder(this.Position + new Vector3(0,0,15), new Vector3(0,0,1), 6, 38, Color.DarkRed);
            button1.AddChildObject(new Box(new Vector3(0,0,1), new Vector3(55, 5, 2), Color.DarkRed), true);
            button1.AddChildObject(new Box(new Vector3(0,0,1), new Vector3(5, 55, 2), Color.DarkRed), true);
            button1.AddChildObject(new Sphere(this.Position + new Vector3(0,0,38), 27, Color.Black, BooleanOP.DIFFERENCE), false);

            Cylinder button2 = new Cylinder(this.Position + new Vector3(0,0,10), new Vector3(0,0,1), 6, 38, Color.Red);
            button2.AddChildObject(new Box(new Vector3(0,0,1), new Vector3(55, 5, 2), Color.Red), true);
            button2.AddChildObject(new Box(new Vector3(0,0,1), new Vector3(5, 55, 2), Color.Red), true);
            button2.AddChildObject(new Sphere(this.Position + new Vector3(0,0,33), 27, Color.Black, BooleanOP.DIFFERENCE), false);

            this.modelStates.Add(button1);
            this.modelStates.Add(button2);
            
            this.boundingBoxSize = new Vector3(90,90,30);

            this.boundingBox = new Box(this.Position + new Vector3(0,0,10), 
                                       this.boundingBoxSize,
                                       Color.Black);

            this.buttonBallTrigger = new PhysicsTrigger(this.Position + new Vector3(0,0,-5), 60);
            this.buttonBallTrigger.onCollisionEnter += OnTriggerEnter;
            this.buttonBallTrigger.onCollisionExit += OnTriggerExit;

            this.buttonPlayerTrigger = new PhysicsTrigger(this.Position + new Vector3(0,0,70), 45);
            this.buttonPlayerTrigger.onCollisionEnter += OnTriggerEnter;
            this.buttonPlayerTrigger.onCollisionExit += OnTriggerExit;

            physicsObjectsList.Add(buttonBallTrigger);
            physicsObjectsList.Add(buttonPlayerTrigger);
        }

        public void OnTriggerEnter(IPortalable obj, PhysicsTrigger trigger)
        {
            if (this.state == 0)
            {
                if (obj is PhysicsObject && trigger == this.buttonBallTrigger)
                {
                    Interact();
                }
                if (obj is Player && trigger == this.buttonPlayerTrigger)
                {
                    Interact();
                }
            }
        }

        public void OnTriggerExit(IPortalable obj, PhysicsTrigger _)
        {
            if (this.state == 1 && this.buttonPlayerTrigger.inTriggerList.Count == 0 
                                && this.buttonBallTrigger.inTriggerList.Count == 0)
            {
                Interact();
            }
        }
    }
}
