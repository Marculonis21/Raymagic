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

        string txtMapsPath = "Maps/GameMaps/";

        public string mapName;

        public Vector3 mapSize;
        public Vector3 mapOrigin;
        public Vector3 mapTopCorner;
        public float distanceMapDetail;
        public DMValue[,,] distanceMap;

        public List<Object> staticObjectList = new List<Object>();
        public List<Object> dynamicObjectList = new List<Object>();
        public List<Object> infoObjectList = new List<Object>();
        public List<Object> laserObjectList = new List<Object>();
        public List<IPortalable> portalableObjectList = new List<IPortalable>();
        public List<PhysicsObject> physicsObjectsList = new List<PhysicsObject>();
        public List<Interactable> interactableObjectList = new List<Interactable>();

        public List<Light> lightList = new List<Light>();
        public List<Portal> portalList = new List<Portal> {null, null}; 

        public BVH BVH = new BVH();

        public PhysicsSpace physicsSpace;

        public float gravity = 3000f;

        private Map()
        {
            maps = new Dictionary<string, MapData>();
        } 

        public static readonly Map instance = new Map();

        public bool enabledUpdate = true;
        public float portalMomentumConstant = 0.96787f;
        public void Update(GameTime gameTime)
        {
            if (enabledUpdate)
            {
                physicsSpace.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        public void RegisterMap(string id, MapData data)
        {
            if (maps.ContainsKey(id))
            {
                maps[id] = data;
            }
            else
            {
                maps.Add(id, data);
            }
        }

        public void LoadMaps()
        {
            new TestArea();
            new Showcase();

            string[] files = Directory.GetFiles(txtMapsPath, "*.map");

            Console.WriteLine($"found {files.Length} files for TxtMapCompiler");
            for (int i = 0; i < files.Length; i++)
            {
                Task.WaitAll(TxtMapCompiler.instance.CompileFile(files[i]));
            }

            if (files.Length > 0)
                Console.WriteLine("Compilation process done");
        }

        public async void ReloadMap()
        {
            if (data.isCompiled)
            {
                await TxtMapCompiler.instance.CompileFile(data.path, false);

                this.data = maps[this.data.mapName];

                this.dynamicObjectList = data.dynamicMapObjects;
                this.physicsObjectsList = data.physicsMapObjects;
                this.portalableObjectList.AddRange(this.physicsObjectsList.Where(x => !x.isTrigger));
                this.lightList = data.mapLights;
                this.physicsSpace = new PhysicsSpace(physicsObjectsList);
                this.infoObjectList = new List<Object>();

                mapSize = data.topCorner - data.botCorner;
                mapOrigin = data.botCorner;
                mapTopCorner = data.topCorner;

                this.BVH = new BVH();
                BVH.BuildBVHDownUp(false);
            }
        }

        public bool activeMapReload = false;
        public async Task MapReloadingAsync(int reloadTime)
        {
            while (this.activeMapReload)
            {
                await Task.Run(() => { while(Screen.instance.DrawPhase) { } } ).ContinueWith( t1 => ReloadMap() );
                await Task.Delay(reloadTime);
            }
            Informer.instance.RemoveInfo("map_compilation1");
            Informer.instance.RemoveInfo("map_compilation2");
        }

        public void SetMap(string id)
        {
            this.data = maps[id];
            this.mapName = data.mapName;
            this.staticObjectList = data.staticMapObjects;
            this.dynamicObjectList = data.dynamicMapObjects;
            this.physicsObjectsList = data.physicsMapObjects;
            this.portalableObjectList.AddRange(this.physicsObjectsList.Where(x => !x.isTrigger));
            this.interactableObjectList = data.interactableObjectList;
            this.lightList = data.mapLights;

            foreach (var item in interactableObjectList)
            {
                item.ObjectSetup();
            }

            this.physicsSpace = new PhysicsSpace(physicsObjectsList);

            foreach (var item in this.physicsObjectsList)
            {
                Console.WriteLine(item.GetType());
            }

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
            distanceMap = new DMValue[(int)(mapSize.X/distanceMapDetail), 
                                      (int)(mapSize.Y/distanceMapDetail),
                                      (int)(mapSize.Z/distanceMapDetail)];

            Console.WriteLine("");
            BVH.BuildBVHDownUp();
            /* BdH.InfoPrint(); */

            if(File.Exists($"Maps/Data/{mapName}-{distanceMapDetail}.dm"))
            {
                Console.WriteLine("\nExisting distance map data found!");
                Console.WriteLine($"Create new/Load - distance map (detail {this.distanceMapDetail}) (C/L)?>");
                string input = Console.ReadLine();
                if(input == "L" || input == "l")
                {
                    Console.WriteLine("Loading from file...");
                    this.distanceMap = LoadDistanceMap(id, this.distanceMapDetail, this.distanceMap);
                    GC.Collect();

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
                    int bestObjIndex = 0;
                    int index = 0;
                    foreach(Object obj in this.staticObjectList)
                    {
                        test = obj.SDF(mapOrigin + new Vector3(x*distanceMapDetail, 
                                                               y*distanceMapDetail, 
                                                               z*distanceMapDetail), best.distance);
                        if(test.distance < best.distance)
                        {
                            best = test;
                            bestObjIndex = index;
                        }
                        index++;
                    }

                    distanceMap[x,y,z] = new DMValue(bestObjIndex, best);
                });
            }

            Console.WriteLine("Saving distance map...");
            SaveDistanceMap(mapName, this.distanceMapDetail);

            Console.CursorVisible = true;
        }

        public bool mapPreloading = false;
        public void PreLoadMap(string id)
        {
            mapPreloading = true;
            var _data = maps[id];
            var _mapName = _data.mapName;
            var _staticObjectList   = _data.staticMapObjects;
            var _dynamicObjectList  = _data.dynamicMapObjects;
            var _physicsObjectsList = _data.physicsMapObjects;
            var _portalableObjectList = new List<IPortalable>();
            _portalableObjectList.AddRange(_physicsObjectsList.Where(x => !x.isTrigger));
            var _interactableObjectList = _data.interactableObjectList;
            var _lightList = _data.mapLights;

            /* foreach (var item in _interactableObjectList) */
            /* { */
            /*     item.ObjectSetup(); */
            /* } */

            /* BVH.BuildBVHDownUp(); */

            var _physicsSpace = new PhysicsSpace(physicsObjectsList);

            var _mapSize      = _data.topCorner - _data.botCorner;
            var _mapOrigin    = _data.botCorner;
            var _mapTopCorner = _data.topCorner;

            var _distanceMap = new DMValue[(int)(_mapSize.X/distanceMapDetail), 
                                           (int)(_mapSize.Y/distanceMapDetail),
                                           (int)(_mapSize.Z/distanceMapDetail)];

            _distanceMap = LoadDistanceMap(id, distanceMapDetail, _distanceMap);

            mapPreloading = false;
        }

        public Vector3 GetPlayerStart()
        {
            return data.playerSpawn;
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

        public DMValue[,,] LoadDistanceMap(string name, float distanceMapDetail, DMValue[,,] distanceMap)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();  
                Stream stream = new FileStream($"Maps/Data/{name}-{distanceMapDetail}.dm", FileMode.Open, FileAccess.Read, FileShare.Read);  
                SaveContainer saveContainer = (SaveContainer)formatter.Deserialize(stream);  
                stream.Close(); 

                distanceMap = saveContainer.Deserialize(distanceMap);

                Console.WriteLine($"Distance map Maps/Data/{name}-{distanceMapDetail}.dm loaded");
                saveContainer = null;

                return distanceMap;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("unable to load");
            }

            return null;
        }
    }
}
