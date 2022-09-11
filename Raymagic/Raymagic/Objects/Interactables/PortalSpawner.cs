using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class PortalSpawner : Interactable
    {
        Vector3 facing;
        int portalType;
        bool startEnabled;

        List<Object> objList = new List<Object>();

        public PortalSpawner(Vector3 position, Vector3 facing, int portalType, Color secondaryColor, bool startEnabled=false) : base(position, secondaryColor)
        {
            this.facing = facing;
            this.portalType = portalType;
            this.stateCount = 2;
            this.startEnabled = startEnabled;

            if (portalType != 0 && portalType != 1)
            {
                throw new Exception($"Unknown portalType while creating PortalSpawner - {portalType}");
            }

        }

        public override void ObjectSetup(ref List<Object> staticObjectList, ref List<Object> dynamicObjectList, ref List<PhysicsObject> physicsObjectsList)
        {
            facing.Normalize();
            Vector3 right = Vector3.Cross(facing, new Vector3(0,0,1));
            right = new Vector3((float)Math.Abs(right.X), (float)Math.Abs(right.Y), (float)Math.Abs(right.Z));
            Vector3 up = Vector3.Cross(facing, -right);

            Box spawnerSide = new Box(this.Position, facing*5 + right*10 + up*100, Color.Gray);
            spawnerSide.AddChildObject(new Cylinder(facing*1 + up*50, up, 100, 3, this.secondaryColor), true);

            Sphere typeIndicator = new Sphere(this.Position, 8, this.portalType == 0 ? Color.Orange : Color.Blue);
            typeIndicator.SetRepetition(new Vector3(0,0,2), 20);
            typeIndicator.AddChildObject(new Plane(facing*1, facing, Color.Black, BooleanOP.INTERSECT), true);
            if (facing == new Vector3(1,0,0) || facing == new Vector3(-1,0,0))
            {
                spawnerSide.SetSymmetry("Y", right*50);
                typeIndicator.SetSymmetry("Y", right*47);
            }
            if (facing == new Vector3(0,1,0) || facing == new Vector3(0,-1,0))
            {
                spawnerSide.SetSymmetry("X", right*50);
                typeIndicator.SetSymmetry("X", right*47);
            }

            staticObjectList.Add(spawnerSide);
            staticObjectList.Add(typeIndicator);

            this.boundingBoxSize = right*110 + facing*10 + up*110;
            this.boundingBox = new Box(this.Position,
                                       this.boundingBoxSize,
                                       Color.Black);
        }

        public override void ObjectStartup()
        {
            if (startEnabled)
            {
                Interact();
            }
        }

        public override SDFout SDF(Vector3 testPos, float minDist, out bool IsTransparent)
        {
            SDFout best = new SDFout(float.MaxValue, Color.Pink);
            /* SDFout test; */
            IsTransparent = false;

            /* foreach (var obj in objList) */
            /* { */
            /*     test = obj.SDF(testPos, minDist, out _); */

            /*     if (test.distance < best.distance) */
            /*     { */
            /*         best = test; */
            /*     } */
            /* } */

            return best;
        }

        public override void Interact()
        {
            base.Interact();

            if (this.state == 1)
            {
                Map.instance.portalList[this.portalType] = new Portal(this.Position, this.facing, this.portalType);
            }
            else 
            {
                if (Map.instance.portalList[this.portalType] != null && Map.instance.portalList[this.portalType].Position == this.Position)
                {
                    Map.instance.portalList[this.portalType] = null;
                }
                else
                {
                    Interact();
                }
            }
        }
    }
}
