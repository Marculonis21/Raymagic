using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class DMOCTTree
    {
        public Vector3 center;
        public Vector3 size;

        public DMOCTTree parent;

        public DMOCTTree[] children = new DMOCTTree[8];
        public bool isSubdivided = false;

        public List<Tuple<Vector3, float>> itemList;
        public float listMin;
        public float listMax;

        public DMOCTTree(DMOCTTree parent, Vector3 center, Vector3 size)
        {
            this.parent = parent;
            this.center = center;
            this.size = size;

            this.itemList = new List<Tuple<Vector3, float>>();
            listMin = float.MinValue;
            listMax = float.MaxValue;
        }

        public DMOCTTree(Vector3 center, Vector3 size) : this(null,center,size)
        {
        }

        void Subdivide()
        {
            this.isSubdivided = true;

            children[0] = new DMOCTTree(this, new Vector3(this.center.X - size.X/4, this.center.Y - size.Y/4, this.center.Z - size.Z/4), size/2);
            children[1] = new DMOCTTree(this, new Vector3(this.center.X + size.X/4, this.center.Y - size.Y/4, this.center.Z - size.Z/4), size/2);
            children[2] = new DMOCTTree(this, new Vector3(this.center.X - size.X/4, this.center.Y - size.Y/4, this.center.Z + size.Z/4), size/2);
            children[3] = new DMOCTTree(this, new Vector3(this.center.X + size.X/4, this.center.Y - size.Y/4, this.center.Z + size.Z/4), size/2);
            children[4] = new DMOCTTree(this, new Vector3(this.center.X - size.X/4, this.center.Y + size.Y/4, this.center.Z - size.Z/4), size/2);
            children[5] = new DMOCTTree(this, new Vector3(this.center.X + size.X/4, this.center.Y + size.Y/4, this.center.Z - size.Z/4), size/2);
            children[6] = new DMOCTTree(this, new Vector3(this.center.X - size.X/4, this.center.Y + size.Y/4, this.center.Z + size.Z/4), size/2);
            children[7] = new DMOCTTree(this, new Vector3(this.center.X + size.X/4, this.center.Y + size.Y/4, this.center.Z + size.Z/4), size/2);
        }

        public void Insert(float distValue, Vector3 position, float possibleDistanceError = 10)
        {
            if(!InBoundary(position)) return;

            if(!isSubdivided)
            {
                if (this.itemList.Count == 0)
                {
                    this.itemList.Add(new Tuple<Vector3, float>(position, distValue));
                    this.listMin = distValue;
                    this.listMax = distValue;
                }
                else if((this.listMin <= distValue && distValue <= this.listMax) || // 1) mezi min max
                        (this.listMin == distValue || distValue == this.listMax))   // 2) == min/max
                {
                    this.itemList.Add(new Tuple<Vector3, float>(position, distValue));
                }
                else if(distValue < this.listMin && Math.Abs(distValue - this.listMax) <= possibleDistanceError) // 3) menší než min ale v distanceErroru od max
                {
                    this.itemList.Add(new Tuple<Vector3, float>(position, distValue));
                    this.listMin = distValue;
                }
                else if(this.listMax < distValue && Math.Abs(distValue - this.listMin) <= possibleDistanceError) // 4) větší než max ale v distanceErroru od min
                {
                    this.itemList.Add(new Tuple<Vector3, float>(position, distValue));
                    this.listMax = distValue;
                }
                else
                {
                    if(!isSubdivided && size.X >= Map.instance.distanceMapDetail && 
                                        size.Y >= Map.instance.distanceMapDetail && 
                                        size.Z >= Map.instance.distanceMapDetail)
                    {
                        Subdivide();

                        // re-add from parent
                        foreach (var child in children)
                        {
                            foreach (var item in itemList)
                            {
                                child.Insert(item.Item2, item.Item1, possibleDistanceError);
                            }
                        }       

                        this.itemList = null;

                        // add new 
                        foreach (var child in children)
                        {
                            child.Insert(distValue, position, possibleDistanceError);
                        }
                    }
                }
            }
            else
            {
                foreach (var child in children)
                {
                    child.Insert(distValue, position, possibleDistanceError);
                }
            }
        }

        bool InBoundary(Vector3 position)
        {
            return (center.X - size.X/2 <= position.X && position.X < center.X + size.X/2 && 
                    center.Y - size.Y/2 <= position.Y && position.Y < center.Y + size.Y/2 &&   
                    center.Z - size.Z/2 <= position.Z && position.Z < center.Z + size.Z/2);
        }

        public DMOCTTree Search(Vector3 position)
        {
            if(!InBoundary(position)) return null;

            if(isSubdivided)
            {
                foreach (var child in children)
                {
                    var _out = child.Search(position);

                    if(_out != null)
                        return _out;
                }
            }

            return this;
        }

        public int CountAllNodes()
        {
            if(!isSubdivided)
                return 1;
            else
            {
                int count = 0;
                foreach (var item in children)
                {
                    count += item.CountAllNodes();
                }

                return count;
            }
        }
    }
}
