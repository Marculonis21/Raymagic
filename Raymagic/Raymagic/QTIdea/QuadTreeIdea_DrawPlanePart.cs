using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class DrawPlanePart
    {
        public Point centerPartPos; //center in window
        public int sizeWH; //square

        public Color color = new Color(-1,-1,-1);
        
        public DrawPlanePart parent;
        public bool isLeaf = true;
        public DrawPlanePart[] subs; 

        public bool safe = false;

        public float dst;
        public int id;

        public DrawPlanePart(Point centerPartPos, int sizeWH, DrawPlanePart parent=null)
        {
            this.id = MainGame.random.Next(0, 99999);
            this.centerPartPos = centerPartPos;
            this.sizeWH = sizeWH;
            this.parent = parent;
        }

        public DrawPlanePart[] Subdivide()
        {
            DrawPlanePart[] _subs = new DrawPlanePart[] { // OMG HOPE THIS WORKS (and CPU, do not ruin it for me!!!)
                new DrawPlanePart(new Point(this.centerPartPos.X - sizeWH/4, this.centerPartPos.Y - sizeWH/4), (int)Math.Ceiling((double)sizeWH/2), this), //NE
                new DrawPlanePart(new Point(this.centerPartPos.X + sizeWH/4, this.centerPartPos.Y - sizeWH/4), (int)Math.Ceiling((double)sizeWH/2), this), //NW
                new DrawPlanePart(new Point(this.centerPartPos.X - sizeWH/4, this.centerPartPos.Y + sizeWH/4), (int)Math.Ceiling((double)sizeWH/2), this), //SE
                new DrawPlanePart(new Point(this.centerPartPos.X + sizeWH/4, this.centerPartPos.Y + sizeWH/4), (int)Math.Ceiling((double)sizeWH/2), this)  //SW      
            };
            this.subs = _subs;
            this.isLeaf = false;
            return this.subs;
        }

        public List<DrawPlanePart> GetNeighbors(string direction)
        {
            DrawPlanePart bigNeighbor = getBiggerOrEqualNeighbors(direction);
            List<DrawPlanePart> allNeighbors = getSmallerNeighbors(bigNeighbor, direction);

            return allNeighbors;
        }

        //https://geidav.wordpress.com/2017/12/02/advanced-octrees-4-finding-neighbor-nodes/
        // 2 step process
        DrawPlanePart getBiggerOrEqualNeighbors(string direction) // w = up, s = down, a = left, d = right
        {
            if(direction == "w")
            {
                if(this.parent == null)
                    return null; // we are in root

                if(this.parent.subs[2] == this) // are we SE?
                    return this.parent.subs[0]; // return NE

                if(this.parent.subs[3] == this) // are we SW?
                    return this.parent.subs[1]; // return NW

                DrawPlanePart dpp = this.parent.getBiggerOrEqualNeighbors(direction);
                if(dpp == null || dpp.isLeaf)
                    return dpp;

                // we are north child
                if(this.parent.subs[0] == this)
                    return dpp.subs[2];
                else
                    return dpp.subs[3];
            }
            if(direction == "s")
            {
                if(this.parent == null)
                    return null; // we are in root

                if(this.parent.subs[0] == this) // are we NE?
                    return this.parent.subs[2]; // return SE

                if(this.parent.subs[1] == this) // are we NW?
                    return this.parent.subs[3]; // return SW

                DrawPlanePart dpp = this.parent.getBiggerOrEqualNeighbors(direction);
                if(dpp == null || dpp.isLeaf)
                    return dpp;

                // we are south child!
                if(this.parent.subs[2] == this)
                    return dpp.subs[0];
                else
                    return dpp.subs[1];
            }
            if(direction == "a")
            {
                if(this.parent == null)
                    return null; // we are in root

                if(this.parent.subs[1] == this) // are we NW?
                    return this.parent.subs[0]; // return NE

                if(this.parent.subs[3] == this) // are we SW?
                    return this.parent.subs[2]; // return SE

                DrawPlanePart dpp = this.parent.getBiggerOrEqualNeighbors(direction);
                if(dpp == null || dpp.isLeaf)
                    return dpp;

                // we are EAST child
                if(this.parent.subs[0] == this)
                    return dpp.subs[1];
                else
                    return dpp.subs[3];
            }
            if(direction == "d")
            {
                if(this.parent == null)
                    return null; // we are in root

                if(this.parent.subs[0] == this) // are we NE?
                    return this.parent.subs[1]; // return NW

                if(this.parent.subs[2] == this) // are we SE?
                    return this.parent.subs[3]; // return SW

                DrawPlanePart dpp = this.parent.getBiggerOrEqualNeighbors(direction);
                if(dpp == null || dpp.isLeaf)
                    return dpp;

                // we are EAST child
                if(this.parent.subs[1] == this)
                    return dpp.subs[0];
                else
                    return dpp.subs[2];
            }
            return null;
        }

        //step 2
        List<DrawPlanePart> getSmallerNeighbors(DrawPlanePart neighbor, string direction)
        {
            Queue<DrawPlanePart> candidates = new Queue<DrawPlanePart>();
            if(neighbor != null)
                candidates.Enqueue(neighbor);

            List<DrawPlanePart> neighbors = new List<DrawPlanePart>();


            while(candidates.Count > 0)
            {
                DrawPlanePart dpp = candidates.Dequeue();

                if(dpp.isLeaf)
                    neighbors.Add(dpp); //is leaf so cannot go further
                else
                {
                    if(direction == "w")
                    {
                        candidates.Enqueue(dpp.subs[2]); //look for children more south
                        candidates.Enqueue(dpp.subs[3]);
                    }
                    if(direction == "s")
                    {
                        candidates.Enqueue(dpp.subs[0]); //look for children more north
                        candidates.Enqueue(dpp.subs[1]);
                    }
                    if(direction == "a")
                    {
                        candidates.Enqueue(dpp.subs[0]); //look for children more east
                        candidates.Enqueue(dpp.subs[2]);
                    }
                    if(direction == "d")
                    {
                        candidates.Enqueue(dpp.subs[1]); //look for children more west
                        candidates.Enqueue(dpp.subs[3]);
                    }
                }
            }

            return neighbors;
        }
    }
}
