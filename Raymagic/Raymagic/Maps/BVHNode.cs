using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class BVHNode
    {
        BVHNode LEFT = null;
        BVHNode RIGHT = null;

        public Box boundingBox {get; private set;}
        Vector3 boundingBoxSize;
        public Vector3 boundingBoxPosition {get; private set;} // needed for BVH building

        Object obj;
        bool isLeaf = false;

        public BVHNode(Object obj)
        {
            this.isLeaf = true;

            this.obj = obj;

            this.boundingBox = obj.BoundingBox;
            this.boundingBoxSize = obj.BoundingBoxSize; 
            this.boundingBoxPosition = obj.BoundingBox.Position; 

        }

        public BVHNode(BVHNode LEFT, BVHNode RIGHT)
        {
            if (Vector3.Distance(LEFT.boundingBoxPosition, new Vector3(0,0,0)) < Vector3.Distance(RIGHT.boundingBoxPosition, new Vector3(0,0,0)))
            {
                this.LEFT = LEFT;
                this.RIGHT = RIGHT;
            }
            else
            {

                this.LEFT = RIGHT;
                this.RIGHT = LEFT;
            }

            this.isLeaf = false;

            Vector3[] oc = new Vector3[] {
                this.LEFT.boundingBoxPosition  - this.LEFT.boundingBoxSize/2,
                this.LEFT.boundingBoxPosition  + this.LEFT.boundingBoxSize/2,            
                this.RIGHT.boundingBoxPosition + this.RIGHT.boundingBoxSize/2,
                this.RIGHT.boundingBoxPosition - this.RIGHT.boundingBoxSize/2,          
            };

            /* Map.instance.infoObjectList.Add(new Sphere(oc[0], 10, Color.Green)); */
            /* Map.instance.infoObjectList.Add(new Sphere(oc[1], 10, Color.Green)); */
            /* Map.instance.infoObjectList.Add(new Sphere(oc[2], 10, Color.Green)); */
            /* Map.instance.infoObjectList.Add(new Sphere(oc[3], 10, Color.Green)); */

            var smallestX = (new float[] {oc[0].X,oc[1].X,oc[2].X,oc[3].X}).Min();
            var smallestY = (new float[] {oc[0].Y,oc[1].Y,oc[2].Y,oc[3].Y}).Min();
            var smallestZ = (new float[] {oc[0].Z,oc[1].Z,oc[2].Z,oc[3].Z}).Min();
            var biggestX = (new float[] {oc[0].X,oc[1].X,oc[2].X,oc[3].X}).Max();
            var biggestY = (new float[] {oc[0].Y,oc[1].Y,oc[2].Y,oc[3].Y}).Max();
            var biggestZ = (new float[] {oc[0].Z,oc[1].Z,oc[2].Z,oc[3].Z}).Max();

            this.boundingBoxSize = new Vector3(Math.Abs(smallestX-biggestX),
                                               Math.Abs(smallestY-biggestY),
                                               Math.Abs(smallestZ-biggestZ));

            this.boundingBoxPosition = new Vector3(smallestX + this.boundingBoxSize.X/2,
                                                   smallestY + this.boundingBoxSize.Y/2,
                                                   smallestZ + this.boundingBoxSize.Z/2);

            this.boundingBox = new Box(this.boundingBoxPosition, this.boundingBoxSize, Color.Red);

            /* Map.instance.infoObjectList.Add(this.boundingBox); */
        }

        public SDFout Test(Vector3 testPos, float minDist, out Object obj, out bool IsTransparent)
        {
            obj = null;

            // test node bounding box
            SDFout test = this.boundingBox.SDF(testPos, minDist, out _);
            if (test.distance < minDist)
            {
                // if bounding box collided try the actuall node
                if(this.isLeaf) // for leaf nodes
                {
                    SDFout objTest;
                    bool objTransparent;
                    if (obj is Interactable)
                    {
                        objTest = (obj as Interactable).SDF(testPos, minDist, out objTransparent);
                    }
                    else
                    {
                        objTest = this.obj.SDF(testPos, minDist, out objTransparent);
                    }

                    if(objTest.distance < minDist)
                    {
                        if (obj is Interactable)
                        {
                            IsTransparent = objTransparent;
                            var _obj = obj as Interactable;
                            obj = _obj.modelStates[_obj.state];
                        }
                        else
                        {
                            IsTransparent = objTransparent;
                            obj = this.obj;
                        }
                        return objTest;
                    }
                }
                else // for parent node
                {
                    SDFout lTest = LEFT.Test(testPos, minDist, out Object lObj, out bool lTransparent);
                    SDFout rTest = RIGHT.Test(testPos, minDist, out Object rObj, out bool rTransparent);

                    // both of them will improve minDist - choose the best one
                    if(lTest.distance < minDist && rTest.distance < minDist)
                    {
                        if(lTest.distance < rTest.distance)
                        {
                            obj = lObj;
                            IsTransparent = lTransparent;
                            return lTest;
                        }
                        else
                        {
                            obj = rObj;
                            IsTransparent = rTransparent;
                            return rTest;
                        }
                    }
                    // only left will improve minDist
                    else if(lTest.distance < minDist && rTest.distance >= minDist)
                    {
                        obj = lObj;
                        IsTransparent = lTransparent;
                        return lTest;
                    }
                    // only right will improve
                    else if(rTest.distance < minDist && lTest.distance >= minDist)
                    {
                        obj = rObj;
                        IsTransparent = rTransparent;
                        return rTest;
                    }
                    // non of them will improve - return (mindist + 1 OR something big) because we didnt
                    // find any wanted sdf collision
                }
            }

            IsTransparent = false;
            return new SDFout(float.MaxValue, Color.Pink);
        }
        
        public void Print(int depth)
        {
            if(isLeaf)
            {
                for (int i = 0; i < depth; i++)
                    Console.Write(" ");

                Console.WriteLine($"pos: {this.boundingBoxPosition}, size: {this.boundingBoxSize}");
            }
            else
            {
                for (int i = 0; i < depth; i++)
                    Console.Write(" ");

                Console.WriteLine($"pos: {this.boundingBoxPosition}, size: {this.boundingBoxSize}");

                LEFT.Print(depth+2);
                RIGHT.Print(depth+2);
            }
        }
    }
}
