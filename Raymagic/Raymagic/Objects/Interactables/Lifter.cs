using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Lifter : Interactable
    {
        Cylinder piston;
        Plane difPlane;
        const float lifterSpeed = 4;

        /* float pistonMaxHeight = 100; */
        float[] pistonStartStopHeights = new float[2]; 
        public Lifter(Vector3 position, float maxHeight, Color secondaryColor) : base(position, secondaryColor)
        {
            this.stateCount = 2;

            pistonStartStopHeights[0] = this.Position.Z + 10;
            pistonStartStopHeights[1] = maxHeight;
        }

        public override void ObjectSetup(ref List<Object> staticObjectList, ref List<Object> dynamicObjectList, ref List<PhysicsObject> physicsObjectsList)
        {
            Vector3 position = this.Position + new Vector3(0,0,10);
            Vector3 lineSize = new Vector3(150,5,20);

            Box line1 = new Box(position, lineSize, secondaryColor);
            Box line2 = new Box(new Vector3(0,5,0), lineSize, Color.Black);
            line1.AddChildObject(line2, true);
            line1.SetRepetition(new Vector3(0,10,0), 10);
            
            line1.Rotate(45, "Z", line1.Position);

            line1.AddChildObject(new Box(this.Position + new Vector3(0,0,3), new Vector3(100,100,6), Color.Black, BooleanOP.INTERSECT), false);
            line1.AddChildObject(new Box(this.Position + new Vector3(0,0,20), new Vector3(90,90,45), Color.Black, BooleanOP.DIFFERENCE), false);
            line1.AddChildObject(new Box(this.Position, new Vector3(100,100,4), Color.Gray), false);

            staticObjectList.Add(line1);

            piston = new Cylinder(this.Position + new Vector3(0,0,pistonStartStopHeights[0]), new Vector3(0,0,1), pistonStartStopHeights[1], 15, Color.DarkGray);
            difPlane = new Plane(this.Position, new Vector3(0,0,1), Color.Black);

            Box pistonTop = new Box(new Vector3(0,0,2),new Vector3(80,80,4),Color.Gray);
            piston.AddChildObject(pistonTop, true);

            this.boundingBoxSize = new Vector3(90,90, pistonStartStopHeights[1]+4);
            this.boundingBox = new Box(this.Position + new Vector3(0,0,boundingBoxSize.Z/2),
                                       this.boundingBoxSize,
                                       Color.Yellow);
        }

        public override SDFout SDF(Vector3 testPos, float minDist, out bool IsTransparent)
        {
            IsTransparent = false;
            var pistonSDF = piston.SDF(testPos, minDist, out _);
            var difPlaneSDF = difPlane.SDF(testPos, minDist, out _);
            return SDFs.Combine(pistonSDF.distance, 
                                difPlaneSDF.distance, 
                                pistonSDF.color, 
                                difPlaneSDF.color, 
                                BooleanOP.DIFFERENCE, 1);
        }

        public override void Interact()
        {
            base.Interact();

            if (this.state == 1)
            {
                LifterUpAsync();
                Console.WriteLine($"{this} up");
            }
            else
            {
                LifterDownAsync();
                Console.WriteLine($"{this}  down");
            }
        }

        public override void EventListener(Interactable obj, int state)
        {
            Interact();
        }

        public void TriggerEnter(IPortalable obj, PhysicsTrigger _)
        {
            if (obj is Player)
            {
                this.state = 0;
                Interact();
            }
        }

        public void TriggerExit(IPortalable obj, PhysicsTrigger _)
        {
            if (obj is Player)
            {
                this.state = 1;
                Interact();
            }
        }

        public async Task LifterUpAsync()
        {
            while (piston.Position.Z != pistonStartStopHeights[1])
            {
                if (state == 0) break; // state was changed to close

                piston.Translate(new Vector3(0,0,1) * lifterSpeed, true);
                await Task.Delay(10).ContinueWith(t => { while(Screen.instance.DrawPhase) { } });
            }
        }

        public async Task LifterDownAsync()
        {
            while (piston.Position.Z != pistonStartStopHeights[0])
            {
                if (state == 1) break; // state was changed to close

                piston.Translate(new Vector3(0,0,1) * -lifterSpeed, true);
                await Task.Delay(10).ContinueWith(t => { while(Screen.instance.DrawPhase) { } });
            }
        }
    }
}
