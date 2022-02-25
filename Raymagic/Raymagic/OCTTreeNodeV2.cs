using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    [Serializable]
    public class OCTTreeNodeV2
    {
        OCTTreeNodeV2 parent; 
        public OCTTreeNodeV2[] children;

        byte relativePosIndex;
        float distanceValue;

        public OCTTreeNodeV2(byte relativePosIndex=0, OCTTreeNodeV2 parent = null)
        {
            this.relativePosIndex = relativePosIndex;
            this.parent = parent;
        }

        public OCTTreeNodeV2(float value, byte relativePosIndex=0, OCTTreeNodeV2 parent = null)
        {
            this.distanceValue = value;
            this.relativePosIndex = relativePosIndex;
            this.parent = parent;
        }

        public int Build(Vector3 pos, float size, int workerID=0)
        {
            int count = 0;
            if(CanSubdivide(size))
            {
                if(ShouldSubdivide(pos, size, out float[] childValues, out Vector3[] childPos))
                {
                    Subdivide(childValues);

                    for (int i = 0; i < children.Length; i++)
                    {
                        count += children[i].Build(childPos[i], size/2, workerID);
                    }
                }
                else
                {
                    count += 1;
                }
            }
            else
            {
                count += 1;
            }

            return count;
        }

        public bool IsRoot() => parent == null;
        public bool IsLeaf() => children == null;
        public bool InBoundary(Vector3 testPos, Vector3 center, float size)
        {
            return (center.X - size/2 <= testPos.X && testPos.X < center.X + size/2 && 
                    center.Y - size/2 <= testPos.Y && testPos.Y < center.Y + size/2 &&   
                    center.Z - size/2 <= testPos.Z && testPos.Z < center.Z + size/2);
        }

        public bool CanSubdivide(float size)
        {
            return size > OCTTree.nodeMinSize;
        }
        
        public bool ShouldSubdivide(Vector3 thisPos, float thisSize, out float[] childValues, out Vector3[] childPos)
        {
            childValues = new float[8];
            float sumValues = 0;

            childPos = GetChildrenPos(thisPos, thisSize);

            float max = float.MinValue;
            float min = float.MaxValue;
            for (int i = 0; i < 8; i++)
            {
                SDFout test;
                SDFout best = new SDFout(float.MaxValue, Color.Pink);
                foreach(Object obj in Map.instance.staticObjectList)
                {
                    test = obj.SDF(childPos[i], best.distance);
                    if(test.distance < best.distance)
                        best = test;
                }

                childValues[i] = best.distance;
                if(childValues[i] > max)
                    max = childValues[i];
                if(childValues[i] < min)
                    min = childValues[i];

                sumValues += best.distance;
            }

            // test for children mean errors 
            float avg = sumValues/8;
            float _sum = 0;

            for (int i = 0; i < 8; i++)
            {
                _sum += (childValues[i] - avg)*(childValues[i] - avg);
            }
            
            float MSE = (1f/8f) * _sum;
            return MSE > OCTTree.maxGroupError;

            float upperValue = 0;
            if(thisSize/4f >= OCTTree.maxGroupError)
                upperValue = thisSize/4f;
            else
                upperValue = OCTTree.maxGroupError;

            /* return Math.Abs(max-min) >= OCTTree.maxGroupError; */
        }

        public void Subdivide(float[] defaultValue)
        {
            children = new OCTTreeNodeV2[8];
            
            children[0] = new OCTTreeNodeV2(defaultValue[0], 0, this);
            children[1] = new OCTTreeNodeV2(defaultValue[1], 4, this);
            children[2] = new OCTTreeNodeV2(defaultValue[2], 2, this);
            children[3] = new OCTTreeNodeV2(defaultValue[3], 6, this);
            children[4] = new OCTTreeNodeV2(defaultValue[4], 1, this);
            children[5] = new OCTTreeNodeV2(defaultValue[5], 5, this);
            children[6] = new OCTTreeNodeV2(defaultValue[6], 3, this);
            children[7] = new OCTTreeNodeV2(defaultValue[7], 7, this);
        }

        public Vector3[] GetChildrenPos(Vector3 thisPos, float thisSize)
        {
            Vector3[] childPos = new Vector3[8];

            childPos[0] = OCTTree.ChildPosFromRelative(0, thisPos, thisSize);
            childPos[1] = OCTTree.ChildPosFromRelative(4, thisPos, thisSize);
            childPos[2] = OCTTree.ChildPosFromRelative(2, thisPos, thisSize);
            childPos[3] = OCTTree.ChildPosFromRelative(6, thisPos, thisSize);
            childPos[4] = OCTTree.ChildPosFromRelative(1, thisPos, thisSize);
            childPos[5] = OCTTree.ChildPosFromRelative(5, thisPos, thisSize);
            childPos[6] = OCTTree.ChildPosFromRelative(3, thisPos, thisSize);
            childPos[7] = OCTTree.ChildPosFromRelative(7, thisPos, thisSize);

            return childPos;
        }

        public void GetPosSize(out Vector3 position, out float size)
        {
            if(this.parent != null)
            {
                this.parent.GetPosSize(out Vector3 parentPosition, out float parentSize);

                position = OCTTree.ChildPosFromRelative(relativePosIndex, parentPosition, parentSize);
                size = parentSize/2;
            }
            else // root node
            {
                position = OCTTree.rootPosition;
                size = OCTTree.rootSize;
            }
        }

        public float Search(Vector3 position)
        {
            GetPosSize(out Vector3 center, out float size);

            if(!InBoundary(position, center, size)) return float.NaN;

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

            return float.NaN;
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
