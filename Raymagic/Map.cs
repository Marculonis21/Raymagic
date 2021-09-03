using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class Map
    {
        //SINGLETON
        MapData data;
        public string[,] map;

        MapLayout layout = MapLayout.instance;

        int blockSize = 100;

        private Map()
        {}

        public static readonly Map instance = new Map();

        public void SetMap(string id)
        {
            data = layout.maps[id];
            map = data.map;
        }

        public Vector3 GetPlayerStart()
        {
            Vector3 spawn = data.playerSpawn;
            return new Vector3(spawn.X*100 + 50, spawn.Y*100 + 50, spawn.Z*100 + 50);
        }

        public bool CheckPointBox(Vector3 testPos, out Color color)
        {
            color = Color.Blue;

            int x = (int)Math.Floor(testPos.X / this.blockSize);
            int y = (int)Math.Floor(testPos.Y / this.blockSize);
            int z = (int)Math.Floor(testPos.Z / this.blockSize);
            
            char col = map[z,x].ToCharArray()[y];
            if(col == ' ')
                return false;
            else
            {
                switch(col)
                {
                    case 'W':
                        color = Color.White;
                        return true;
                    case 'R':
                        color = Color.DarkRed;
                        return true;
                    case 'G':
                        color = Color.Green;
                        return true;
                }
                return true;
            }
        }

        bool GetIntersection(float fDst1, float fDst2, Vector3 P1, Vector3 P2, out Vector3 Hit) 
        {
            Hit = new Vector3();
            if ( (fDst1 * fDst2) >= 0.0f) return false;
            if ( fDst1 == fDst2) return false; 

            Hit = P1 + (P2-P1) * ( -fDst1/(fDst2-fDst1) );
            return true;
        }

        bool InBox(Vector3 Hit, Vector3 B1, Vector3 B2, int Axis) 
        {
            if ( Axis==1 && Hit.Z > B1.Z && Hit.Z < B2.Z && Hit.Y > B1.Y && Hit.Y < B2.Y) return true;
            if ( Axis==2 && Hit.Z > B1.Z && Hit.Z < B2.Z && Hit.X > B1.X && Hit.X < B2.X) return true;
            if ( Axis==3 && Hit.X > B1.X && Hit.X < B2.X && Hit.Y > B1.Y && Hit.Y < B2.Y) return true;
            return false;
        }

        // returns true if line (L1, L2) intersects with the box (B1, B2)
        // returns intersection point in Hit
        bool CheckLineBox(Vector3 B1, Vector3 B2, Vector3 L1, Vector3 L2, out Vector3 Hit)
        {
            Hit = new Vector3();

            if (L2.X < B1.X && L1.X < B1.X) return false;
            if (L2.X > B2.X && L1.X > B2.X) return false;
            if (L2.Y < B1.Y && L1.Y < B1.Y) return false;
            if (L2.Y > B2.Y && L1.Y > B2.Y) return false;
            if (L2.Z < B1.Z && L1.Z < B1.Z) return false;
            if (L2.Z > B2.Z && L1.Z > B2.Z) return false;
            if (L1.X > B1.X && L1.X < B2.X &&
                L1.Y > B1.Y && L1.Y < B2.Y &&
                L1.Z > B1.Z && L1.Z < B2.Z) { Hit = L1; return true;  }

            if ((GetIntersection(L1.X-B1.X, L2.X-B1.X, L1, L2, out Hit) && InBox( Hit, B1, B2, 1 ))
             || (GetIntersection(L1.Y-B1.Y, L2.Y-B1.Y, L1, L2, out Hit) && InBox( Hit, B1, B2, 2 )) 
             || (GetIntersection(L1.Z-B1.Z, L2.Z-B1.Z, L1, L2, out Hit) && InBox( Hit, B1, B2, 3 )) 
             || (GetIntersection(L1.X-B2.X, L2.X-B2.X, L1, L2, out Hit) && InBox( Hit, B1, B2, 1 )) 
             || (GetIntersection(L1.Y-B2.Y, L2.Y-B2.Y, L1, L2, out Hit) && InBox( Hit, B1, B2, 2 )) 
             || (GetIntersection(L1.Z-B2.Z, L2.Z-B2.Z, L1, L2, out Hit) && InBox( Hit, B1, B2, 3 )))
            {
                return true;
            }
            return false;
        }

        public bool CheckLine(Vector3 L1, Vector3 L2, out Vector3 Hit)
        {
            float x1 = (int)Math.Floor(L1.X / this.blockSize) * blockSize;
            float y1 = (int)Math.Floor(L1.Y / this.blockSize) * blockSize;
            float z1 = (int)Math.Floor(L1.Z / this.blockSize) * blockSize;

            float x2 = (int)Math.Floor(L2.X / this.blockSize) * blockSize;
            float y2 = (int)Math.Floor(L2.Y / this.blockSize) * blockSize;
            float z2 = (int)Math.Floor(L2.Z / this.blockSize) * blockSize;

            if(CheckLineBox(new Vector3(x1-blockSize,y1-blockSize,z1-blockSize), new Vector3(x1,y1,z1), L1, L2, out Hit)) return true;
            if(CheckLineBox(new Vector3(x2-blockSize,y2-blockSize,z2-blockSize), new Vector3(x2,y2,z2), L1, L2, out Hit)) return true;

            if(CheckLineBox(new Vector3(x1-blockSize-blockSize,y1-blockSize,z1-blockSize), new Vector3(x1-blockSize,y1,z1), L1, L2, out Hit)) return true;
            if(CheckLineBox(new Vector3(x1-blockSize,y1-blockSize-blockSize,z1-blockSize), new Vector3(x1,y1-blockSize,z1), L1, L2, out Hit)) return true;
            if(CheckLineBox(new Vector3(x1-blockSize,y1-blockSize,z1-blockSize-blockSize), new Vector3(x1,y1,z1-blockSize), L1, L2, out Hit)) return true;

            if(CheckLineBox(new Vector3(x2-blockSize-blockSize,y2-blockSize,z2-blockSize), new Vector3(x2-blockSize,y2,z2), L1, L2, out Hit)) return true;
            if(CheckLineBox(new Vector3(x2-blockSize,y2-blockSize-blockSize,z2-blockSize), new Vector3(x2,y2-blockSize,z2), L1, L2, out Hit)) return true;
            if(CheckLineBox(new Vector3(x2-blockSize,y2-blockSize,z2-blockSize-blockSize), new Vector3(x2,y2,z2-blockSize), L1, L2, out Hit)) return true;
            return false;
        }
    }
}
