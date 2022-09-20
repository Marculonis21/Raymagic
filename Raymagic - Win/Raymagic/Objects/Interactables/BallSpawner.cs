using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class BallSpawner : Interactable
    {
        PhysicsObject ballObject;
        int delay;

        List<Object> doorList = new List<Object>();

        float translation = 0;

        public BallSpawner(Vector3 roofPosition, Color secondaryColor, PhysicsObject ballObject, int delay=0) : base(roofPosition, secondaryColor)
        {
            this.delay =  delay;
            this.ballObject = ballObject;
            this.stateCount = 2;
        }

        public override void ObjectSetup(ref List<Object> staticObjectList, ref List<Object> dynamicObjectList, ref List<PhysicsObject> physicsObjectsList)
        {
            Cylinder body = new Cylinder(this.Position, new Vector3(0,0,1), 75, 60, Color.Gray);
            Capsule sdiffCapsule = new Capsule(new Vector3(0,0,-70),40,20,Color.Red, BooleanOP.SDIFFERENCE, 5);
            sdiffCapsule.SetSymmetry("XY", new Vector3(50,50,0));
            body.AddChildObject(sdiffCapsule, true);

            Box line1 = new Box(new Vector3(0,0,-75), new Vector3(110,10,10), Color.Black, BooleanOP.DIFFERENCE);
            Box line2 = new Box(new Vector3(0,0,-75), new Vector3(10,110,10), Color.Black, BooleanOP.DIFFERENCE);

            body.AddChildObject(line1, true);
            body.AddChildObject(line2, true);

            Cylinder indicator = new Cylinder(this.Position + new Vector3(0,0,-75),new Vector3(0,0,1),8,62, this.secondaryColor);
            indicator.AddChildObject(new Cylinder(new Vector3(0,0,100), new Vector3(0,0,1), 200, 55, Color.Black, BooleanOP.DIFFERENCE), true);

            body.AddChildObject(new Cylinder(new Vector3(0,0,-200), new Vector3(0,0,-1), 300, 40, Color.Black, BooleanOP.DIFFERENCE), true);

            body.AddChildObject(indicator, false);

            staticObjectList.Add(body);

            Box door1 = new Box(this.Position + new Vector3(-50,-50,-65), new Vector3(98,98,8), Color.DarkGray);
            Box door2 = new Box(this.Position + new Vector3( 50,-50,-65), new Vector3(98,98,8), Color.DarkGray);
            Box door3 = new Box(this.Position + new Vector3(-50, 50,-65), new Vector3(98,98,8), Color.DarkGray);
            Box door4 = new Box(this.Position + new Vector3( 50, 50,-65), new Vector3(98,98,8), Color.DarkGray);

            Sphere intersectSphere = new Sphere(this.Position + new Vector3(0,0,-65), 50, Color.Black, BooleanOP.INTERSECT);
            door1.AddChildObject(intersectSphere, false);
            door2.AddChildObject(intersectSphere, false);
            door3.AddChildObject(intersectSphere, false);
            door4.AddChildObject(intersectSphere, false);

            doorList.AddRange(new Object[] {door1,door2,door3,door4});

            this.boundingBoxSize = new Vector3(130,130,10);
            this.boundingBox = new Box(this.Position + new Vector3(0,0,-65),
                                       this.boundingBoxSize,
                                       Color.Black);
        }

        public override SDFout SDF(Vector3 testPos, float minDist, out bool IsTransparent)
        {
            SDFout best = new SDFout(float.MaxValue, Color.Pink);
            SDFout test;
            IsTransparent = false;

            foreach (var obj in doorList)
            {
                test = obj.SDF(testPos, minDist, out _);

                if (test.distance < best.distance)
                {
                    best = test;
                }
            }

            return best;
        }

        public override void Interact()
        {
            if (this.state == 0)
            {
                base.Interact();

                Map.instance.physicsObjectsList.Remove(this.ballObject);
                Map.instance.portalableObjectList.Remove(this.ballObject);

                Task.Delay(100).ContinueWith(t1 => OpenDoor());
            }
        }

        async Task OpenDoor()
        {
            while (translation != 50)
            {
                doorList[0].Translate(new Vector3(-5,-5,0),false);
                doorList[1].Translate(new Vector3( 5,-5,0),false);
                doorList[2].Translate(new Vector3(-5, 5,0),false);
                doorList[3].Translate(new Vector3( 5, 5,0),false);

                translation += 5;

                await Task.Delay(20).ContinueWith(t => { while(Screen.instance.DrawPhase) { } });
            }

            await Task.Delay(this.delay).ContinueWith(t => SpawnBall());
        }

        async Task SpawnBall()
        {
            this.ballObject.TranslateAbsolute(this.Position-new Vector3(0,0,25));

            await Task.Run(() => { while(Screen.instance.DrawPhase) { } }).ContinueWith(t => { 
                Map.instance.physicsObjectsList.Add(this.ballObject); 
                Map.instance.portalableObjectList.Add(this.ballObject);
            });

            await Task.Delay(250).ContinueWith(t => CloseDoor());
        }

        async Task CloseDoor()
        {
            while (translation != 0)
            {
                doorList[0].Translate(new Vector3( 5, 5,0),false);
                doorList[1].Translate(new Vector3(-5, 5,0),false);
                doorList[2].Translate(new Vector3( 5,-5,0),false);
                doorList[3].Translate(new Vector3(-5,-5,0),false);

                translation -= 5;

                await Task.Delay(20).ContinueWith(t => { while(Screen.instance.DrawPhase) { } });
            }

            this.state = 0;
        }
    }
}
