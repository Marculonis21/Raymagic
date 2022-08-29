using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class LoadingMap
    {
        static Vector3 loadingMapSize = new Vector3(600,300,500);

        public LoadingMap()
        {
        } 

        public static void AddLoadingMaps(ref MapData data)
        {
            /* data.inDoor; */
            /* data.outDoor; */

            var inDirection = -data.outDoor.facing;
            var inDirectionRight = Vector3.Cross(data.outDoor.facing, new Vector3(0,0,1));
            var inDirectionUp = new Vector3(0,0,1);

            var inPosition = data.outDoor.Position;

            var inLoadingBC = inPosition + (inDirection * loadingMapSize.X + inDirectionRight * -loadingMapSize.Y/2 + inDirectionUp * -loadingMapSize.Z/2);
            var inLoadingTC = inPosition + (inDirection * 0                + inDirectionRight *  loadingMapSize.Y/2 + inDirectionUp *  loadingMapSize.Z/2);
            Console.WriteLine($"BC: {inLoadingBC}, TC: {inLoadingTC}");

            // set new bot/top corner size
            data.botCorner.Deconstruct(out float bcx, out float bcy, out float bcz);
            if (inLoadingBC.X < bcx)
            {
                bcx = inLoadingBC.X;
            }
            if (inLoadingBC.Y < bcy)
            {
                bcy = inLoadingBC.Y;
            }
            if (inLoadingBC.Z < bcz)
            {
                bcz = inLoadingBC.Z;
            }
            data.botCorner = new Vector3(bcx,bcy,bcz);

            data.topCorner.Deconstruct(out float tcx, out float tcy, out float tcz);
            if (inLoadingTC.X > tcx)
            {
                tcx = inLoadingTC.X;
            }
            if (inLoadingTC.Y > tcy)
            {
                tcy = inLoadingTC.Y;
            }
            if (inLoadingTC.Z > tcz)
            {
                tcz = inLoadingTC.Z;
            }
            data.topCorner = new Vector3(tcx,tcy,tcz);

            Plane inFloor = new Plane(inPosition + -inDirectionUp*248,  inDirectionUp, Color.Black);
            Plane roof    = new Plane(inPosition +  inDirectionUp*248, -inDirectionUp, Color.Beige);

            Plane wall1   = new Plane(inPosition +  inDirectionRight*148, -inDirectionRight, Color.Black);
            Plane wall2   = new Plane(inPosition + -inDirectionRight*148,  inDirectionRight, Color.Black);
            Plane wall3   = new Plane(inPosition + inDirection*5,    inDirection, Color.Beige);
            Plane wall4   = new Plane(inPosition + inDirection*598, -inDirection, Color.Beige);

            Plane inIntersect1 = new Plane(inPosition, -inDirection, Color.Black, BooleanOP.INTERSECT);
            Plane inIntersect2 = new Plane(inPosition + inDirection*610, inDirection, Color.Black, BooleanOP.INTERSECT);

            wall1.AddChildObject(inIntersect1, false);
            wall1.AddChildObject(inIntersect2, false);

            wall2.AddChildObject(inIntersect1, false);
            wall2.AddChildObject(inIntersect2, false);

            wall3.AddChildObject(inIntersect1, false);
            wall3.AddChildObject(inIntersect2, false);

            wall4.AddChildObject(inIntersect1, false);
            wall4.AddChildObject(inIntersect2, false);

            inFloor.AddChildObject(inIntersect1, false);
            inFloor.AddChildObject(inIntersect2, false);

            roof.AddChildObject(inIntersect1, false);
            roof.AddChildObject(inIntersect2, false);

            data.staticMapObjects.Add(wall1);
            data.staticMapObjects.Add(wall2);
            /* data.staticMapObjects.Add(wall3); */
            data.staticMapObjects.Add(wall4);
            data.staticMapObjects.Add(roof);
            data.staticMapObjects.Add(inFloor);

/*             data.staticMapObjects.Add(new Box(inPosition + inDirection*50, new Vector3(20,20,20), Color.Red)); */

            Box side = new Box(inPosition + inDirection*300,
                               inDirection*600 + inDirectionRight*10 + inDirectionUp*15,
                               Color.Gray);
            if (inDirection.X == 1 || inDirection.X == -1)
            {
                side.SetSymmetry("Y",new Vector3(0,60,0));
            }
            else
            {
                side.SetSymmetry("X",new Vector3(60,0,0));
            }
            data.staticMapObjects.Add(side);


            Box floorTile = new Box(inPosition + inDirection*300,
                                    new Vector3(12,12,3),
                                    Color.DarkGray, 
                                    boundingBoxSize: inDirection*600 + inDirectionRight*120 + inDirectionUp * 20);
            floorTile.AddChildObject(new Cylinder(inDirectionUp*-10, new Vector3(0,0,-1), 20, 5, Color.Black, BooleanOP.DIFFERENCE), true);
            if (inDirection.X == 1 || inDirection.X == -1)
            {
                floorTile.SetRepetition(new Vector3(25,4,0), 12);
            }
            else
            {
                floorTile.SetRepetition(new Vector3(4,25,0), 12);
            }

            data.dynamicMapObjects.Add(floorTile);

            Box railing = new Box(inPosition + inDirection*300 + inDirectionUp*50,
                                  inDirection*30 + inDirectionRight*5 + inDirectionUp*5,
                                  Color.Orange,
                                  boundingBoxSize: inDirection*600 + inDirectionRight*140 + inDirectionUp*100);
            railing.AddChildObject(new Cylinder(new Vector3(), new Vector3(0,0,1),50,2,Color.Red), true);

            if (inDirection.X == 1 || inDirection.X == -1)
            {
                railing.SetSymmetry("Y",new Vector3(0,60,0));
                railing.SetRepetition(new Vector3(9,0,0), 30);
            }
            else
            {
                railing.SetSymmetry("X",new Vector3(60,0,0));
                railing.SetRepetition(new Vector3(0,9,0), 30);
            }
            
            data.dynamicMapObjects.Add(railing);


            data.mapLights.Add(new Light(inPosition + inDirection*300 + inDirectionUp*-240, 
                                         Color.White, 
                                         20000, 
                                         inLoadingBC,
                                         inLoadingTC));
        }
    }
}
