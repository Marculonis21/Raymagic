using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Door2 : Interactable
    {
        public Vector3 facing {get; private set;} // for loading levels setter
        Vector3 right;
        Object wallObject;

        Object[] plateObjects;
        Object[] lockObjects;

        Object intersectObject;

        Vector3[] doorPlateClosedOpened;
        float lockRotation = -90;

        public Door2(Vector3 floorDoorPosition, Vector3 facing, Object wallObject, Color secondaryColor) : base(floorDoorPosition, secondaryColor)
        {
            this.facing = facing;
            this.right = Vector3.Cross(facing, new Vector3(0,0,1));

            this.stateCount = 2;
            this.wallObject = wallObject;

            if (facing.Z != 0)
                throw new Exception("Unable to make this kind of button - facing Z");
        }

        public override void ObjectSetup(ref List<Object> staticObjectList, ref List<Object> dynamicObjectList, ref List<PhysicsObject> physicsObjectsList)
        {
            Vector3 up = new Vector3(0,0,1);

            Vector3 centerPos = this.Position + new Vector3(0,0,50);

            wallObject.AddChildObject(new Sphere(centerPos, 80, Color.Black, BooleanOP.DIFFERENCE),false);
            intersectObject = new Sphere(centerPos, 80, Color.Black);

            Cylinder mainFrame = new Cylinder(centerPos + facing*10, facing, 20, 85, Color.Gray);
            mainFrame.AddChildObject(new Cylinder(facing*50, facing, 100, 75, Color.Black, BooleanOP.DIFFERENCE), true);
            mainFrame.AddChildObject(new Cylinder(-facing*6, facing, 8, 75, Color.DarkGray, BooleanOP.UNION), true);
            mainFrame.AddChildObject(new Cylinder(facing*50, facing, 100, 70, Color.Black, BooleanOP.DIFFERENCE), true);

            staticObjectList.Add(mainFrame);

            Box plate1 = new Box(centerPos - right*50, facing*4 + right*100 + up*140, Color.Gray);
            Box plate2 = new Box(centerPos + right*50, facing*4 + right*100 + up*140, this.secondaryColor);

            plate1.AddChildObject(new Box(centerPos + facing*10 + up*10, facing*22 + right*1 + up*140, Color.Black, BooleanOP.DIFFERENCE), false);
            plate2.AddChildObject(new Box(centerPos + facing*10 + up*10, facing*22 + right*1 + up*140, Color.Black, BooleanOP.DIFFERENCE), false);

            plate1.AddChildObject(new Cylinder(centerPos + facing*10 + up*10, facing, 11, 25, Color.Black, BooleanOP.DIFFERENCE), false);
            plate2.AddChildObject(new Cylinder(centerPos + facing*10 + up*10, facing, 11, 25, Color.Black, BooleanOP.DIFFERENCE), false);

            Cylinder lock1 = new Cylinder(centerPos + facing*3 + up*10, facing, 3, 24, Color.Gray);
            Cylinder lock2 = new Cylinder(centerPos + facing*3 + up*10, facing, 3, 24, this.secondaryColor);

            lock1.AddChildObject(new Box( up*23, facing*20 + right*50 + up*45, Color.Black, BooleanOP.INTERSECT), true);
            lock2.AddChildObject(new Box(-up*23, facing*20 + right*50 + up*45, Color.Black, BooleanOP.INTERSECT), true);

            lock1.Rotate(-90, facing, lock1.Position);
            lock2.Rotate(-90, facing, lock2.Position);

            doorPlateClosedOpened = new Vector3[2] {centerPos - right*50, 
                                                    centerPos - right*100};

            plate1.AddChildObject(lock1, false);
            plate2.AddChildObject(lock2, false);

            plateObjects = new Object[2] {plate1, plate2};
            lockObjects = new Object[2] {lock1, lock2};

            this.boundingBoxSize = facing * 10 + right * 200 + new Vector3(0,0,200);
            this.boundingBox = new Box(centerPos,
                                       boundingBoxSize,
                                       Color.Black);
        }

        public override SDFout SDF(Vector3 testPos, float minDist, out bool IsTransparent)
        {
            IsTransparent = false;
            var best = new SDFout(float.MaxValue, Color.Pink);
            var test = new SDFout(float.MaxValue, Color.Pink);
            
            for (int i = 0; i < 2; i++)
            {
                test = plateObjects[i].SDF(testPos, minDist, out _);
                if (test.distance < best.distance)
                {
                    best = test;
                }

                test = lockObjects[i].SDF(testPos, minDist, out _);
                if (test.distance < best.distance)
                {
                    best = test;
                }
            }
            var intersect = intersectObject.SDF(testPos, minDist, out _);

            return SDFs.Combine(best.distance, intersect.distance, best.color, intersect.color, BooleanOP.INTERSECT, 1);
        }

        public override void Interact()
        {
            base.Interact();

            if (this.state == 1)
            {
                DoorOpenAsync();
            }
            else
            {
                DoorCloseAsync();
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

        async Task UnlockDoor()
        {
            while (lockRotation != 90)
            {
                if (state == 0) break; // state was changed to close

                lockObjects[0].Rotate(9,facing,lockObjects[0].Position);
                lockObjects[1].Rotate(9,facing,lockObjects[1].Position);

                lockRotation += 9;

                await Task.Delay(10).ContinueWith(t => { while(Screen.instance.DrawPhase) { } });
            }
        }

        async Task LockDoor()
        {
            while (lockRotation != -90)
            {
                if (state == 1) break; // state was changed to close

                lockObjects[0].Rotate(-9,facing,lockObjects[0].Position);
                lockObjects[1].Rotate(-9,facing,lockObjects[1].Position);
                lockRotation -= 9;

                await Task.Delay(10).ContinueWith(t => { while(Screen.instance.DrawPhase) { } });
            }
        }

        async Task OpenDoor()
        {
            while (plateObjects[0].Position != doorPlateClosedOpened[1])
            {
                if (state == 0) break; // state was changed to close

                plateObjects[0].Translate(this.right*-5f);
                plateObjects[1].Translate(this.right* 5f);

                await Task.Delay(10).ContinueWith(t => { while(Screen.instance.DrawPhase) { } });
            }
        }

        async Task CloseDoor()
        {
            while (plateObjects[0].Position != doorPlateClosedOpened[0])
            {
                if (state == 1) break; // state was changed to close

                plateObjects[0].Translate(this.right* 5f);
                plateObjects[1].Translate(this.right*-5f);

                /* lockObjects[0].Translate(this.right* 5f, true); */
                /* lockObjects[1].Translate(this.right*-5f, true); */

                await Task.Delay(10).ContinueWith(t => { while(Screen.instance.DrawPhase) { } });
            }

        }
        
        async void DoorOpenAsync()
        {
            await Task.Run(() => UnlockDoor()).ContinueWith(t => OpenDoor());
        }

        async void DoorCloseAsync()
        {
            await Task.Run(() => CloseDoor()).ContinueWith(t => LockDoor());
        }
    }
}
