using System;
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
            this.LEFT = LEFT;
            this.RIGHT = RIGHT;

            this.isLeaf = false;

            this.boundingBoxPosition = new Vector3(Math.Abs((LEFT.boundingBoxPosition.X + RIGHT.boundingBoxPosition.X)/2),
                                                   Math.Abs((LEFT.boundingBoxPosition.Y + RIGHT.boundingBoxPosition.Y)/2),
                                                   Math.Abs((LEFT.boundingBoxPosition.Z + RIGHT.boundingBoxPosition.Z)/2));

            this.boundingBoxSize = Vector3.One*(LEFT.boundingBoxPosition - RIGHT.boundingBoxPosition).Length();
            this.boundingBoxSize +=  new Vector3(LEFT.boundingBoxSize.X/2 + RIGHT.boundingBoxSize.X/2, 
                                                 LEFT.boundingBoxSize.Y/2 + RIGHT.boundingBoxSize.Y/2,
                                                 LEFT.boundingBoxSize.Z/2 + RIGHT.boundingBoxSize.Z/2);

            this.boundingBox = new Box(this.boundingBoxPosition, this.boundingBoxSize, Color.Red);
        }

        public SDFout Test(Vector3 testPos, float minDist, bool physics, out Object obj)
        {
            obj = null;

            // test node bounding box
            SDFout test = this.boundingBox.SDF(testPos, minDist, physics);
            if (test.distance < minDist)
            {
                // if bounding box collided try the actuall node
                if(this.isLeaf) // for leaf nodes
                {
                    SDFout objTest;
                    if (obj is Interactable)
                    {
                        objTest = (obj as Interactable).SDF(testPos, minDist, physics);
                    }
                    else
                    {
                        objTest = this.obj.SDF(testPos, minDist, physics);
                    }

                    if(objTest.distance < minDist)
                    {
                        if (obj is Interactable)
                        {
                            var _obj = obj as Interactable;
                            obj = _obj.modelStates[_obj.state];
                        }
                        else
                        {
                            obj = this.obj;
                        }
                        return objTest;
                    }
                }
                else // for parent node
                {
                    SDFout lTest = LEFT.Test(testPos, minDist, physics, out Object lObj);
                    SDFout rTest = RIGHT.Test(testPos, minDist, physics, out Object rObj);

                    // both of them will improve minDist - choose the best one
                    if(lTest.distance < minDist && rTest.distance < minDist)
                    {
                        if(lTest.distance < rTest.distance)
                        {
                            obj = lObj;
                            return lTest;
                        }
                        else
                        {
                            obj = rObj;
                            return rTest;
                        }
                    }
                    // only left will improve minDist
                    else if(lTest.distance < minDist && rTest.distance >= minDist)
                    {
                        obj = lObj;
                        return lTest;
                    }
                    // only right will improve
                    else if(rTest.distance < minDist && lTest.distance >= minDist)
                    {
                        obj = rObj;
                        return rTest;
                    }
                    // non of them will improve - return (mindist + 1 OR something big) because we didnt
                    // find any wanted sdf collision
                }
            }

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
