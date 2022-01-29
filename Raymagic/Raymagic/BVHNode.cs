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

        public float Test(Vector3 testPos, float minDist, out Object obj)
        {
            obj = null;

            float test;
            if(this.isLeaf)
            {
                test = this.boundingBox.SDF(testPos, minDist, false, false);
                // test the object bounding box
                if(test < minDist)
                {
                    // if bounding box collided try the actuall object SDF
                    float objTest = this.obj.SDF(testPos, minDist, false, false);

                    if(objTest < minDist)
                    {
                        obj = this.obj;
                        return objTest;
                    }
                }
            }
            else
            {
                test = this.boundingBox.SDF(testPos, minDist, false, false);
                if(minDist > test)
                {
                    float lTest = LEFT.Test(testPos, minDist, out Object lObj);
                    float rTest = RIGHT.Test(testPos, minDist, out Object rObj);

                    // both of them will improve MIN - choose the best one
                    if(lTest < minDist && rTest < minDist)
                    {
                        if(lTest < rTest)
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
                    // only left will improve MIN
                    else if(lTest < minDist && rTest >= minDist)
                    {
                        obj = lObj;
                        return lTest;
                    }
                    // only right
                    else if(rTest < minDist && lTest >= minDist)
                    {
                        obj = rObj;
                        return rTest;
                    }
                    // non of them will - return (mindist + 1 OR something big) because we didnt
                    // find any wanted sdf collision
                }
            }

            return float.MaxValue;
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
