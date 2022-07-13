using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class BVH
    {
        BVHNode root;

        public BVH()
        {
        }

        public void InfoPrint()
        {
            if(root != null)
                root.Print(0);
        }

        public void BuildBVHDownUp()
        {
            List<Object> dList = new List<Object>(Map.instance.dynamicObjectList);
            dList.AddRange(Map.instance.interactableObjectList);

            if(dList.Count == 0) return;

            List<BVHNode> tmpNodes = new List<BVHNode>();

            Console.WriteLine("Building BVH");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while(dList.Count != 0)
            {
                Object obj1 = dList[0];
                dList.Remove(obj1);

                if(dList.Count == 0) // if only 1 object -> single leaf
                {
                    tmpNodes.Add(new BVHNode(obj1));
                }
                else if(dList.Count == 1) // only 2 objects -> root -> (L,R):(n1,n2)
                {
                    Object obj2 = dList[0];
                    dList.Remove(obj2);

                    BVHNode n1 = new BVHNode(obj1);
                    BVHNode n2 = new BVHNode(obj2);

                    tmpNodes.Add(new BVHNode(n1,n2));
                }
                else // more objects -> choose closest to obj1 -> root -> (L,R):(n1,n2)
                {
                    Object obj2 = null;
                    float distanceRecord = float.MaxValue;
                    foreach (var obj in dList)
                    {
                        if((obj1.Position - obj.Position).Length() < distanceRecord)
                        {
                            obj2 = obj;
                            distanceRecord = (obj1.Position - obj.Position).Length();
                        }
                    }

                    dList.Remove(obj2);

                    BVHNode n1 = new BVHNode(obj1);
                    BVHNode n2 = new BVHNode(obj2);

                    tmpNodes.Add(new BVHNode(n1,n2));
                }
            }

            // until there is only one node
            while(tmpNodes.Count > 1)
            {
                BVHNode bestN1 = null;
                BVHNode bestN2 = null;
                float distanceRecord = float.MaxValue;
                float _distance;

                foreach (var n1 in tmpNodes) // find two closest nodes
                {
                    foreach (var n2 in tmpNodes)
                    {
                        if(n1 == n2) continue;

                        _distance = (n1.boundingBoxPosition - n2.boundingBoxPosition).Length();
                        if(_distance < distanceRecord)
                        {
                            bestN1 = n1;
                            bestN2 = n2;
                            distanceRecord = _distance;
                        }
                    }
                }

                tmpNodes.Remove(bestN1);
                tmpNodes.Remove(bestN2);

                tmpNodes.Add(new BVHNode(bestN1,bestN2)); // create new node with those two as child nodes
            }

            this.root = tmpNodes[0]; // root is the last node

            sw.Stop();
            Console.WriteLine($"BVH done - {sw.ElapsedMilliseconds}ms");
        }

        public SDFout Test(Vector3 testPos, float minDist, bool physics, out Object obj)
        {
            if(root != null)
                return root.Test(testPos, minDist, physics, out obj);
            else
            {
                obj = null;
                return new SDFout(float.MaxValue, Color.Pink);
            }
        }
    }
}
