using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Door : Interactable
    {
        Object[] doorPlates;
        Vector3 facing;
        Vector3 right;

        Vector3[] doorPlateClosed;
        Vector3[] doorPlateOpened;
        public Door(Vector3 floorDoorPosition, Vector3 facing) : base(floorDoorPosition)
        {
            this.facing = facing;
            this.right = Vector3.Cross(facing, new Vector3(0,0,1));
            this.stateCount = 2;

            if (facing.Z != 0)
                throw new Exception("Unable to make this kind of button - facing Z");
        }

        public override void ObjectSetup()
        {
            Object doorFrame = new Capsule(this.Position + new Vector3(0,0, -45-45), 70, 65, Color.DarkGray, false);
            doorFrame.AddChildObject(new Capsule(this.Position + new Vector3(0,0, -45-45), 90, 50, Color.Black, false, BooleanOP.DIFFERENCE), false);
            doorFrame.AddChildObject(new Plane( facing * 5,  facing, Color.Black, BooleanOP.INTERSECT), true);
            doorFrame.AddChildObject(new Plane(-facing * 5, -facing, Color.Black, BooleanOP.INTERSECT), true);

            Object doorPlatesSideBar = new Capsule(this.Position + new Vector3(0,0, -45-45), 90, 50, Color.Cyan, false);
            doorPlatesSideBar.AddChildObject(new Capsule(this.Position + new Vector3(0,0, -45-45), 91, 48, Color.Black, false, BooleanOP.DIFFERENCE), false);
            doorPlatesSideBar.AddChildObject(new Plane( facing * 4,  facing, Color.Black, BooleanOP.INTERSECT), true);
            doorPlatesSideBar.AddChildObject(new Plane(-facing * 4, -facing, Color.Black, BooleanOP.INTERSECT), true);
            doorFrame.AddChildObject(doorPlatesSideBar, false);

            Map.instance.staticObjectList.Add(doorFrame);
            /* this.modelStates.Add(doorFrame); // frame is the same when closed and opened */
            /* this.modelStates.Add(doorFrame); // frame is the same when closed and opened */

            this.doorPlateClosed = new Vector3[] {this.Position + right* 40 + new Vector3(0, 0, 55),
                                                  this.Position + right*-40 + new Vector3(0, 0 ,55)};

            this.doorPlateOpened = new Vector3[] {this.Position + right* 40 + new Vector3(0, 0, 55) + right* 44f,
                                                  this.Position + right*-40 + new Vector3(0, 0 ,55) + right*-44f};

            Object doorPlate1 = new Box(doorPlateClosed[0], facing*3 + right*80 + new Vector3(0,0,110), Color.Gray, false);
            Object doorPlate2 = new Box(doorPlateClosed[1], facing*3 + right*80 + new Vector3(0,0,110), Color.Gray, false);
            doorPlate1.AddChildObject(new Capsule(this.Position + new Vector3(0,0, -45-45), 90, 50, Color.Black, false, BooleanOP.INTERSECT), false);
            doorPlate2.AddChildObject(new Capsule(this.Position + new Vector3(0,0, -45-45), 90, 50, Color.Black, false, BooleanOP.INTERSECT), false);

            doorPlates = new Object[2] {doorPlate1, doorPlate2};

            this.boundingBoxSize = facing * 6 + right * 120 + new Vector3(0,0,120);
            this.boundingBox = new Box(this.Position + new Vector3(0,0,55),
                                       boundingBoxSize,
                                       Color.Black);
            /* Map.instance.staticObjectList.Add(boundingBox); */
        }

        public override SDFout SDF(Vector3 testPos, float minDist, bool physics=false)
        {
            var dp1 = doorPlates[0].SDF(testPos, minDist, physics);
            var dp2 = doorPlates[1].SDF(testPos, minDist, physics);

            if (dp1.distance < dp2.distance)
                return dp1;
            else
                return dp2;

        }

        public override void Interact()
        {
            base.Interact();

            if (this.state == 1)
            {
                DoorOpenAsync();
                Console.WriteLine($"{this} openning");
            }
            else
            {
                DoorCloseAsync();
                Console.WriteLine($"{this} closing");
            }
        }

        public override void EventListener(Interactable obj, int state)
        {
            Interact();
        }

        public void TriggerEnterExit(IPortalable obj, PhysicsTrigger _)
        {
            if (obj is Player)
            {
                Interact();
            }
        }
        
        public async Task DoorOpenAsync()
        {
            while (doorPlates[0].Position != doorPlateOpened[0])
            {
                if (state == 0) break; // state was changed to close

                doorPlates[0].Translate(this.right* 4f, false);
                doorPlates[1].Translate(this.right*-4f, false);
                await Task.Delay(10).ContinueWith(t => { while(Screen.instance.DrawPhase) { } });
            }
        }

        public async Task DoorCloseAsync()
        {
            while (doorPlates[0].Position != doorPlateClosed[0])
            {
                if (state == 1) break; // state was changed to open

                doorPlates[0].Translate(this.right*-4f, false);
                doorPlates[1].Translate(this.right* 4f, false);
                await Task.Delay(10).ContinueWith(t => { while(Screen.instance.DrawPhase) { } });
            }
        }
    }
}
