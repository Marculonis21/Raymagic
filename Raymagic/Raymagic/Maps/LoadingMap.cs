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
            if (data.inDoor != null)
            {
                Console.WriteLine("add indoor");
                data.inDoor.IN = true;
                data.inDoor.doorClosedEvent += Map.instance.LoadingDoorClosed;
                AddToDoor(ref data, data.inDoor, true);
            }

            if (data.outDoor != null)
            {
                Console.WriteLine("add outdoor");
                data.outDoor.IN = false;
                data.outDoor.doorClosedEvent += Map.instance.LoadingDoorClosed;
                AddToDoor(ref data, data.outDoor, false);
            }

        }

        static void AddToDoor(ref MapData data, Door2 door, bool IN)
        {
            Vector3 inDirection = IN ? door.facing : -door.facing;

            var inDirectionRight = Vector3.Cross(inDirection, new Vector3(0,0,1));
            // hack for negative directions
            inDirectionRight = new Vector3(Math.Abs(inDirectionRight.X),Math.Abs(inDirectionRight.Y),Math.Abs(inDirectionRight.Z)); 
            var inDirectionUp = new Vector3(0,0,1);

            var inPosition = door.Position;

            if (IN)
            {
                data.playerSpawn = inPosition + inDirection*400 + inDirectionUp*100;

                data.levelStartAnchor = inPosition + inDirection*300 + inDirectionUp*75;
                PhysicsTrigger inDoorTrigger = new PhysicsTrigger(inPosition + inDirection*100 + inDirectionUp*50, 100);
                inDoorTrigger.onCollisionEnter += data.inDoor.TriggerEnter;
                inDoorTrigger.onCollisionExit += data.inDoor.TriggerExit;
                data.physicsMapObjects.Add(inDoorTrigger);
            }
            else
            {
                data.levelEndAnchor = inPosition + inDirection*300 + inDirectionUp*75;
            }

            Vector3 inLoadingBC;
            Vector3 inLoadingTC;

            if (inDirection.X > 0 || inDirection.Y > 0)
            {
                inLoadingBC = inPosition + (inDirection * 0                + inDirectionRight * -loadingMapSize.Y/2 + inDirectionUp * -loadingMapSize.Z/2);
                inLoadingTC = inPosition + (inDirection * loadingMapSize.X + inDirectionRight *  loadingMapSize.Y/2 + inDirectionUp *  loadingMapSize.Z/2);
            }
            else
            {
                inLoadingBC = inPosition + (inDirection * loadingMapSize.X + inDirectionRight * -loadingMapSize.Y/2 + inDirectionUp * -loadingMapSize.Z/2);
                inLoadingTC = inPosition + (inDirection * 0                + inDirectionRight * +loadingMapSize.Y/2 + inDirectionUp *  loadingMapSize.Z/2);
            }
            /* Console.WriteLine($"BC: {inLoadingBC}, TC: {inLoadingTC}"); */

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

            Plane inIntersect1 = new Plane(inPosition,                  -inDirection, Color.Black, BooleanOP.INTERSECT);
            Plane inIntersect2 = new Plane(inPosition + inDirection*610, inDirection, Color.Black, BooleanOP.INTERSECT);

            Plane inIntersect3 = new Plane(inPosition +  inDirectionRight*155, inDirectionRight, Color.Black, BooleanOP.INTERSECT);
            Plane inIntersect4 = new Plane(inPosition + -inDirectionRight*155,-inDirectionRight, Color.Black, BooleanOP.INTERSECT);

            Plane inIntersect5 = new Plane(inPosition + -inDirectionUp*255, -inDirectionUp, Color.Black, BooleanOP.INTERSECT);
            Plane inIntersect6 = new Plane(inPosition +  inDirectionUp*255, inDirectionUp, Color.Black, BooleanOP.INTERSECT);

            wall1.AddChildObject(inIntersect1, false);
            wall1.AddChildObject(inIntersect2, false);
            wall1.AddChildObject(inIntersect3, false);
            wall1.AddChildObject(inIntersect4, false);
            wall1.AddChildObject(inIntersect5, false);
            wall1.AddChildObject(inIntersect6, false);

            wall2.AddChildObject(inIntersect1, false);
            wall2.AddChildObject(inIntersect2, false);
            wall2.AddChildObject(inIntersect3, false);
            wall2.AddChildObject(inIntersect4, false);
            wall2.AddChildObject(inIntersect5, false);
            wall2.AddChildObject(inIntersect6, false);

            wall3.AddChildObject(inIntersect1, false);
            wall3.AddChildObject(inIntersect2, false);
            wall3.AddChildObject(inIntersect3, false);
            wall3.AddChildObject(inIntersect4, false);
            wall3.AddChildObject(inIntersect5, false);
            wall3.AddChildObject(inIntersect6, false);

            wall4.AddChildObject(inIntersect1, false);
            wall4.AddChildObject(inIntersect2, false);
            wall4.AddChildObject(inIntersect3, false);
            wall4.AddChildObject(inIntersect4, false);
            wall4.AddChildObject(inIntersect5, false);
            wall4.AddChildObject(inIntersect6, false);

            inFloor.AddChildObject(inIntersect1, false);
            inFloor.AddChildObject(inIntersect2, false);
            inFloor.AddChildObject(inIntersect3, false);
            inFloor.AddChildObject(inIntersect4, false);
            inFloor.AddChildObject(inIntersect5, false);
            inFloor.AddChildObject(inIntersect6, false);

            roof.AddChildObject(inIntersect1, false);
            roof.AddChildObject(inIntersect2, false);
            roof.AddChildObject(inIntersect3, false);
            roof.AddChildObject(inIntersect4, false);
            roof.AddChildObject(inIntersect5, false);
            roof.AddChildObject(inIntersect6, false);

            data.staticMapObjects.Add(wall1);
            data.staticMapObjects.Add(wall2);
            data.staticMapObjects.Add(wall4);
            data.staticMapObjects.Add(roof);
            data.staticMapObjects.Add(inFloor);

            //NOT TESTED AS MUCH
            Door2 outLoadingDoor;
            if (IN)
            {
                outLoadingDoor = new Door2(inPosition + inDirection*598, inDirection, wall4, Color.Blue, 1);
            }
            else
            {
                outLoadingDoor = new Door2(inPosition + inDirection*598, -inDirection, wall4, Color.Blue, 1);
            }
            data.interactableObjectList.Add(outLoadingDoor);
            //

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

            /* if (IN) */
            /* { */
            /*     Console.WriteLine(inPosition + inDirection*300 + inDirectionUp*-240); */
            /*     Console.WriteLine(inLoadingBC); */
            /*     Console.WriteLine(inLoadingTC); */
            /* } */

            data.mapLights.Add(new Light(inPosition + inDirection*300 + inDirectionUp*-240, 
                                         Color.White, 
                                         20000, 
                                         inLoadingBC,
                                         inLoadingTC));

            data.mapLights.Add(new Light(inPosition + inDirection*100 + inDirectionUp*100, 
                                         Color.Gray, 
                                         1000, 
                                         inLoadingBC,
                                         inLoadingTC));
        }
    }
}
