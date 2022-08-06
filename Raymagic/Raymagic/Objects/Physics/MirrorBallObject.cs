using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class MirrorBall : PhysicsObject
    {
        public Vector3 outDir {get; private set;}

        public MirrorBall(Vector3 position, float size, Color color1, Color color2) : base(position, size, color1, color2)
        {
            outDir = new Vector3(1,0,0);
        }

        public override void ObjectSetup()
        {
            this.AddChildObject(new Cylinder(new Vector3(size+5,0,0),
                                             new Vector3(1,0,0),
                                             (size+5)*2,
                                             size-11,
                                             Color.Black,
                                             BooleanOP.DIFFERENCE), true);
            this.AddChildObject(new Cylinder(new Vector3(0,size+5,0),
                                             new Vector3(0,1,0),
                                             (size+5)*2,
                                             size-11,
                                             Color.Black,
                                             BooleanOP.DIFFERENCE), true);
            this.AddChildObject(new Cylinder(new Vector3(0,0,size+5),
                                             new Vector3(0,0,1),
                                             (size+5)*2,
                                             size-11,
                                             Color.Black,
                                             BooleanOP.DIFFERENCE), true);

            /* this.AddChildObject(new Sphere(new Vector3( 15, 15, 15), 4, Color.Black, BooleanOP.DIFFERENCE), true); */
            /* this.AddChildObject(new Sphere(new Vector3( 15,-15, 15), 4, Color.Black, BooleanOP.DIFFERENCE), true); */
            /* this.AddChildObject(new Sphere(new Vector3(-15, 15, 15), 4, Color.Black, BooleanOP.DIFFERENCE), true); */
            /* this.AddChildObject(new Sphere(new Vector3(-15,-15, 15), 4, Color.Black, BooleanOP.DIFFERENCE), true); */
            /* this.AddChildObject(new Sphere(new Vector3( 15, 15,-15), 4, Color.Black, BooleanOP.DIFFERENCE), true); */
            /* this.AddChildObject(new Sphere(new Vector3( 15,-15,-15), 4, Color.Black, BooleanOP.DIFFERENCE), true); */
            /* this.AddChildObject(new Sphere(new Vector3(-15, 15,-15), 4, Color.Black, BooleanOP.DIFFERENCE), true); */
            /* this.AddChildObject(new Sphere(new Vector3(-15,-15,-15), 4, Color.Black, BooleanOP.DIFFERENCE), true); */

            Sphere mirror = new Sphere(position, size-5, color2);
            mirror.SetTransparent(true);
            mirror.AddChildObject(new Sphere(new Vector3(), size-10, Color.Black, BooleanOP.DIFFERENCE), true);
            this.AddChildObject(mirror, false);
        }
    }
}
