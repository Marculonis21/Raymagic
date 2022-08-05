using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class LaserCatcher : Interactable
    {
        Vector3 normal;
        Object ground;
        public LaserCatcher(Vector3 position, Vector3 normal, Object ground, Color secondaryColor) : base(position, secondaryColor)
        {
            this.normal = Vector3.Normalize(normal);
            this.ground = ground;

            this.stateCount = 2;
        }

        public override void ObjectSetup(ref List<Object> staticObjectList, ref List<Object> dynamicObjectList, ref List<PhysicsObject> physicsObjectsList)
        {

            Vector3 right = Vector3.Cross(normal, new Vector3(0,0,1));
            if (normal.Z == 1 || normal.Z == -1)
            {
                right = new Vector3(1,0,0);
            }
            Vector3 up = Vector3.Cross(normal, -right);

            Box empty = new Box(this.Position-normal*9, up*35 + right*35 + normal*20, Color.Black, BooleanOP.DIFFERENCE);
            this.ground.AddChildObject(empty, false);

            Box inside = new Box(this.Position - normal*5, up*35 + right*35 + normal*10, Color.Gray)  ;
            inside.AddChildObject(new Box(normal*4, up*31 + right*31 + normal*10, Color.Black, BooleanOP.DIFFERENCE), true);

            Box wiring1 = new Box(this.Position - normal*5, right*40 + up*4 + normal*4, Color.Black);
            Box wiring2 = new Box(this.Position - normal*5, right*4 + up*40 + normal*4, Color.Black);
            wiring1.Rotate(45, normal, wiring1.Position);
            wiring2.Rotate(45, normal, wiring2.Position);

            inside.AddChildObject(wiring1, false);
            inside.AddChildObject(wiring2, false);
            staticObjectList.Add(inside);

            Cylinder base1 = new Cylinder(this.Position + normal*2, normal, 18, 12, Color.White); 
            base1.AddChildObject(new Sphere(normal*5, 12, Color.Gray, BooleanOP.DIFFERENCE), true);
            base1.AddChildObject(new Box(normal*-5, right*25 + up*6 + normal*11, secondaryColor), true);
            base1.AddChildObject(new Box(normal*-5, right*6 + up*25 + normal*11, secondaryColor), true);
            base1.AddChildObject(new Sphere(normal*5, 12, Color.Gray, BooleanOP.DIFFERENCE), true);
            base1.AddChildObject(new Sphere(normal*-10, 12, new Color(30,0,0)), true);

            Cylinder base2 = new Cylinder(this.Position + normal*2, normal, 18, 12, Color.White); 
            base2.AddChildObject(new Sphere(normal*5, 12, Color.Gray, BooleanOP.DIFFERENCE), true);
            base2.AddChildObject(new Box(normal*-5, right*25 + up*6 + normal*11, secondaryColor), true);
            base2.AddChildObject(new Box(normal*-5, right*6 + up*25 + normal*11, secondaryColor), true);
            base2.AddChildObject(new Sphere(normal*5, 12, Color.Gray, BooleanOP.DIFFERENCE), true);
            base2.AddChildObject(new Sphere(normal*-10, 12, Color.Red), true);

            this.modelStates.Add(base1);
            this.modelStates.Add(base2);

            this.boundingBoxSize = up*25 + right*25 + normal*25;
            this.boundingBox = new Box(this.Position,
                                       this.boundingBoxSize,
                                       Color.Lime);
        }
    }
}
