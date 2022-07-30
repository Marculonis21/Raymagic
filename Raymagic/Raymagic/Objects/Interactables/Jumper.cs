using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Jumper : Interactable
    {
        Vector3 normal;
        Vector3 arrowDir;
        Vector3 right;

        Vector3 jumperDirection;
        float jumperStrength;
        Object floor;

        List<Object> objList = new List<Object>();
        Object glassPlate;

        public Jumper(Vector3 position, Vector3 normal, Vector3 arrowDir, Vector3 jumperDirection, float strength, Object floorObject) : base(position, Color.Aqua)
        {
            this.stateCount = 2;
            this.normal = normal;
            this.arrowDir = arrowDir;
            this.jumperDirection = Vector3.Normalize(jumperDirection);
            this.jumperStrength = strength;
            this.floor = floorObject;
        }

        public override void ObjectSetup()
        {
            // offset for easier modelling 
            normal.Normalize();
            right = Vector3.Cross(arrowDir, normal);

            Cylinder c1 = new Cylinder(this.Position + normal*30 + arrowDir*30,
                                       normal,
                                       50, 30, Color.Black, BooleanOP.DIFFERENCE);

            Cylinder c2 = new Cylinder(this.Position + normal*30 - arrowDir*30,
                                       normal,
                                       50, 30, Color.Black, BooleanOP.DIFFERENCE);

            Box b1 = new Box(this.Position,
                             arrowDir*60 + right*60 + normal*40,
                             Color.Black, BooleanOP.DIFFERENCE);

            floor.AddChildObject(c1, false);
            floor.AddChildObject(c2, false);
            floor.AddChildObject(b1, false);

            Box inside = new Box(this.Position-normal*20,
                                 arrowDir*120 + right*80 + normal*40,
                                 Color.Gray);

            Cylinder ic1 = new Cylinder(normal*50 + arrowDir*30,
                                        normal,
                                        48, 28, Color.Black, BooleanOP.DIFFERENCE);

            Cylinder ic2 = new Cylinder(normal*50 -arrowDir*30,
                                        normal,
                                        48, 28, Color.Black, BooleanOP.DIFFERENCE);

            Box ib1 = new Box(normal*20,
                             arrowDir*56 + right*56 + normal*36,
                             Color.Black, BooleanOP.DIFFERENCE);

            inside.AddChildObject(ic1,true);
            inside.AddChildObject(ic2,true);
            inside.AddChildObject(ib1,true);

            Map.instance.staticObjectList.Add(inside);

            Cylinder rotator = new Cylinder(this.Position + arrowDir*55 + right*10,
                                            right, 20, 5, Color.DarkGray);

            Map.instance.staticObjectList.Add(rotator);

            // needs direction change - plateFrame, glassPlate
            // correct orientation - arrow
            Box plateFrame = new Box(this.Position - normal*5,
                                     arrowDir*10 + right*80 + new Vector3(0,0,150),
                                     Color.Black);

            plateFrame.AddChildObject(new Capsule(new Vector3(0,0,-58), 60, 28, Color.Red, BooleanOP.INTERSECT), true);
            plateFrame.AddChildObject(new Capsule(new Vector3(0,0,-55), 64, 24, Color.Red, BooleanOP.DIFFERENCE), true);

            objList.Add(plateFrame);


            Box arrow = new Box(this.Position -arrowDir*30 - normal*5,
                                arrowDir*10 + right*20 + normal*10,
                                Color.Aqua);

            arrow.AddChildObject(new Box(arrowDir*20, 
                                         arrowDir*30 + right*35 + normal*10,
                                         Color.Aqua), true);

            arrow.AddChildObject(new Plane(arrowDir*22, -arrowDir-right, Color.Black, BooleanOP.DIFFERENCE), true);
            arrow.AddChildObject(new Plane(arrowDir*22, -arrowDir+right, Color.Black, BooleanOP.DIFFERENCE), true);

            objList.Add(arrow);

            glassPlate = new Box(this.Position - normal*3,
                                 arrowDir*4 + right*80 + new Vector3(0,0,150),
                                 Color.Black, boundingBoxSize:new Vector3(120,120,120));
            glassPlate.AddChildObject(new Capsule(new Vector3(0,0,-58), 60, 28, Color.Black, BooleanOP.INTERSECT), true);
            glassPlate.SetTransparent(true);

            Map.instance.dynamicObjectList.Add(glassPlate);
            objList.Add(glassPlate);

            Box wiring1 = new Box(this.Position - normal*5,
                                  arrowDir*115 + right*4 + normal*8,
                                  Color.Black);

            Box wiring2 = new Box(this.Position - normal*5,
                                  arrowDir*4 + right*50 + normal*8,
                                  Color.Black);
            wiring2.SetRepetition(new Vector3((float)Math.Abs(arrowDir.X),
                                              (float)Math.Abs(arrowDir.Y),
                                              (float)Math.Abs(arrowDir.Z)), 30);

            objList.Add(wiring1);
            objList.Add(wiring2);

            PhysicsTrigger trigger1 = null;
            PhysicsTrigger trigger2 = null;

            if (normal.Z == 1 || normal.Z == -1)
            {
                glassPlate.Rotate(90, right, glassPlate.Position);
                plateFrame.Rotate(90, right, plateFrame.Position);

                trigger1 = new PhysicsTrigger(this.Position + normal*50 + arrowDir*15, 30);
                trigger2 = new PhysicsTrigger(this.Position + normal*50 - arrowDir*15, 30);
            }
            else
            {
                trigger1 = new PhysicsTrigger(this.Position + normal*25 + arrowDir*25, 25);
                trigger2 = new PhysicsTrigger(this.Position + normal*25 - arrowDir*25, 25);
            }

            trigger1.onCollisionEnter += this.TriggerEnter;
            trigger2.onCollisionEnter += this.TriggerEnter;

            Map.instance.physicsObjectsList.Add(trigger1);
            Map.instance.physicsObjectsList.Add(trigger2);

            /* Map.instance.infoObjectList.Add(new Sphere(this.Position + normal*50 + arrowDir*15, 30, Color.Lime)); */
            /* Map.instance.infoObjectList.Add(new Sphere(this.Position + normal*50 - arrowDir*15, 30, Color.Green)); */

            this.boundingBoxSize = arrowDir*120 + right*60 + normal*80;
            this.boundingBox = new Box(this.Position + normal*40,
                                       this.boundingBoxSize,
                                       Color.Red);
        }

        public override SDFout SDF(Vector3 testPos, float minDist, bool physics=false)
        {
            SDFout best = new SDFout(float.MaxValue, Color.Pink);
            SDFout test;

            foreach (var obj in objList)
            {
                if (obj == glassPlate) continue;

                test = obj.SDF(testPos, minDist, physics);

                if (test.distance < best.distance)
                {
                    best = test;
                }
            }

            return best;
        }


        bool jumperReady = true;
        async Task RotateUpDown(bool up)
        {
            if (up) 
                await Task.Delay(150);

            for (int i = 0; i < (up ? 3 : 15); i++)
            {
                foreach (var obj in objList)
                {
                    obj.Rotate(up ? 15 : -3, right, this.Position + arrowDir*55);
                }
                await Task.Delay(2).ContinueWith(t1 => { while(Screen.instance.DrawPhase) { } });
            }

            if (!up) // finished rewinding
            {
                await Task.Delay(50);
                jumperReady = true;
            }
            else
            {
                await Task.Delay(50);
                RotateUpDown(!up);
            }
        }

        public override void EventListener(Interactable obj, int state) 
        { 
            Interact();
        }

        public void TriggerEnter(IPortalable obj, PhysicsTrigger _)
        {
            if (obj is PhysicsObject && Player.instance.grabbing == obj) return;
            if (!jumperReady) return;

            jumperReady = false;
            if (obj is Player) (obj as Player).grounded = false;
            obj.SetVelocity(this.jumperDirection*this.jumperStrength);
            Interact();
        }

        public override void Interact()
        {
            RotateUpDown(true);
        }
    }
}
