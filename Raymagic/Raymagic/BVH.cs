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
            if(dList.Count == 0) return;

            List<BVHNode> tmpNodes = new List<BVHNode>();

            Console.WriteLine("Building BVH");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while(dList.Count != 0)
            {
                Object obj1 = dList[0];
                dList.Remove(obj1);

                if(dList.Count == 0)
                {
                    tmpNodes.Add(new BVHNode(obj1));
                }
                else if(dList.Count == 1)
                {
                    Object obj2 = dList[0];
                    dList.Remove(obj2);

                    BVHNode n1 = new BVHNode(obj1);
                    BVHNode n2 = new BVHNode(obj2);

                    tmpNodes.Add(new BVHNode(n1,n2));
                }
                else
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

            // last node is root left
            while(tmpNodes.Count > 1)
            {
                BVHNode bestN1 = null;
                BVHNode bestN2 = null;
                float distanceRecord = float.MaxValue;
                float _distance;

                foreach (var n1 in tmpNodes)
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

                tmpNodes.Add(new BVHNode(bestN1,bestN2));
            }

            this.root = tmpNodes[0];

            sw.Stop();
            Console.WriteLine($"BVH done - {sw.ElapsedMilliseconds}ms");
        }

        public SDFout Test(Vector3 testPos, float minDist, out Object obj)
        {
            if(root != null)
                return root.Test(testPos, minDist, out obj);
            else
            {
                obj = null;
                return new SDFout(float.MaxValue, Color.Pink);
            }
        }
    }
}
