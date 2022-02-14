using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class OCTTree
    {
        public OCTTreeNode root;
        public Queue<OCTTreeNode> nodeQueue;

        public OCTTree(SDFout[,,] distanceMap) // I would say that it works pretty fine!!!!
        {
            Map map = Map.instance;
            Vector3 center = (map.mapTopCorner - map.mapOrigin)/2;
            Vector3 size = map.mapTopCorner - map.mapOrigin;

            Console.WriteLine("Creating octtree");
            this.root = new OCTTreeNode(center,size, Vector3.One * map.distanceMapDetail);

            int dmLenX = distanceMap.GetLength(0);
            int dmLenY = distanceMap.GetLength(1);
            int dmLenZ = distanceMap.GetLength(2);
            for (int z = 0; z < dmLenZ; z++)
            {
                for (int y = 0; y < dmLenY; y++)
                {
                    for (int x = 0; x < dmLenX; x++)
                    {
                        Vector3 testPos = Map.instance.mapOrigin + new Vector3(x*map.distanceMapDetail,
                                                                               y*map.distanceMapDetail,
                                                                               z*map.distanceMapDetail);

                        root.Insert(testPos, distanceMap[x,y,z].distance);
                    }
                }
            }

            Console.WriteLine("Processing octtree");

            root.GroupTogether(2);

            Console.WriteLine("DONE");
            int leafCounter = 0;
            int fullCounter = 1;

            nodeQueue = new Queue<OCTTreeNode>();
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
    }
}
