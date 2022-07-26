using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Lifter : Interactable
    {
        Cylinder piston;
        Plane difPlane;

        /* float pistonMaxHeight = 100; */
        float[] pistonStartStopHeights = new float[2]; 
        public Lifter(Vector3 position, float maxHeight, Color secondaryColor) : base(position, secondaryColor)
        {
            this.stateCount = 2;

            pistonStartStopHeights[0] = this.Position.Z + 10;
            pistonStartStopHeights[1] = maxHeight;
        }

        public override void ObjectSetup()
        {
            Vector3 position = this.Position + new Vector3(0,0,10);
            Vector3 lineSize = new Vector3(150,10,20);

            Box line = new Box(position, lineSize, secondaryColor);
            line.AddChildObject(new Box(new Vector3(0, 10,0), lineSize, Color.Black),    true);
            line.AddChildObject(new Box(new Vector3(0, 20,0), lineSize, secondaryColor), true);
            line.AddChildObject(new Box(new Vector3(0, 30,0), lineSize, Color.Black),    true);
            line.AddChildObject(new Box(new Vector3(0, 40,0), lineSize, secondaryColor), true);
            line.AddChildObject(new Box(new Vector3(0, 50,0), lineSize, Color.Black),    true);
            line.AddChildObject(new Box(new Vector3(0, 60,0), lineSize, secondaryColor), true);
            line.AddChildObject(new Box(new Vector3(0, 70,0), lineSize, Color.Black),    true);
            line.AddChildObject(new Box(new Vector3(0,-10,0), lineSize, Color.Black),    true);
            line.AddChildObject(new Box(new Vector3(0,-20,0), lineSize, secondaryColor), true);
            line.AddChildObject(new Box(new Vector3(0,-30,0), lineSize, Color.Black),    true);
            line.AddChildObject(new Box(new Vector3(0,-40,0), lineSize, secondaryColor), true);
            line.AddChildObject(new Box(new Vector3(0,-50,0), lineSize, Color.Black),    true);
            line.AddChildObject(new Box(new Vector3(0,-60,0), lineSize, secondaryColor), true);
            line.AddChildObject(new Box(new Vector3(0,-70,0), lineSize, Color.Black),    true);

            line.Rotate(45, "Z", line.Position);

            line.AddChildObject(new Box(this.Position + new Vector3(0,0,3), new Vector3(100,100,6), Color.Black, BooleanOP.INTERSECT), false);
            line.AddChildObject(new Box(this.Position + new Vector3(0,0,20), new Vector3(90,90,45), Color.Black, BooleanOP.DIFFERENCE), false);
            line.AddChildObject(new Box(this.Position, new Vector3(100,100,4), Color.Gray), false);

            Map.instance.staticObjectList.Add(line);

            piston = new Cylinder(this.Position + new Vector3(0,0,pistonStartStopHeights[0]), new Vector3(0,0,1), pistonStartStopHeights[1], 15, Color.DarkGray);
            difPlane = new Plane(this.Position, new Vector3(0,0,1), Color.Black);

            Box pistonTop = new Box(new Vector3(0,0,2),new Vector3(80,80,4),Color.Gray);
            piston.AddChildObject(pistonTop, true);

            this.boundingBoxSize = new Vector3(90,90, pistonStartStopHeights[1]);
            this.boundingBox = new Box(this.Position + new Vector3(0,0,pistonStartStopHeights[1]/2),
                                       this.boundingBoxSize,
                                       Color.Yellow);
        }

        public override SDFout SDF(Vector3 testPos, float minDist, bool physics=false)
        {
            var pistonSDF = piston.SDF(testPos, minDist, physics);
            var difPlaneSDF = difPlane.SDF(testPos, minDist, physics);
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

                piston.Translate(new Vector3(0,0,1) * 2, true);
                await Task.Delay(10).ContinueWith(t => { while(Screen.instance.DrawPhase) { } });
            }
        }

        public async Task LifterDownAsync()
        {
            while (piston.Position.Z != pistonStartStopHeights[0])
            {
                if (state == 1) break; // state was changed to close

                piston.Translate(new Vector3(0,0,1) * -2, true);
                await Task.Delay(10).ContinueWith(t => { while(Screen.instance.DrawPhase) { } });
            }
        }
    }
}
