using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Map
    {
        //SINGLETON
        public Dictionary<string, MapData> maps {get; private set;}
        MapData data;
        public string mapName;

        public List<Object> staticObjectList = new List<Object>();
        public List<Object> dynamicObjectList = new List<Object>();
        public BVH BVH = new BVH();
        public List<Object> infoObjectList = new List<Object>();
        public List<Light> lightList = new List<Light>();

        public Vector3 mapSize;
        public Vector3 mapOrigin;
        public Vector3 mapTopCorner;
        public float distanceMapDetail;
        public SDFout[,,] distanceMap;

        private Map()
        {
            maps = new Dictionary<string, MapData>();
        } 

        public static readonly Map instance = new Map();

        public void AddMap(string id, MapData data)
        {
            maps.Add(id, data);
        }

        public void LoadMaps()
        {
            new Basic();
            new TestArea();
        }

        public void SetMap(string id)
        {
            this.data = maps[id];
            this.mapName = id;
            this.staticObjectList = data.staticMapObjects;
            this.dynamicObjectList = data.dynamicMapObjects;
            this.lightList = data.mapLights;

            mapSize = data.topCorner - data.botCorner;
            mapOrigin = data.botCorner;
            mapTopCorner = data.topCorner;

            Console.WriteLine("\nSelect distance map detail: ");
            while (true)
            {
                if(float.TryParse(Console.ReadLine(), out float detail))
                {
                    this.distanceMapDetail = detail;
                    break;
                }
                else
                {
                    Console.WriteLine("\nEnter a float value...");
                }
            }

            /* Maps/{name}-{distanceMapDetail}.dm */

            distanceMap = new SDFout[(int)(mapSize.X/distanceMapDetail), 
                                     (int)(mapSize.Y/distanceMapDetail),
                                     (int)(mapSize.Z/distanceMapDetail)];

            Console.WriteLine("");
            BVH.BuildBVHDownUp();
            /* BdH.InfoPrint(); */

            OCTTree.LoadNeeded();

            if(File.Exists($"Maps/Data/{mapName}-{distanceMapDetail}.dm"))
            {
                Console.WriteLine("\nExisting distance map data found!");
                Console.WriteLine($"Create new/Load - distance map (detail {this.distanceMapDetail}) (C/L)?>");
                string input = Console.ReadLine();
                if(input == "L" || input == "l")
                {
                    Console.WriteLine("Loading from file...");
                    LoadDistanceMap(id, this.distanceMapDetail);

                    Console.WriteLine("Loading from OCTTree...");
                    LoadFromOCTTree();
                    return;
                }
                else if(input != "C" && input != "c")
                {
                    throw new Exception("Distance map option not selected");
                }
            }

            Console.WriteLine("Baking distance map...");
            Console.CursorVisible = false;
            for(int z = 0; z < mapSize.Z/distanceMapDetail; z++)
            {
                if(z != 0)
                    Console.SetCursorPosition(0,Console.CursorTop-1);

                int width = Console.WindowWidth - 5;

                for(int l = 1; l < (z+1)/(mapSize.Z/distanceMapDetail)*width; l++)
                {
                    Console.Write("■");
                }
                Console.CursorLeft = width+1;
                Console.Write($"{(int)((z+1)/(mapSize.Z/distanceMapDetail)*100)}%\n");

                int layer = (int)((mapSize.Y/distanceMapDetail) * (mapSize.X/distanceMapDetail));
                Parallel.For(0, layer, i => 
                {
                    int y = (int)(i / (mapSize.X/distanceMapDetail));
                    int x = (int)(i % (mapSize.X/distanceMapDetail));

                    SDFout test;
                    SDFout best = new SDFout(float.MaxValue, Color.Pink);
                    foreach(Object obj in this.staticObjectList)
                    {
                        test = obj.SDF(mapOrigin + new Vector3(x*distanceMapDetail, 
                                                               y*distanceMapDetail, 
                                                               z*distanceMapDetail), best.distance);
                        if(test.distance < best.distance)
                            best = test;
                    }

                    distanceMap[x,y,z] = best;
                });
            }

            Console.WriteLine("Saving distance map...");
            SaveDistanceMap(mapName, this.distanceMapDetail);

            Console.CursorVisible = true;
        }

        public Vector3 GetPlayerStart()
        {
            Vector3 spawn = data.playerSpawn;
            return new Vector3(spawn.X*100, spawn.Y*100, spawn.Z*100);
        }

        // SERIALIZATION
        public void SaveDistanceMap(string name, float distanceMapDetail)
        {
            SaveContainer saveContainer = new SaveContainer(this.distanceMap);

            IFormatter formatter = new BinaryFormatter();  

            Stream stream = new FileStream($"Maps/Data/{name}-{distanceMapDetail}.dm", FileMode.Create, FileAccess.Write, FileShare.None);  
            formatter.Serialize(stream, saveContainer);  
            stream.Close();  

            Console.WriteLine($"DistanceMap Maps/Data/{name}-{distanceMapDetail}.dm saved");
            saveContainer = null;
            
            GC.Collect();
        }

        public void LoadDistanceMap(string name, float distanceMapDetail)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();  
                Stream stream = new FileStream($"Maps/Data/{name}-{distanceMapDetail}.dm", FileMode.Open, FileAccess.Read, FileShare.Read);  
                SaveContainer saveContainer = (SaveContainer)formatter.Deserialize(stream);  
                stream.Close(); 

                this.distanceMap = saveContainer.Deserialize(this.distanceMap);

                Console.WriteLine($"Distance map Maps/Data/{name}-{distanceMapDetail}.dm loaded");
                saveContainer = null;

                GC.Collect();
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("unable to load");
            }
        }

        public void LoadFromOCTTree()
        {
            OCTTreeNode root = OCTTree.LoadRoot();

            for (int z = 0; z < distanceMap.GetLength(2); z++)
            {
                for (int y = 0; y < distanceMap.GetLength(1); y++)
                {
                    for (int x = 0; x < distanceMap.GetLength(0); x++)
                    {
                        Vector3 testPos = Map.instance.mapOrigin + new Vector3(x*distanceMapDetail,
                                                                               y*distanceMapDetail,
                                                                               z*distanceMapDetail);

                        distanceMap[x,y,z] = new SDFout(root.Search(testPos), distanceMap[x,y,z].color);
                    }
                }
            }

        }
    }
}
