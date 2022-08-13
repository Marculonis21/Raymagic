using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class MirrorBall : PhysicsObject
    {
        public Vector3 outDir;

        public MirrorBall(Vector3 position, float size, Color color1, Color color2) : base(position, size, color1, color2)
        {
            outDir = new Vector3(1,0,0);
        }

        public override void ObjectSetup()
        {
            Cylinder c1 = new Cylinder(new Vector3(0,0,0),
                                       new Vector3(1,0,0),
                                       size+20,
                                       size-11,
                                       Color.Black,
                                       BooleanOP.DIFFERENCE);
            c1.SetSymmetry("X", new Vector3(40,0,0));
            this.AddChildObject(c1, true);

            Cylinder c2 = new Cylinder(new Vector3(0,0,0),
                                       new Vector3(0,1,0),
                                       size+20,
                                       size-11,
                                       Color.Black,
                                       BooleanOP.DIFFERENCE);
            c2.SetSymmetry("Y", new Vector3(0,40,0));
            this.AddChildObject(c2, true);

            Cylinder c3 = new Cylinder(new Vector3(0,0,0),
                                       new Vector3(0,0,1),
                                       size+20,
                                       size-11,
                                       Color.Black,
                                       BooleanOP.DIFFERENCE);
            c3.SetSymmetry("Z", new Vector3(0,0,40));
            this.AddChildObject(c3, true);

            Sphere dimples = new Sphere(new Vector3(0,0,0), 5, Color.Black, BooleanOP.DIFFERENCE);
            dimples.SetSymmetry("XYZ", new Vector3(15,15,15));
            this.AddChildObject(dimples, true);

            Sphere mirror = new Sphere(position, size-5, color2);
            mirror.SetTransparent(true);
            mirror.AddChildObject(new Sphere(new Vector3(), size-10, Color.Black, BooleanOP.DIFFERENCE), true);
            this.AddChildObject(mirror, false);
        }

        public override void UpdateRotation()
        {
            this.AddChildObject(new Sphere(this.outDir*1, 1, Color.Black), true); //guide
            base.UpdateRotation();
            this.outDir = this.childObjects[this.childObjects.Count - 1].Position - this.Position;
            this.childObjects.RemoveAt(this.childObjects.Count - 1);
        }
    }
}
