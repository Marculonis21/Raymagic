using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class DMCompressionOCTTree
    {
        public DMOCTTree root;
        public DMCompressionOCTTree(SDFout[,,] distanceMap)
        {
            Console.WriteLine("OCTTree magic");
            Vector3 center = (Map.instance.mapTopCorner - Map.instance.mapOrigin)/2;
            Vector3 size = Map.instance.mapTopCorner - Map.instance.mapOrigin;

            this.root = new DMOCTTree(center,size);

            int dmLenX = distanceMap.GetLength(0);
            int dmLenY = distanceMap.GetLength(1);
            int dmLenZ = distanceMap.GetLength(2);
            for (int z = 0; z < dmLenZ; z++)
            {
                for (int y = 0; y < dmLenY; y++)
                {
                    for (int x = 0; x < dmLenX; x++)
                    {
                        Vector3 testPos = Map.instance.mapOrigin + new Vector3(x*Map.instance.distanceMapDetail,
                                                                               y*Map.instance.distanceMapDetail,
                                                                               z*Map.instance.distanceMapDetail);

                        root.Insert(distanceMap[x,y,z].distance, testPos);
                    }
                }
            }
            Console.WriteLine("weirdly done");
            Console.WriteLine(root.CountAllNodes());
        }
    }
}
