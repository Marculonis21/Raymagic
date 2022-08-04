using System;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    [Serializable]
    class SaveContainer
    {
        float[][][] distanceMap;
        byte[][][][] colorMap;
        int[][][] objIndex;

        public SaveContainer(DMValue[][][] map)
        {
            /* distanceMap = new float[map.GetLength(0),map.GetLength(1),map.GetLength(2)]; */
            /* colorMap = new byte[map.GetLength(0),map.GetLength(1),map.GetLength(2),3]; */
            /* objIndex = new int[map.GetLength(0),map.GetLength(1),map.GetLength(2)]; */

            distanceMap = new float[map.Length][][];
                colorMap = new byte[map.Length][][][];
                 objIndex = new int[map.Length][][];
            for (int x = 0; x < map.Length; x++)
            {
                distanceMap[x] = new float[map[x].Length][];
                   colorMap[x] =  new byte[map[x].Length][][];
                   objIndex[x] =   new int[map[x].Length][];
                for (int y = 0; y < map[x].Length; y++)
                {
                    distanceMap[x][y] = new float[map[x][y].Length];
                       colorMap[x][y] =  new byte[map[x][y].Length][];
                       objIndex[x][y] =   new int[map[x][y].Length];

                    for (int z = 0; z < map[x][y].Length; z++)
                    {
                        colorMap[x][y][z] = new byte[3];
                    }
                }
            }

            for (int z = 0; z < map[0][0].Length; z++)
            {
                for (int y = 0; y < map[0].Length; y++)
                {
                    for (int x = 0; x < map.Length; x++)
                    {
                        /* distanceMap[x,y,z] = map[x,y,z].sdfValue.distance; */
                        /* colorMap[x,y,z,0] = map[x,y,z].sdfValue.color.R; */
                        /* colorMap[x,y,z,1] = map[x,y,z].sdfValue.color.G; */
                        /* colorMap[x,y,z,2] = map[x,y,z].sdfValue.color.B; */
                        /* objIndex[x,y,z] = map[x,y,z].objIndex; */

                        distanceMap[x][y][z] = map[x][y][z].sdfValue.distance;
                        colorMap[x][y][z][0] = map[x][y][z].sdfValue.color.R;
                        colorMap[x][y][z][1] = map[x][y][z].sdfValue.color.G;
                        colorMap[x][y][z][2] = map[x][y][z].sdfValue.color.B;
                        objIndex[x][y][z]    = map[x][y][z].objIndex;
                    }
                }
            }
        }

        public DMValue[][][] Deserialize(DMValue[][][] map)
        {
            /* DMValue[][][] newMap = new DMValue[map.GetLength(0),map.GetLength(1),map.GetLength(2)]; */

            DMValue[][][] newMap = new DMValue[map.Length][][];

            for (int x = 0; x < map.Length; x++)
            {
                newMap[x] = new DMValue[map[x].Length][];
                for (int y = 0; y < map[x].Length; y++)
                {
                    newMap[x][y] = new DMValue[map[x][y].Length];
                }
            }
            for (int z = 0; z < map[0][0].Length; z++)
            {
                for (int y = 0; y < map[0].Length; y++)
                {
                    for (int x = 0; x < map.Length; x++)
                    {
                        newMap[x][y][z] = new DMValue(objIndex[x][y][z],
                                                      new SDFout(distanceMap[x][y][z],
                                                                 new Color(colorMap[x][y][z][0],
                                                                           colorMap[x][y][z][1],
                                                                           colorMap[x][y][z][2])));
                    }
                }
            }

            return newMap;
        }
    }
}
