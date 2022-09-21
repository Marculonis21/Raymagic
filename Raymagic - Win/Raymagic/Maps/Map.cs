using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MessagePack;

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
        public Vector3 levelStartAnchor;
        public Vector3 levelEndAnchor;
        public string nextLevelID;
        public float nextLevelDetail;
        public int levelInputs;

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

        public bool gameMode = false;
        private Map()
        {
            maps = new Dictionary<string, MapData>();
        } 

        public static readonly Map instance = new Map();

        public bool enabledUpdate = true;
        public float portalMomentumConstant = 0.96787f; // portal velocity transfer MAGIC
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
            new Level1();

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
                BVH.BuildBVHDownUp(this.dynamicObjectList, this.interactableObjectList, false);
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
        public void SetMap(string id, bool gameMode=false)
        {
            this.gameMode = gameMode;

            LoadingMap ld = new LoadingMap();

            this.data = maps[id];

            if (maps[id].inDoor != null || maps[id].outDoor != null)
            {
                LoadingMap.AddLoadingMaps(ref this.data);
            }

            this.mapName = data.mapName;
            this.staticObjectList = data.staticMapObjects;
            this.dynamicObjectList = data.dynamicMapObjects;
            this.physicsObjectsList = data.physicsMapObjects;
            this.portalableObjectList.AddRange(this.physicsObjectsList.Where(x => !x.isTrigger));
            this.interactableObjectList = data.interactableObjectList;
            this.lightList = data.mapLights;
            this.nextLevelID = data.nextLevelID;
            this.nextLevelDetail = data.nextLevelDetail;
            this.levelInputs = data.gameLevelInputs;

            foreach (var item in interactableObjectList)
            {
                item.ObjectSetup(ref this.staticObjectList, ref this.dynamicObjectList, ref this.physicsObjectsList);
                item.ObjectStartup();
            }
            foreach (var item in physicsObjectsList)
            {
                item.ObjectSetup();
            }

            this.physicsSpace = new PhysicsSpace(physicsObjectsList);

            this.mapSize = data.topCorner - data.botCorner;
            this.mapOrigin = data.botCorner;
            this.mapTopCorner = data.topCorner;
            this.levelStartAnchor = data.levelStartAnchor;
            this.levelEndAnchor = data.levelEndAnchor;

            if (gameMode) // game mode skips all menu interactions and loads detail 2/4 (good enough) of selected map
            {
                this.distanceMapDetail = 2;

                if (id == "lvl4") // hack for starting this extra large map
                {
                    this.distanceMapDetail = 4;
                }
                
                distanceMap = new DMValue[(int)Math.Ceiling(mapSize.X/distanceMapDetail), 
                                          (int)Math.Ceiling(mapSize.Y/distanceMapDetail),
                                          (int)Math.Ceiling(mapSize.Z/distanceMapDetail)];

                BVH.BuildBVHDownUp(this.dynamicObjectList, this.interactableObjectList);
                this.distanceMap = LoadDistanceMap(id, this.distanceMapDetail, this.distanceMap);
                Console.CursorVisible = true;
                return;
            }

            Console.WriteLine("\nSelect distance map detail: (default 2 / large map 4) ");
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

            distanceMap = new DMValue[(int)Math.Ceiling(mapSize.X/distanceMapDetail), 
                                      (int)Math.Ceiling(mapSize.Y/distanceMapDetail),
                                      (int)Math.Ceiling(mapSize.Z/distanceMapDetail)];

            Console.WriteLine("");
            BVH.BuildBVHDownUp(this.dynamicObjectList, this.interactableObjectList);

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
                                                               z*distanceMapDetail), best.distance, out _);
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
        public bool mapPreloadingLoadingMap = false;
        public bool changeMap = false;
        Stopwatch sw = new Stopwatch();
        public void PreloadMap(string id="", float mapDetail=2, bool next=false)
        {
            if (!gameMode)
            {
                Console.WriteLine("Sandbox mode - no map preloading");
                return;
            }

            mapPreloadingLoadingMap = false;
            changeMap = false;

            if (id == "" && !next)
            {
                throw new Exception("Map preloading failed - need of next map ID");
            }
            if (!next && !maps.ContainsKey(id))
            {
                throw new Exception($"Map preloading failed - map ID {id} not found in map list");
            }

            Console.WriteLine("Loading next map");

            if (next)
            {
                id = this.nextLevelID;
                mapDetail = this.nextLevelDetail;
            }

            sw.Start();
            // START PRELOADING NEXT LEVEL
            mapPreloading = true;
            var _data                   = maps[id];

            if (_data.inDoor != null || _data.outDoor != null)
            {
                LoadingMap.AddLoadingMaps(ref _data);
            }

            var _mapName                = _data.mapName;
            var _staticObjectList       = _data.staticMapObjects;
            var _dynamicObjectList      = _data.dynamicMapObjects;
            var _physicsObjectsList     = _data.physicsMapObjects;
            var _portalableObjectList   = new List<IPortalable>();
            _portalableObjectList.AddRange(_physicsObjectsList.Where(x => !x.isTrigger));
            _portalableObjectList.Add(Player.instance);
            var _interactableObjectList = _data.interactableObjectList;
            var _lightList              = _data.mapLights;
            var _levelStartAnchor       = _data.levelStartAnchor;
            var _levelEndAnchor         = _data.levelEndAnchor;
            var _nextLevelID            = _data.nextLevelID;
            var _nextLevelDetail        = _data.nextLevelDetail;
            var _gameLevelInputs        = _data.gameLevelInputs;

            foreach (var item in _interactableObjectList)
            {
                item.ObjectSetup(ref _staticObjectList, ref _dynamicObjectList, ref _physicsObjectsList);
            }
            foreach (var item in _physicsObjectsList)
            {
                item.ObjectSetup();
            }

            var _BVH = new BVH();
            _BVH.BuildBVHDownUp(_dynamicObjectList, _interactableObjectList);

            var _physicsSpace = new PhysicsSpace(_physicsObjectsList);

            var _mapSize      = _data.topCorner - _data.botCorner;
            var _mapOrigin    = _data.botCorner;
            var _mapTopCorner = _data.topCorner;

            var _distanceMap = new DMValue[(int)(_mapSize.X/mapDetail), 
                                           (int)(_mapSize.Y/mapDetail),
                                           (int)(_mapSize.Z/mapDetail)];

            _distanceMap = LoadDistanceMap(id, mapDetail, _distanceMap);
            mapPreloading = false;

            if (_distanceMap == null) // failed to load distance map
            {
                Console.WriteLine("Loading failed - missing distance map data");
                return;
            }

            sw.Stop();
            /* Console.WriteLine($"DONEDONE {sw.ElapsedMilliseconds}"); */

            Console.WriteLine("Loading done");

            mapPreloadingLoadingMap = false;
            while (!changeMap) // if player still playing level, wait with data transfer to next level
            {
                Thread.Sleep(1000);
            }

            /* Console.WriteLine("Player and data transfer"); */
            Task.Run(() => { while(Screen.instance.DrawPhase) { } });

            this.data = _data;

            Player.instance.TranslateAbsolute(_levelStartAnchor + (Player.instance.position-this.levelEndAnchor));

            this.distanceMapDetail      = mapDetail;
            this.mapName                = _mapName;
            this.staticObjectList       = _staticObjectList;
            this.dynamicObjectList      = _dynamicObjectList;
            this.physicsObjectsList     = _physicsObjectsList;
            this.portalableObjectList   = _portalableObjectList;
            this.interactableObjectList = _interactableObjectList;
            this.lightList              = _lightList;
            this.BVH                    = _BVH;
            this.physicsSpace           = _physicsSpace;
            this.mapSize                = _mapSize;
            this.mapOrigin              = _mapOrigin;
            this.mapTopCorner           = _mapTopCorner;
            this.distanceMap            = _distanceMap;
            this.levelStartAnchor       = _levelStartAnchor;
            this.levelEndAnchor         = _levelEndAnchor;
            this.nextLevelID            = _nextLevelID;
            this.nextLevelDetail        = _nextLevelDetail;
            this.levelInputs            = _gameLevelInputs;
            this.portalList             = new List<Portal> { null, null };
            this.laserObjectList        = new List<Object>();

            foreach (var item in interactableObjectList)
            {
                item.ObjectStartup();
            }

            GC.Collect();
        }

        public void LoadingDoorClosed(Door2 door, bool IN)
        {
            var playerPos = Player.instance.position;
            if (Vector3.Dot(playerPos - door.Position, door.facing) > 0) // door closing but player on wrong side
            {
                return;
            }

            if (IN) // player in level - start next level loading
            {
                Task.Factory.StartNew(() => PreloadMap(next:true), TaskCreationOptions.LongRunning).Start();
            }
            else // coming out of the map into loading map
            {
                if (this.mapPreloading) // still loading next map - speed up
                {
                    this.mapPreloadingLoadingMap = true;
                }

                this.changeMap = true;
            }

        }

        public Vector3 GetPlayerStart()
        {
            return data.playerSpawn;
        }

        Stopwatch saveWatch = new Stopwatch();
        // SERIALIZATION - compression update from https://itecnote.com/tecnote/c-custom-serialization-deserialization-together-with-deflatestreams/
        public void SaveDistanceMap(string name, float distanceMapDetail)
        {
            saveWatch.Start();
            SaveContainer saveContainer = new SaveContainer();
            saveContainer.Setup(this.distanceMap);

            var bytes = MessagePackSerializer.Serialize(saveContainer);

            using (var uncompressed = new MemoryStream())
            using (var fileStream = File.Create($"Maps/Data/{name}-{distanceMapDetail}.dm"))
            {
                uncompressed.Write(bytes, 0, bytes.Length);
                uncompressed.Seek(0, SeekOrigin.Begin);

                using (var deflateStream = new DeflateStream(fileStream, CompressionMode.Compress))
                {
                    uncompressed.CopyTo(deflateStream);
                }
            }

            saveWatch.Stop();
            Console.WriteLine(saveWatch.ElapsedMilliseconds);
            saveWatch.Reset();

            GC.Collect();
        }

        public DMValue[,,] LoadDistanceMap(string name, float distanceMapDetail, DMValue[,,] distanceMap)
        {
            saveWatch.Start();
            try
            {
                using (var fileStream = File.OpenRead($"Maps/Data/{name}-{distanceMapDetail}.dm"))
                using (var decompressed = new MemoryStream())
                {
                    using (var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(decompressed);
                    }

                    decompressed.Seek(0, SeekOrigin.Begin);
                    SaveContainer saveContainer = (SaveContainer)MessagePackSerializer.Deserialize(typeof(SaveContainer), decompressed);
                    distanceMap = saveContainer.Deserialize(distanceMap);
                }

                Console.WriteLine($"Distance map Maps/Data/{name}-{distanceMapDetail}.dm loaded");

                saveWatch.Stop();
                Console.WriteLine(saveWatch.ElapsedMilliseconds);
                saveWatch.Reset();

                return distanceMap;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("LOAD ABORT - unable to load next distance map - file missing");
            }

            return null;
        }
    }
}
