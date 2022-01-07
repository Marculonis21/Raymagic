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

        public List<Object> staticObjectList = new List<Object>();
        public List<Object> dynamicObjectList = new List<Object>();
        public List<Light> lightList = new List<Light>();

        public Vector3 mapOrigin;
        public float distanceMapDetail = 2;
        public float[,,] distanceMap;

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
            this.staticObjectList = data.staticMapObjects;
            this.dynamicObjectList = data.dynamicMapObjects;
            this.lightList = data.mapLights;

            Vector3 mapSize = data.topCorner - data.botCorner;
            mapOrigin = data.botCorner;

            distanceMap = new float[(int)(mapSize.X/distanceMapDetail), (int)(mapSize.Y/distanceMapDetail),
                                    (int)(mapSize.Z/distanceMapDetail)];

            Console.WriteLine($"Create/Load - distance map (detail {this.distanceMapDetail}) (C/L)?>");
            string input = Console.ReadLine();
            if(input == "L" || input == "l")
            {
                LoadDistanceMap(id, this.distanceMapDetail);
                return;
            }
            else if(input != "C" && input != "c")
            {
                throw new Exception("DM not selected");
            }

            Console.WriteLine("baking distance field");
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

                    float d;
                    float dBest = 9999;
                    foreach(Object obj in this.staticObjectList)
                    {
                        d = obj.SDF(mapOrigin + new Vector3(x*distanceMapDetail, 
                                                            y*distanceMapDetail, 
                                                            z*distanceMapDetail),dBest);
                        if(d < dBest)
                            dBest = d;
                    }

                    distanceMap[x,y,z] = dBest;
                });
            }
            Console.CursorVisible = true;

            SaveDistanceMap(id, this.distanceMapDetail);
        }

        public void UpdateLightDynamicObjectList(MainGame game)
        {
            foreach(Light light in this.lightList)
            {
                light.dObjVisible.Clear();

                Vector3 start = light.position;
                foreach(Object dObj in this.dynamicObjectList)
                {
                    Vector3 dir = dObj.Position - start;
                    dir.Normalize();
                    game.PhysicsRayMarch(start, dir, 500, 0, out float length, out Vector3 hit, out Object hitObj);

                    if(hitObj == dObj)
                    {
                        light.dObjVisible.Add(dObj);
                    }
                }
            }
        }

        public Vector3 GetPlayerStart()
        {
            Vector3 spawn = data.playerSpawn;
            return new Vector3(spawn.X*100, spawn.Y*100, spawn.Z*100);
        }

        SaveContainer saveContainer;
        public void SaveDistanceMap(string name, float distanceMapDetail)
        {
            saveContainer = new SaveContainer(this.distanceMap);

            IFormatter formatter = new BinaryFormatter();  
            Stream stream = new FileStream($"Maps/{name}-{distanceMapDetail}.dm", FileMode.Create, FileAccess.Write, FileShare.None);  
            formatter.Serialize(stream, saveContainer);  
            stream.Close();  

            Console.WriteLine($"DistanceMap Maps/{name}-{distanceMapDetail}.dm saved");
        }

        public void LoadDistanceMap(string name, float distanceMapDetail)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();  
                Stream stream = new FileStream($"Maps/{name}-{distanceMapDetail}.dm", FileMode.Open, FileAccess.Read, FileShare.Read);  
                SaveContainer saveContainer = (SaveContainer)formatter.Deserialize(stream);  
                stream.Close(); 

                this.distanceMap = saveContainer.distanceMap;

                Console.WriteLine($"DistanceMap Maps/{name}-{distanceMapDetail}.dm loaded");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("unable to load");
            }
        }
    }
}
