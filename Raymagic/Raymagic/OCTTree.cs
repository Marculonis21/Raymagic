using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public static class OCTTree
    {
        public static OCTTreeNode root;
        public static float maxGroupError;

        public static Vector3 rootPosition;
        public static float rootSize;
        public static float nodeMinSize;

        public static void OCTTFromDistanceMap(SDFout[,,] distanceMap, float allowedError)
        {
            Map map = Map.instance;
            Vector3 center = (map.mapTopCorner - map.mapOrigin)/2;
            Vector3 size = map.mapTopCorner - map.mapOrigin;

            maxGroupError = allowedError;

            Console.WriteLine("Creating octtree");
            /* root = new OCTTreeNode(center.X, center.Y, center.Z, size.X, map.distanceMapDetail); */

            rootPosition = center;
            rootSize = size.X;
            nodeMinSize = map.distanceMapDetail;

            root = new OCTTreeNode();
            root.Subdivide();

            int dmLenX = distanceMap.GetLength(0);
            int dmLenY = distanceMap.GetLength(1);
            int dmLenZ = distanceMap.GetLength(2);

            Parallel.For(0, 8, new ParallelOptions{MaxDegreeOfParallelism = 8}, i => // it works in parallel!
            {
                int xStart;
                int yStart;
                int zStart;

                int xEnd;
                int yEnd;
                int zEnd;
                if(i == 0)
                {
                    xStart = 0;
                    yStart = 0;
                    zStart = 0;

                    xEnd = dmLenX/2;
                    yEnd = dmLenY/2;
                    zEnd = dmLenZ/2;
                }
                else if(i == 1)
                {
                    xStart = dmLenX/2;
                    yStart = 0;
                    zStart = 0;

                    xEnd = dmLenX;
                    yEnd = dmLenY/2;
                    zEnd = dmLenZ/2;
                }
                else if(i == 2)
                {
                    xStart = 0;
                    yStart = dmLenY/2;
                    zStart = 0;

                    xEnd = dmLenX/2;
                    yEnd = dmLenY;
                    zEnd = dmLenZ/2;
                }
                else if(i == 3)
                {
                    xStart = dmLenX/2;
                    yStart = dmLenY/2;
                    zStart = 0;

                    xEnd = dmLenX;
                    yEnd = dmLenY;
                    zEnd = dmLenZ/2;
                }
                else if(i == 4)
                {
                    xStart = 0;
                    yStart = 0;
                    zStart = dmLenZ/2;

                    xEnd = dmLenX/2;
                    yEnd = dmLenY/2;
                    zEnd = dmLenZ;
                }
                else if(i == 5)
                {
                    xStart = dmLenX/2;
                    yStart = 0;
                    zStart = dmLenZ/2;

                    xEnd = dmLenX;
                    yEnd = dmLenY/2;
                    zEnd = dmLenZ;
                }
                else if(i == 6)
                {
                    xStart = 0;
                    yStart = dmLenY/2;
                    zStart = dmLenZ/2;

                    xEnd = dmLenX/2;
                    yEnd = dmLenY;
                    zEnd = dmLenZ;
                }
                else
                {
                    xStart = dmLenX/2;
                    yStart = dmLenY/2;
                    zStart = dmLenZ/2;

                    xEnd = dmLenX;
                    yEnd = dmLenY;
                    zEnd = dmLenZ;
                }

                for (int z = zStart; z < zEnd; z++)
                {
                    Console.WriteLine($"Thread {i}: {z}/{zEnd}");
                    for (int y = yStart; y < yEnd; y++)
                    {
                        for (int x = xStart; x < xEnd; x++)
                        {
                            Vector3 testPos = Map.instance.mapOrigin + new Vector3(x*map.distanceMapDetail,
                                                                                   y*map.distanceMapDetail,
                                                                                   z*map.distanceMapDetail);

                            root.children[i].Insert(testPos, distanceMap[x,y,z].distance);
                        }
                    }
                }
            });


            /* for (int z = 0; z < dmLenZ; z++) */
            /* { */
            /*     Console.WriteLine($"{z}/{dmLenZ}"); */
            /*     for (int y = 0; y < dmLenY; y++) */
            /*     { */
            /*         for (int x = 0; x < dmLenX; x++) */
            /*         { */
            /*             Vector3 testPos = Map.instance.mapOrigin + new Vector3(x*map.distanceMapDetail, */
            /*                                                                    y*map.distanceMapDetail, */
            /*                                                                    z*map.distanceMapDetail); */

            /*             root.Insert(testPos, distanceMap[x,y,z].distance); */
            /*         } */
            /*     } */
            /* } */


            Console.WriteLine("DONE");

            /* IFormatter formatter = new BinaryFormatter(); */  

            /* Stream stream = new FileStream($"Maps/Data/TEST-bytes.dmt", FileMode.Create, FileAccess.Write, FileShare.None); */  
            /* formatter.Serialize(stream, root); */  
            /* stream.Close(); */  

            /* Console.WriteLine($"SAVED"); */

            int leafCounter = 0;
            int fullCounter = 1;

            Queue<OCTTreeNode> nodeQueue = new Queue<OCTTreeNode>();
            nodeQueue.Enqueue(root);

            while (nodeQueue.Count > 0)
            {
                var node = nodeQueue.Dequeue();
                if (node.IsLeaf())
                {
                    leafCounter++;
                }
                else
                {
                    foreach (var child in node.children)
                    {
                        nodeQueue.Enqueue(child);
                        fullCounter++;
                    }
                }
            }

            Console.WriteLine($"Number of leaf nodes: {leafCounter}");
            Console.WriteLine($"Number of all nodes: {fullCounter}");
        }

        public static Vector3 ChildPosFromRelative(byte relativePosIndex, Vector3 parentPos, float parentSize)
        {
            parentPos.Deconstruct(out float X, out float Y, out float Z);

            X += (relativePosIndex & 0b100) != 0 ? parentSize/4 : -parentSize/4;
            Y += (relativePosIndex & 0b010) != 0 ? parentSize/4 : -parentSize/4;
            Z += (relativePosIndex & 0b001) != 0 ? parentSize/4 : -parentSize/4;

            return new Vector3(X,Y,Z);
        }
    }
}
