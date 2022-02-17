using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class OCTTreeNode
    {
        OCTTreeNode parent; 
        public OCTTreeNode[] children;

        public Vector3 center;
        float size;
        float minSize;

        bool empty;
        float distanceValue;

        public OCTTreeNode(Vector3 center, float size, float minSize, OCTTreeNode parent=null)
        {
            this.center = center;
            this.size = size;
            this.minSize = minSize;
            this.parent = parent;
            empty = true;
        }

        public bool IsRoot() => parent == null;
        public bool IsLeaf() => children == null;
        public bool IsEmpty() => empty;
        public bool CanSubdivide() => (size > minSize);
        public bool InBoundary(Vector3 position)
        {
            return (center.X - size/2 <= position.X && position.X < center.X + size/2 && 
                    center.Y - size/2 <= position.Y && position.Y < center.Y + size/2 &&   
                    center.Z - size/2 <= position.Z && position.Z < center.Z + size/2);
        }

        public void Subdivide()
        {
            children = new OCTTreeNode[8];

            children[0] = new OCTTreeNode(new Vector3(this.center.X - size/4, this.center.Y - size/4, this.center.Z - size/4), size/2, minSize, this);
            children[1] = new OCTTreeNode(new Vector3(this.center.X + size/4, this.center.Y - size/4, this.center.Z - size/4), size/2, minSize, this);
            children[2] = new OCTTreeNode(new Vector3(this.center.X - size/4, this.center.Y - size/4, this.center.Z + size/4), size/2, minSize, this);
            children[3] = new OCTTreeNode(new Vector3(this.center.X + size/4, this.center.Y - size/4, this.center.Z + size/4), size/2, minSize, this);
            children[4] = new OCTTreeNode(new Vector3(this.center.X - size/4, this.center.Y + size/4, this.center.Z - size/4), size/2, minSize, this);
            children[5] = new OCTTreeNode(new Vector3(this.center.X + size/4, this.center.Y + size/4, this.center.Z - size/4), size/2, minSize, this);
            children[6] = new OCTTreeNode(new Vector3(this.center.X - size/4, this.center.Y + size/4, this.center.Z + size/4), size/2, minSize, this);
            children[7] = new OCTTreeNode(new Vector3(this.center.X + size/4, this.center.Y + size/4, this.center.Z + size/4), size/2, minSize, this);
        }

        public void Insert(Vector3 position, float distance)
        {
            if(!InBoundary(position)) return;

            if(IsLeaf())
            {
                if(CanSubdivide())
                {
                    Subdivide();
                    foreach (var child in children)
                    {
                        child.Insert(position, distance);
                    }
                }
                else
                {
                    this.distanceValue = distance;
                    this.empty = false;

                    // inserted -> call for parent if he can contract his children
                    this.parent.GroupTogether(OCTTree.maxGroupError);
                }
            }
            else
            {
                foreach (var child in children)
                {
                    child.Insert(position, distance);
                }
            }
        }

        public float Search(Vector3 position)
        {
            if(!InBoundary(position)) return float.NaN;

            if(IsLeaf())
            {
                return this.distanceValue;
            }
            else
            {
                foreach (var child in children)
                {
                    float _out = child.Search(position);
                    if (!float.IsNaN(_out))
                    {
                        return _out;
                    }
                }

            }

            /* throw new Exception(""); */
            return float.NaN;
        }

        public bool GroupTogether(float maxSpread, string id="start")
        {
            if(this.IsLeaf() && this.IsEmpty()) return false;

            /* Console.WriteLine($"{id}: {this.IsLeaf()}, {this.IsEmpty()}, {this.distanceValue}"); */

            if(IsLeaf())
            {
                return true;
            }
            else
            {
                bool[] childTest = new bool[8] {children[0].GroupTogether(maxSpread, "1"),
                                                children[1].GroupTogether(maxSpread, "2"),
                                                children[2].GroupTogether(maxSpread, "3"),
                                                children[3].GroupTogether(maxSpread, "4"),
                                                children[4].GroupTogether(maxSpread, "5"),
                                                children[5].GroupTogether(maxSpread, "6"),
                                                children[6].GroupTogether(maxSpread, "7"),
                                                children[7].GroupTogether(maxSpread, "8")};

                if(childTest[0] &&
                   childTest[1] &&
                   childTest[2] &&
                   childTest[3] &&
                   childTest[4] &&
                   childTest[5] &&
                   childTest[6] &&
                   childTest[7])
                {
                    float min = float.MaxValue;
                    float max = float.MinValue;

                    foreach (var child in children)
                    {
                        if(child.distanceValue < min)
                            min = child.distanceValue;

                        if(child.distanceValue > max)
                            max = child.distanceValue;
                    }

                    if(Math.Abs(max - min) <= maxSpread)
                    {
                        this.children = null;
                        this.distanceValue = min;
                        this.empty = false;

                        this.parent.GroupTogether(maxSpread,"hey");

                        return true;
                    }
                }
            }

            return false;
        }

        public int CountAllLeafs()
        {
            if(IsLeaf())
                return 1;
            else
            {
                int count = 0;
                foreach (var child in children)
                {
                    count += CountAllLeafs();
                }
                return count;
            }
        }
    }
}
