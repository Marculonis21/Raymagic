using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    [Serializable]
    class SaveContainer
    {
        /* SDFout[,,] distanceMap; */
        float[,,] distanceMap;
        byte[,,,] colorMap;

        public SaveContainer(SDFout[,,] map)
        {
            distanceMap = new float[map.GetLength(0),map.GetLength(1),map.GetLength(2)];
            colorMap = new byte[map.GetLength(0),map.GetLength(1),map.GetLength(2),3];

            for (int z = 0; z < map.GetLength(2); z++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    for (int x = 0; x < map.GetLength(0); x++)
                    {
                        distanceMap[x,y,z] = map[x,y,z].distance;
                        colorMap[x,y,z,0] = map[x,y,z].color.R;
                        colorMap[x,y,z,1] = map[x,y,z].color.G;
                        colorMap[x,y,z,2] = map[x,y,z].color.B;
                    }
                }
            }
        }

        public SDFout[,,] Deserialize(SDFout[,,] map)
        {
            SDFout[,,] newMap = new SDFout[map.GetLength(0),map.GetLength(1),map.GetLength(2)];

            for (int z = 0; z < map.GetLength(2); z++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    for (int x = 0; x < map.GetLength(0); x++)
                    {
                        newMap[x,y,z] = new SDFout(distanceMap[x,y,z], new Color(colorMap[x,y,z,0],
                                                                                 colorMap[x,y,z,1],
                                                                                 colorMap[x,y,z,2]));
                    }
                }
            }

            return newMap;
        }
    }
}
