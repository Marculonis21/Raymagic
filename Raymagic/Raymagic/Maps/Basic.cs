using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Basic
    {
        public Basic()
        {
            /* MapData data = new MapData(); */

            /* data.topCorner = new Vector3(800,800,500); */
            /* data.botCorner = new Vector3(  0,  0,-200); */

            /* Box area = new Box(new Vector3(400,400,250), */
            /*                    new Vector3(800,800,500), */
            /*                    Color.White); */

            /* //floor height 50 */
            /* area.AddBoolean(BooleanOP.DIFFERENCE, */
            /*                 new Box(new Vector3(), */
            /*                         new Vector3(600,600,400), */
            /*                         Color.Black)); */

            /* area.AddBoolean(BooleanOP.DIFFERENCE, */
            /*                 new Box(new Vector3(150,150,-250), */
            /*                         new Vector3(201,201,400), */
            /*                         Color.Black)); */
            
            /* data.staticMapObjects.Add(area); */

            /* Box pillar = new Box(new Vector3(300,300,250), */
            /*                      new Vector3(30, 30, 500), */
            /*                      Color.Red); */

            /* data.staticMapObjects.Add(pillar); */

            /* Box dip = new Box(new Vector3(550,550,-50), */
            /*                   new Vector3(300,300,201), */
            /*                   Color.Blue); */

            /* dip.AddBoolean(BooleanOP.DIFFERENCE, */
            /*                new Box(new Vector3(0,0,50), */
            /*                        new Vector3(200,200,200), */
            /*                        Color.Black)); */

            /* data.staticMapObjects.Add(dip); */


            /* Box dipStairs = new Box(new Vector3(550,530,-100), */
            /*                         new Vector3(50,30,50), */
            /*                         Color.Green); */

            /* dipStairs.AddBoolean(BooleanOP.UNION, */
            /*                      new Box(new Vector3(0,-20,0), */
            /*                              new Vector3(50,30,100), */
            /*                              Color.Black)); */
            /* dipStairs.AddBoolean(BooleanOP.UNION, */
            /*                      new Box(new Vector3(0,-40,0), */
            /*                              new Vector3(50,30,150), */
            /*                              Color.Black)); */
            /* dipStairs.AddBoolean(BooleanOP.UNION, */
            /*                      new Box(new Vector3(0,-60,0), */
            /*                              new Vector3(50,30,200), */
            /*                              Color.Black)); */
            /* dipStairs.AddBoolean(BooleanOP.UNION, */
            /*                      new Box(new Vector3(0,-80,0), */
            /*                              new Vector3(50,30,250), */
            /*                              Color.Black)); */

            /* dipStairs.AddBoolean(BooleanOP.DIFFERENCE, */
            /*                      new Sphere(new Vector3(0,-125,0), */
            /*                                 100, */
            /*                                 Color.Black)); */

            /* data.staticMapObjects.Add(dipStairs); */


            /* Box tunnel = new Box(new Vector3(550,400,50), */
            /*                      new Vector3(300,100,800), */
            /*                      Color.Gray); */

            /* tunnel.AddBoolean(BooleanOP.DIFFERENCE, */
            /*                   new Box(new Vector3(), */
            /*                           new Vector3(40,140,200), */
            /*                           Color.Black)); */


            /* data.staticMapObjects.Add(tunnel); */

            /* Box tunnel2 = new Box(new Vector3(400,550,50), */
            /*                       new Vector3(100,400,800), */
            /*                       Color.Gray); */

            /* tunnel2.AddBoolean(BooleanOP.UNION, */
            /*                    new Sphere(new Vector3(0,0,50), */
            /*                               80, */
            /*                               Color.Black)); */

            /* data.staticMapObjects.Add(tunnel2); */

            /* Box platform = new Box(new Vector3(200,200,50), */
            /*                        new Vector3(20,100,25), */
            /*                        Color.Orange); */

            /* platform.AddBoolean(BooleanOP.UNION, */
            /*                     new Box(new Vector3(20,0,0), */
            /*                             new Vector3(20,100,50), */
            /*                             Color.Black)); */
            /* platform.AddBoolean(BooleanOP.UNION, */
            /*                     new Box(new Vector3(40,0,0), */
            /*                             new Vector3(20,100,75), */
            /*                             Color.Black)); */
            /* platform.AddBoolean(BooleanOP.UNION, */
            /*                     new Box(new Vector3(60,0,0), */
            /*                             new Vector3(20,100,100), */
            /*                             Color.Black)); */
            /* platform.AddBoolean(BooleanOP.UNION, */
            /*                     new Box(new Vector3(80,0,0), */
            /*                             new Vector3(20,100,125), */
            /*                             Color.Black)); */

            /* data.staticMapObjects.Add(platform); */

            /* Box box = new Box(new Vector3(400,200,75), */
            /*                   new Vector3(100,20,50), */
            /*                   Color.Aqua, false, */ 
            /*                   new Vector3(100,100,60),info:"rotator"); */

            /* box.AddBoolean(BooleanOP.DIFFERENCE, new Sphere(new Vector3(0,0,-30), */ 
            /*                                            50, */ 
            /*                                            Color.Black, */ 
            /*                                            false)); */

            /* data.dynamicMapObjects.Add(box); */

            /* Sphere sphere = new Sphere(new Vector3(200,400,150), */
            /*                            50, */
            /*                            Color.Lime, false, */ 
            /*                            new Vector3(110,110,320),info:"sphere"); */

            /* data.dynamicMapObjects.Add(sphere); */

            /* Light light = new Light(new Vector3(600,550,200), */
            /*                         50); */
            /* data.mapLights.Add(light); */

            /* Light light2 = new Light(new Vector3(250,200,350), */
            /*                          100); */
            /* data.mapLights.Add(light2); */

            /* data.playerSpawn = new Vector3(5,5,1); */

            /* Map.instance.AddMap("basic", data); */
        }
    }
}