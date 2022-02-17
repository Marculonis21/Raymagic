using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    [Serializable]
    public class OCTTreeNode
    {
        OCTTreeNode parent; 
        public OCTTreeNode[] children;

        /// Třeba by šlo si vytvořit bitmasku, kde bych si jen pamatoval svoji
        /// relativní pozici v rodiči, mezi ostatními syny. (Root zná svoji polohu).
        /// Každý node by si pak pamatoval jeden byte kde by 1 na určitém bitu značila, 
        /// že je to takový danný syn svého rodiče. 
        /// Pro hledání skutečné polohy musíme udělat search nahoru po 
        /// parentnech a pak dolů zjišťovat rekurzivně přesnou polohu
        /// středu svého parenta a pak sebe samotného. Nemusel bych vůbec
        /// ukládat střed ani velikost (+minVelikost) node. Každá by držela
        /// pouze 1 byte svého umístění. cca 41 bitů celkem na node.
        ///
        /// Pro detail 1 error 1 je zmenšení 6x. přitom zde by byl nárůst
        /// velikosti na node o necelých 25% - SAVE BY SE MĚL ZMENŠIT...

        byte relativePosIndex; //functional but f slow
        bool empty;
        float distanceValue;

        public OCTTreeNode(byte relativePosIndex=0, OCTTreeNode parent = null)
        {
            this.relativePosIndex = relativePosIndex;
            this.parent = parent;
            this.empty = true;
        }

        public bool IsRoot() => parent == null;
        public bool IsLeaf() => children == null;
        public bool IsEmpty() => empty;
        public bool CanSubdivide()
        {
            GetPosSize(out Vector3 center, out float size);
            return size > OCTTree.nodeMinSize;
        }
        public bool InBoundary(Vector3 testPos)
        {
            GetPosSize(out Vector3 center, out float size);
            return (center.X - size/2 <= testPos.X && testPos.X < center.X + size/2 && 
                    center.Y - size/2 <= testPos.Y && testPos.Y < center.Y + size/2 &&   
                    center.Z - size/2 <= testPos.Z && testPos.Z < center.Z + size/2);
        }

        public void Subdivide()
        {
            children = new OCTTreeNode[8];

            // krychle rozdělená na 8 menších,
            // směr relativní změny pozice vůči rodiči popisují 3 bity XYZ, 
            // kde 0 značí směr záporný a 1 kladný.
            // X změna v x-ose, Y v y-ose, Z v z-ose.
            
            children[0] = new OCTTreeNode(0, this);
            children[1] = new OCTTreeNode(4, this);
            children[2] = new OCTTreeNode(2, this);
            children[3] = new OCTTreeNode(6, this);
            children[4] = new OCTTreeNode(1, this);
            children[5] = new OCTTreeNode(5, this);
            children[6] = new OCTTreeNode(3, this);
            children[7] = new OCTTreeNode(7, this);
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

            return float.NaN;
        }

        public bool GroupTogether(float maxSpread, string id="start")
        {
            if(this.IsLeaf() && this.IsEmpty()) return false;

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
