using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using CColor = System.Drawing.Color;
using System.Linq;

namespace Raymagic
{
    public class TxtMapCompiler
    {
        //SINGLETON
        Map map;
        List<string> possibleObjects;
        List<string> possibleOperations;

        Dictionary<string, Object> declaredObjectsStatic;
        Dictionary<string, Object> declaredObjectsDynamic;
        Dictionary<string, Tuple<PhysicsObject, Type>> declaredPhysicsObjects;
        Dictionary<string, Tuple<Interactable, Type>> declaredInteractableObjects;

        Dictionary<string, Vector3> declaredVectorVars;
        Dictionary<string, Color> declaredColorVars;
        Dictionary<string, Vector3> declaredVectorVarsPersistant;
        Dictionary<string, Color> declaredColorVarsPersistant;

        enum ObjectCategory
        {
            STATIC,
            DYNAMIC,
            PHYSICS,
            INTERACTABLE,
        }

        private TxtMapCompiler()
        {
            map = Map.instance;

            declaredObjectsStatic = new Dictionary<string, Object>();

            declaredVectorVarsPersistant = new Dictionary<string, Vector3>();
            declaredColorVarsPersistant = new Dictionary<string, Color>();

            possibleObjects = new List<string>() {
                "box",
                "box_frame",
                "capsule",
                "cylinder",
                "plane",
                "sphere",

                "light",

                "physics_object",
                "physics_trigger",
                "mirror_ball",

                "button",
                "floor_button",
                "door",
                "lifter",
                "jumper",
                "laser_spawner",
                "laser_catcher",
                "portal_spawner",
                "ball_spawner",
            };

            possibleOperations = new List<string>() {
                "rotateX",
                "rotateY",
                "rotateZ",
                "rotate",
                "repeat",
                "symmetry",
                "connect",
                "boolean"
            };
        } 

        public static readonly TxtMapCompiler instance = new TxtMapCompiler();

        MapData data;

        public async Task CompileFile(string path, bool full = true)
        {
            if (path == "") throw new ArgumentNullException("Compiler needs a map file input");
            if (!path.EndsWith(".map")) throw new ArgumentException("Incorrect file type - needs .map file type");

            string[] input = GetTextFromFile(path);

            if (full)
            {
                declaredObjectsStatic = new Dictionary<string, Object>();    
            }

            declaredObjectsDynamic = new Dictionary<string, Object>();

            declaredPhysicsObjects = new Dictionary<string, Tuple<PhysicsObject, Type>>();
            declaredInteractableObjects = new Dictionary<string, Tuple<Interactable, Type>>();

            declaredVectorVars = new Dictionary<string, Vector3>();
            declaredColorVars = new Dictionary<string, Color>();

            data = new MapData();
            data.isCompiled = true;
            data.path = path;

            try
            {
                await Task.Run(() => ParseConfigAsync(input));
                List<Task> tasks = new List<Task>();

                if (full)
                {
                    // they contain static elements - needs full recompile
                    // awaits for consistency
                    tasks.Add(ParseStaticAsync(input));
                    tasks.Add(ParseDynamicAsync(input));
                    tasks.Add(ParseLightsAsync(input));
                    tasks.Add(ParsePhysicsAsync(input));
                    tasks.Add(ParseInteractableAsync(input));
                    tasks.Add(ParsePortalableAsync(input));
                }
                else
                {
                    tasks.Add(ParseDynamicAsync(input));
                    tasks.Add(ParseLightsAsync(input));
                    tasks.Add(ParsePhysicsAsync(input));
                }

                await Task.WhenAll(tasks);

                if (!full)
                {
                    Informer.instance.AddInfo("map_compilation1", $"Compiling done", true);
                    Informer.instance.AddInfo("map_compilation2", "", true);

                    string s = "";
                    foreach (var item in declaredVectorVars)
                    {
                        s += $"{item.Key}:{item.Value}, ";
                    }
                    Informer.instance.AddInfo("declareVectors", s, true);

                    s = "";
                    foreach (var item in declaredColorVars)
                    {
                        s += $"{item.Key}:{item.Value}, ";
                    }
                    Informer.instance.AddInfo("declareColors", s, true);
                }
            }
            catch (FormatException e)
            {
                if (full)
                {
                    Console.WriteLine($"Error while compiling {path}");
                    Console.WriteLine(e.Message);
                }
                else
                {
                    Informer.instance.AddInfo("map_compilation1", $"Error while compiling {path}", true);
                    Informer.instance.AddInfo("map_compilation2", e.Message, true);
                }
                return;
            }

            Map.instance.RegisterMap(data.mapName, data);
        }

        private string[] GetTextFromFile(string path)
        {
            StreamReader reader;
            reader = new StreamReader(path);
            var full = reader.ReadToEnd();
            string[] allLines = full.Split("\n");
            for (int i = 0; i < allLines.Length; i++)
            {
                allLines[i] = allLines[i].Trim();
            }

            return allLines;
        }

        private async Task ParseConfigAsync(string[] input)
        {
            Tuple<int,int> startEnd = GetBlockStartEnd("config", input, true);
            (int start, int end) = startEnd;

            Dictionary<string, bool> partsFound = new Dictionary<string, bool>();
            partsFound.Add("name",false);
            partsFound.Add("player_spawn",false);
            partsFound.Add("top_corner",false);
            partsFound.Add("bot_corner",false);

            int lineNum;
            for (int i = start+1; i < end; i++)
            {
                var line = input[i];
                lineNum = i+1;

                if (line.StartsWith("#")) continue;

                if (line.StartsWith("name:"))
                {
                    partsFound["name"] = true;
                    var mapName = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.mapName = mapName;
                }
                else if (line.StartsWith("player_spawn:"))
                {
                    partsFound["player_spawn"] = true;
                    var spawn = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.playerSpawn = GetVector3FromText(spawn, lineNum);
                }
                else if (line.StartsWith("top_corner:"))
                {
                    partsFound["top_corner"] = true;
                    var tc = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.topCorner = GetVector3FromText(tc, lineNum);
                }
                else if (line.StartsWith("bot_corner:"))
                {
                    partsFound["bot_corner"] = true;
                    var bc = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.botCorner = GetVector3FromText(bc, lineNum);
                }
                else if (line.StartsWith("anchor_start:"))
                {
                    /* partsFound["anchor_start"] = true; */
                    var As = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.levelStartAnchor = GetVector3FromText(As, lineNum);
                }
                else if (line.StartsWith("anchor_end:"))
                {
                    /* partsFound["anchor_end"] = true; */
                    var ae = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.levelEndAnchor = GetVector3FromText(ae, lineNum);
                }
                else if (line.StartsWith("next_lvl:"))
                {
                    var nextLvlID = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.nextLevelID = nextLvlID;
                }
                else if (line.StartsWith("next_lvl_detail:"))
                {
                    var nextLvlDetail = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.nextLevelDetail = float.Parse(nextLvlDetail);
                }
                else if (line.StartsWith("next_lvl_door_color:"))
                {
                    var nextLvlDoorColor = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.nextLevelInColor = GetColorFromText(nextLvlDoorColor, lineNum);
                }
                else if (line.StartsWith("last_lvl_door_color:"))
                {
                    var lastLvlDoorColor = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.lastLevelInColor = GetColorFromText(lastLvlDoorColor, lineNum);
                }
                else if (line.StartsWith("game_lvl_order:"))
                {
                    var gameLvlOrder = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.gameLevelOrder = int.Parse(gameLvlOrder);
                }
                else if (line.StartsWith("game_lvl_name:"))
                {
                    var gameLvlName = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.gameLevelName = gameLvlName;
                }
                else if (line.StartsWith("game_lvl_inputs:"))
                {
                    var gameLvlInputs = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    int inputs = int.Parse(gameLvlInputs);
                    if (inputs < 0 || inputs > 2)
                    {
                        throw new FormatException($"Format warning line {lineNum} - game inputs can be only 0 - no portals, 1 - blue portal, 2 - both portals");
                    }
                    data.gameLevelInputs = inputs;
                }
                else    
                {
                    throw new FormatException($"Format warning line {lineNum} - Unknown config input - expects 'name', 'player_spawn', 'top_corner', 'bot_corner'");
                }
            }

            if (!partsFound["name"])
            {
                throw new FormatException("config - missing 'name' attribute");
            }
            if (!partsFound["player_spawn"])
            {
                throw new FormatException("config - missing 'player_spawn' attribute");
            }
            if (!partsFound["top_corner"])
            {
                throw new FormatException("config - missing 'top_corner' attribute");
            }
            if (!partsFound["bot_corner"])
            {
                throw new FormatException("config - missing 'bot_corner' attribute");
            }
        }

        private async Task ParseStaticAsync(string[] input)
        {
            Tuple<int,int> startEnd = GetBlockStartEnd("static", input, true);
            (int start, int end) = startEnd;

            List<Object> objectList = new List<Object>();

            string[] objectPart;
            string[] paramPart;

            string objectType;
            string objectInfoName;

            int lineNum;
            for (int i = start+1; i < end; i++)
            {
                lineNum = i+1;

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;
                if (LineContainsHelp(line, lineNum)) {}

                if (LineContainsObject(line, lineNum, out objectPart, out paramPart))
                {
                    objectInfoName = objectPart[1];

                    Object obj = GetObjectFromData(ObjectCategory.STATIC, lineNum, objectPart, paramPart, BooleanOP.NONE, 0, out Type type);

                    try
                    {
                        declaredObjectsStatic.Add(objectInfoName, obj);
                    }
                    catch (ArgumentException)
                    {
                        throw new FormatException($"Format error line {lineNum} - multiple objects cannot share info name");
                    }
                    objectList.Add(obj);
                }
                else if (LineContainsOperation(line, lineNum, out string operationType, out string targetObject, out string[] operationContent))
                {
                    if (!declaredObjectsStatic.ContainsKey(targetObject)) throw new FormatException($"NullReference error line {lineNum} - object {targetObject} was not yet declared");

                    AssignOperationFromData(ObjectCategory.STATIC, lineNum, operationType, targetObject, operationContent);
                }
                else if (LineContainsVariableDeclaration(line, lineNum, true))
                {
                }
                else    
                {
                    throw new FormatException($"Format warning line {lineNum} - Unknown input");
                }
            }

            data.staticMapObjects.AddRange(objectList);
        }

        private async Task ParseDynamicAsync(string[] input)
        {
            Tuple<int,int> startEnd = GetBlockStartEnd("dynamic", input, true);
            (int start, int end) = startEnd;

            List<Object> objectList = new List<Object>();

            string[] objectPart;
            string[] paramPart;

            string objectType;
            string objectInfoName;

            int lineNum;
            for (int i = start+1; i < end; i++)
            {
                lineNum = i+1;

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;
                if (LineContainsHelp(line, lineNum)) {}

                if (LineContainsObject(line, lineNum, out objectPart, out paramPart))
                {
                    Object obj = GetObjectFromData(ObjectCategory.DYNAMIC, lineNum, objectPart, paramPart, BooleanOP.NONE, 0, out Type type);

                    objectType = objectPart[0];
                    objectInfoName = objectPart[1];

                    try
                    {
                        declaredObjectsDynamic.Add(objectInfoName, obj);
                    }
                    catch (ArgumentException)
                    {
                        throw new FormatException($"Format error line {lineNum} - multiple objects cannot share info name");
                    }
                    
                    objectList.Add(obj);
                }
                else if (LineContainsOperation(line, lineNum, out string operationType, out string targetObject, out string[] operationContent))
                {
                    if (!declaredObjectsDynamic.ContainsKey(targetObject)) throw new FormatException($"NullReference error line {lineNum} - object {targetObject} was not yet declared");

                    AssignOperationFromData(ObjectCategory.STATIC, lineNum, operationType, targetObject, operationContent);
                }
                else if (LineContainsVariableDeclaration(line, lineNum, false))
                {
                }
                else    
                {
                    throw new FormatException($"Format warning line {lineNum} - Unknown input");
                }
            }

            data.dynamicMapObjects.AddRange(objectList);
        }

        private async Task ParseLightsAsync(string[] input)
        {
            Tuple<int,int> startEnd = GetBlockStartEnd("lights", input, true);
            (int start, int end) = startEnd;

            string[] objectPart;
            string[] paramPart;

            string objectType;

            int lineNum;
            for (int i = start+1; i < end; i++)
            {
                lineNum = i+1;

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;
                if (LineContainsHelp(line, lineNum)) {}

                if (LineContainsObject(line, lineNum, out objectPart, out paramPart))
                {
                    objectType = objectPart[0];

                    if (objectType == "light")
                    {
                        Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                        Color color = GetColorFromText(paramPart[1], lineNum);
                        float intensity = float.Parse(paramPart[2]);

                        Vector3 botCorner = data.botCorner;
                        Vector3 topCorner = data.topCorner;
                        if (paramPart.Length == 5)
                        {
                            botCorner = GetVector3FromText(paramPart[3], lineNum);
                            topCorner = GetVector3FromText(paramPart[4], lineNum);
                        }

                        Light light = new Light(position, color, intensity, botCorner, topCorner);
                        data.mapLights.Add(light);
                    }
                    else
                    {
                        throw new FormatException($"Format error line {lineNum} - Cannot add object type '{objectType}' into lights block)");
                    }
                }
                else
                {
                    throw new FormatException($"Format warning line {lineNum} - Lights block can have only objects of type light");
                }
            }
        }

        private async Task ParsePhysicsAsync(string[] input)
        {
            Tuple<int,int> startEnd = GetBlockStartEnd("physics", input, false);
            (int start, int end) = startEnd;

            List<PhysicsObject> pObjectList = new List<PhysicsObject>();
            List<string> pIndexList = new List<string>();

            string[] objectPart;
            string[] paramPart;

            string objectType;
            string objectInfoName;

            int lineNum;
            for (int i = start+1; i < end; i++)
            {
                lineNum = i+1;

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;
                if (LineContainsHelp(line, lineNum)) {}

                if (LineContainsObject(line, lineNum, out objectPart, out paramPart))
                {
                    objectType = objectPart[0];
                    objectInfoName = objectPart[1];

                    PhysicsObject pObj = GetPhysicsObjectFromData(ObjectCategory.PHYSICS, lineNum, objectPart, paramPart, out Type type);

                    try
                    {
                        declaredPhysicsObjects.Add(objectInfoName, new Tuple<PhysicsObject, Type>(pObj, type));
                    }
                    catch (ArgumentException)
                    {
                        throw new FormatException($"Format error line {lineNum} - multiple objects cannot share info name");
                    }
                    data.physicsMapObjects.Add(pObj);
                }
                else if (LineContainsVariableDeclaration(line, lineNum, false))
                {
                }
                else
                {
                    throw new FormatException($"Format warning line {lineNum} - Unknown input");
                }
            }
        }

        private async Task ParseInteractableAsync(string[] input)
        {
            Tuple<int,int> startEnd = GetBlockStartEnd("interactables", input, false);
            (int start, int end) = startEnd;

            string[] objectPart;
            string[] paramPart;

            string objectType;
            string objectInfoName;

            int lineNum;
            for (int i = start+1; i < end; i++)
            {
                /* await Task.Yield(); */
                lineNum = i+1;

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;
                if (LineContainsHelp(line, lineNum)) {}

                if (LineContainsObject(line, lineNum, out objectPart, out paramPart))
                {
                    objectType = objectPart[0];
                    objectInfoName = objectPart[1];

                    Interactable iObj = GetInteractableObjectFromData(ObjectCategory.INTERACTABLE, lineNum, objectPart, paramPart, out Type type);
                        
                    try
                    {
                        declaredInteractableObjects.Add(objectInfoName, new Tuple<Interactable, Type>(iObj, type));
                    }
                    catch (ArgumentException)
                    {
                        throw new FormatException($"Format error line {lineNum} - multiple objects cannot share info name");
                    }
                    data.interactableObjectList.Add(iObj);
                    if (objectInfoName.ToLower() == "indoor")
                    {
                        data.inDoor = iObj as Door2;
                    }
                    if (objectInfoName.ToLower() == "outdoor")
                    {
                        data.outDoor = iObj as Door2;
                    }
                }
                else if (LineContainsOperation(line, lineNum, out string operationType, out string _, out string[] operationContent))
                {
                    if (operationType != "connect") throw new FormatException($"Format error line {lineNum} - Interactable object allow only 'connect' operation");

                    string from = operationContent[0];
                    string to = operationContent[1];

                    // connect: from -> to 
                    // input can be interactable or trigger - output must be interactabl
                    if (!declaredInteractableObjects.ContainsKey(from) && !declaredPhysicsObjects.ContainsKey(from)) 
                        throw new FormatException($"Format error line {lineNum} - 'From' connection object not found");

                    if (!declaredInteractableObjects.ContainsKey(to))
                        throw new FormatException($"Format error line {lineNum} - 'To' connection object not found");

                    // from is interactable
                    if (declaredInteractableObjects.ContainsKey(from))
                    {
                        declaredInteractableObjects[from].Deconstruct(out Interactable fromObj, out Type fromType);
                        declaredInteractableObjects[to].Deconstruct(out Interactable toObj, out Type toType);

                        fromObj.stateChangeEvent += toObj.EventListener;
                    }
                    else 
                    {
                        declaredPhysicsObjects[from].Deconstruct(out PhysicsObject pObj, out Type pType);
                        declaredInteractableObjects[to].Deconstruct(out Interactable toObj, out Type toType);

                        if (pType == typeof(PhysicsTrigger))
                        {
                            var trigger = (pObj as PhysicsTrigger);

                            if (toType == typeof(Door2))
                            {
                                var door = (toObj as Door2);

                                trigger.onCollisionEnter += door.TriggerEnter;
                                trigger.onCollisionExit += door.TriggerExit;
                            }
                            else if (toType == typeof(Lifter))
                            {
                                var lifter = (toObj as Lifter);

                                trigger.onCollisionEnter += lifter.TriggerEnter;
                                trigger.onCollisionExit += lifter.TriggerExit;
                            }
                        }
                        else
                        {
                            throw new FormatException($"Format error line {lineNum} - Cannot use physics ball as an event trigger");
                        }
                        
                    }
                }
                else if (LineContainsVariableDeclaration(line, lineNum, true))
                {
                }
                else
                {
                    throw new FormatException($"Format warning line {lineNum} - Unknown input");
                }
            }

        }

        private async Task ParsePortalableAsync(string[] input)
        {
            Tuple<int,int> startEnd = GetBlockStartEnd("portalable", input, false);
            (int start, int end) = startEnd;

            int lineNum;
            for (int i = start+1; i < end; i++)
            {
                lineNum = i+1;

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;
                if (LineContainsHelp(line, lineNum)) {}

                if (declaredObjectsStatic.ContainsKey(line))
                {
                    Console.WriteLine($"static {line}");
                    declaredObjectsStatic[line].SetPortalable(true);
                }
                else if (declaredObjectsDynamic.ContainsKey(line))
                {
                    Console.WriteLine($"dynamic {line}");
                    declaredObjectsDynamic[line].SetPortalable(true);
                }
                else
                {
                    throw new FormatException($"Format error line {lineNum} - Portalable block inputs are only names of declared object on new lines to set them portalable");
                }
            }
        }

        private Tuple<int, int> GetBlockStartEnd(string target, string[] input, bool necessary)
        {
            Tuple<int, int> startEndRecord = new Tuple<int, int>(-1,-1);

            int brackets = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == target && input[i+1] == "{")
                {
                    if (input[i+1] == "{")
                    {
                        startEndRecord = new Tuple<int, int>(i+1, startEndRecord.Item2);
                        brackets += 1;
                        i += 1;
                        continue;
                    }
                    else
                    {
                        throw new FormatException($"{target} - missing opening at line {i+1} brackets");
                    }
                }
                else if (brackets == 0)
                {
                    continue;
                }

                switch (input[i])
                {
                    case "{":
                        brackets += 1;
                        break;
                    case "}":
                        brackets -= 1;
                        break;

                    default:
                        break;
                }

                if (brackets == 0)
                {
                    startEndRecord = new Tuple<int, int>(startEndRecord.Item1, i);
                    break;
                }
            }
            if (brackets != 0) throw new FormatException($"{target} - block starting at line {startEndRecord.Item1} not closed");
            if (necessary && startEndRecord.Item1 == -1) throw new FormatException($"{target} - necessary block missing");

            return startEndRecord;
        }

        private Object GetObjectFromData(ObjectCategory category, int lineNum, string[] objectPart, string[] paramPart, BooleanOP boolean, float booleanStrength, out Type type)
        {
            Object obj;

            var objectType = objectPart[0];
            var objectInfoName = objectPart[1];

            try
            {
                switch (objectType)
                {
                    case "box":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            Vector3 size = GetVector3FromText(paramPart[1], lineNum);
                            Color color = GetColorFromText(paramPart[2], lineNum);
                            Vector3 boundingSize = new Vector3();
                            if (category == ObjectCategory.DYNAMIC)
                            {
                                boundingSize = GetVector3FromText(paramPart[3], lineNum);
                            }
                            obj = new Box(position, size, color, boolean, booleanStrength, boundingSize, info:objectInfoName);
                            obj.SetTransparent(paramPart.Contains("transparent"));
                        }
                        break;
                    case "box_frame":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            Vector3 size = GetVector3FromText(paramPart[1], lineNum);
                            float frameSize = float.Parse(paramPart[2]);
                            Color color = GetColorFromText(paramPart[3], lineNum);
                            Vector3 boundingSize = new Vector3();
                            if (category == ObjectCategory.DYNAMIC)
                            {
                                boundingSize = GetVector3FromText(paramPart[4], lineNum);
                            }
                            obj = new BoxFrame(position, size, frameSize, color, boolean, booleanStrength, boundingSize, info:objectInfoName);
                            obj.SetTransparent(paramPart.Contains("transparent"));
                        }
                        break;
                    case "capsule":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            float height = float.Parse(paramPart[1]);
                            float radius = float.Parse(paramPart[2]);
                            Color color = GetColorFromText(paramPart[3], lineNum);
                            Vector3 boundingSize = new Vector3();
                            if (category == ObjectCategory.DYNAMIC)
                            {
                                boundingSize = GetVector3FromText(paramPart[4], lineNum);
                            }
                            obj = new Capsule(position, height, radius, color, boolean, booleanStrength, boundingSize, info:objectInfoName);
                            obj.SetTransparent(paramPart.Contains("transparent"));
                        }
                        break;
                    case "cylinder":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            Vector3 baseNormal = GetVector3FromText(paramPart[1], lineNum);
                            float height = float.Parse(paramPart[2]);
                            float radius = float.Parse(paramPart[3]);
                            Color color = GetColorFromText(paramPart[4], lineNum);
                            Vector3 boundingSize = new Vector3();
                            if (category == ObjectCategory.DYNAMIC)
                            {
                                boundingSize = GetVector3FromText(paramPart[5], lineNum);
                            }
                            obj = new Cylinder(position, baseNormal, height, radius, color, boolean, booleanStrength, boundingSize, info:objectInfoName);
                            obj.SetTransparent(paramPart.Contains("transparent"));
                        }
                        break;
                    case "plane":
                        {
                            if (category == ObjectCategory.DYNAMIC) throw new FormatException($"Format error line {lineNum} - Cannot add plane as dynamic object");

                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            Vector3 normal = GetVector3FromText(paramPart[1], lineNum);
                            Color color = GetColorFromText(paramPart[2], lineNum);

                            obj = new Plane(position, normal, color, boolean, booleanStrength, info:objectInfoName);
                            obj.SetTransparent(paramPart.Contains("transparent"));
                        }
                        break;
                    case "sphere":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            float size = float.Parse(paramPart[1]);
                            Color color = GetColorFromText(paramPart[2], lineNum);
                            Vector3 boundingSize = new Vector3();
                            if (category == ObjectCategory.DYNAMIC)
                            {
                                boundingSize = GetVector3FromText(paramPart[3], lineNum);
                            }
                            obj = new Sphere(position, size, color, boolean, booleanStrength, boundingSize, info:objectInfoName);
                            obj.SetTransparent(paramPart.Contains("transparent"));
                        }
                        break;

                    default:
                        throw new FormatException($"Format error line {lineNum} - Cannot add this type of object as static object");
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new FormatException($"Format error line {lineNum} - Missing some arguments while declaring object");
            }

            type = obj.GetType();
            return obj;

        }

        private PhysicsObject GetPhysicsObjectFromData(ObjectCategory category, int lineNum, string[] objectPart, string[] paramPart, out Type type)
        {
            PhysicsObject pObj;

            var objectType = objectPart[0];
            var objectInfoName = objectPart[1];

            try
            {
                if (objectType == "physics_object")
                {
                    Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                    Color color1 = GetColorFromText(paramPart[1], lineNum);
                    Color color2 = GetColorFromText(paramPart[2], lineNum);
                    pObj = new PhysicsObject(position, 25, color1, color2);
                }
                else if (objectType == "physics_trigger")
                {
                    Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                    float size = float.Parse(paramPart[1]);
                    pObj = new PhysicsTrigger(position, size);
                }
                else if (objectType == "mirror_ball")
                {
                    Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                    pObj = new MirrorBall(position, 25);
                }
                else
                {
                    throw new FormatException($"Format error line {lineNum} - Cannot add object type '{objectType}' into lights block)");
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new FormatException($"Format error line {lineNum} - Missing some arguments");
            }

            type = pObj.GetType();
            return pObj;

        }

        private Interactable GetInteractableObjectFromData(ObjectCategory category, int lineNum, string[] objectPart, string[] paramPart, out Type type)
        {
            Interactable iObj;

            var objectType = objectPart[0];
            var objectInfoName = objectPart[1];

            try
            {
                switch (objectType)
                {
                    case "button":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            Vector3 facing = GetVector3FromText(paramPart[1], lineNum);
                            Color color = GetColorFromText(paramPart[2], lineNum);
                            iObj = new Button(position, facing, color);
                        }
                        break;

                    case "floor_button":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            Color color = GetColorFromText(paramPart[1], lineNum);
                            iObj = new FloorButton(position, color);
                        }
                        break;

                    case "door":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            Vector3 facing = GetVector3FromText(paramPart[1], lineNum);
                            Color color = GetColorFromText(paramPart[3], lineNum);
                            int triggersNeeded = int.Parse(paramPart[4]);
                            iObj = new Door2(position, facing, declaredObjectsStatic[paramPart[2]], color, triggersNeeded);
                        }
                        break;

                    case "lifter":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            float maxHeight = float.Parse(paramPart[1]);
                            bool inverted = bool.Parse(paramPart[2]);
                            Color secondaryColor = GetColorFromText(paramPart[3], lineNum);
                            iObj = new Lifter(position, maxHeight, inverted, secondaryColor);
                        }
                        break;

                    case "jumper":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            Vector3 normal = GetVector3FromText(paramPart[1], lineNum);
                            Vector3 arrowDir = GetVector3FromText(paramPart[2], lineNum);
                            Vector3 jumperDirection = GetVector3FromText(paramPart[3], lineNum);
                            float jumperStrength = float.Parse(paramPart[4]);
                            Object ground = null;
                            if (declaredObjectsStatic.ContainsKey(paramPart[5]))
                            {
                                 ground = declaredObjectsStatic[paramPart[5]];
                            }
                            else if (declaredObjectsDynamic.ContainsKey(paramPart[5]))
                            {
                                 ground = declaredObjectsDynamic[paramPart[5]];
                            }

                            iObj = new Jumper(position, normal, arrowDir, jumperDirection, jumperStrength, ground);
                        }
                        break;

                    case "laser_spawner":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            Vector3 normal = GetVector3FromText(paramPart[1], lineNum);
                            Object ground = null;
                            if (declaredObjectsStatic.ContainsKey(paramPart[2]))
                            {
                                 ground = declaredObjectsStatic[paramPart[2]];
                            }
                            else if (declaredObjectsDynamic.ContainsKey(paramPart[2]))
                            {
                                 ground = declaredObjectsDynamic[paramPart[2]];
                            }
                            iObj = new LaserSpawner(position, normal, ground);
                        }
                        break;

                    case "laser_catcher":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            Vector3 normal = GetVector3FromText(paramPart[1], lineNum);
                            Object ground = null;
                            if (declaredObjectsStatic.ContainsKey(paramPart[2]))
                            {
                                 ground = declaredObjectsStatic[paramPart[2]];
                            }
                            else if (declaredObjectsDynamic.ContainsKey(paramPart[2]))
                            {
                                 ground = declaredObjectsDynamic[paramPart[2]];
                            }
                            Color color = GetColorFromText(paramPart[3], lineNum);
                            iObj = new LaserCatcher(position, normal, ground, color);
                        }
                        break;

                    case "portal_spawner":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            Vector3 facing = GetVector3FromText(paramPart[1], lineNum);
                            int portalType = int.Parse(paramPart[2]);
                            Color color = GetColorFromText(paramPart[3], lineNum);
                            bool startEnabled = bool.Parse(paramPart[4]);

                            iObj = new PortalSpawner(position, facing, portalType, color, startEnabled);
                        }
                        break;

                    case "ball_spawner":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            Color color = GetColorFromText(paramPart[1], lineNum);
                            PhysicsObject ball = declaredPhysicsObjects[paramPart[2]].Item1;
                            data.physicsMapObjects.Remove(ball);
                            int delay = int.Parse(paramPart[3]);

                            iObj = new BallSpawner(position, color, ball, delay);
                        }
                        break;
                        
                    default:
                        throw new FormatException($"Format error line {lineNum} - Cannot add this type of object as dynamic object");
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new FormatException($"Format error line {lineNum} - Missing some arguments");
            }

            type = iObj.GetType();
            return iObj;

        }

        private void AssignOperationFromData(ObjectCategory category, int lineNum, string operationType, string targetObject, string[] operationContent)
        {
            Object target = null;
            if (declaredObjectsStatic.ContainsKey(targetObject))
            {
                 target = declaredObjectsStatic[targetObject];
            }
            else if (declaredObjectsDynamic.ContainsKey(targetObject))
            {
                 target = declaredObjectsDynamic[targetObject];
            }
            else
            {
                throw new FormatException($"Format error line {lineNum} - {targetObject} target of operation not found");
            }

            try
            {
                switch (operationType)
                {
                    case "rotateX":
                        {
                            float angle = float.Parse(operationContent[0]);
                            Vector3 pivot = target.Position;
                            if (operationContent.Length > 1)
                            {
                                pivot = GetVector3FromText(operationContent[1], lineNum);
                            }
                            target.Rotate(angle, "x", pivot);
                        }
                        break;
                    case "rotateY":
                        {
                            float angle = float.Parse(operationContent[0]);
                            Vector3 pivot = target.Position;
                            if (operationContent.Length > 1)
                            {
                                pivot = GetVector3FromText(operationContent[1], lineNum);
                            }
                            target.Rotate(angle, "y", pivot);
                        }
                        break;
                    case "rotateZ":
                        {
                            float angle = float.Parse(operationContent[0]);
                            Vector3 pivot = target.Position;
                            if (operationContent.Length > 1)
                            {
                                pivot = GetVector3FromText(operationContent[1], lineNum);
                            }
                            target.Rotate(angle, "z", pivot);
                        }
                        break;
                    case "rotate":
                        {
                            float angle = float.Parse(operationContent[0]);
                            Vector3 axis = GetVector3FromText(operationContent[1], lineNum);
                            Vector3 pivot = target.Position;
                            if (operationContent.Length > 2)
                            {
                                pivot = GetVector3FromText(operationContent[2], lineNum);
                            }
                            target.Rotate(angle, axis, pivot);
                        }
                        break;
                    case "repeat":
                        {
                            Vector3 repetitionLimit = GetVector3FromText(operationContent[0], lineNum);
                            float repetitionDistance = float.Parse(operationContent[1]);

                            target.SetRepetition(repetitionLimit, repetitionDistance);
                        }
                        break;
                    case "symmetry":
                        {
                            string symmetryOptions = operationContent[0].ToUpper();
                            Vector3 symmetryPlaneOffset = GetVector3FromText(operationContent[1], lineNum);

                            target.SetSymmetry(symmetryOptions, symmetryPlaneOffset);
                        }
                        break;
                    case "boolean":
                        {
                            BooleanOP op = GetBooleanOPFromText(operationContent[0]);
                            float opStrength = 1;
                            float.TryParse(operationContent[1], out opStrength);

                            if (LineContainsObject(operationContent[2], lineNum, out string[] _objectPart, out string[] _paramPart))
                            {
                                bool relativeTransform = bool.Parse(_paramPart.Last());
                                _paramPart = _paramPart.SkipLast(1).ToArray();
                                Object opObj = GetObjectFromData(category, lineNum, _objectPart, _paramPart, op, opStrength, out Type type);

                                target.AddChildObject(opObj, relativeTransform);
                            }
                            else if (op == BooleanOP.UNION && declaredObjectsStatic.ContainsKey(operationContent[2].Split('|')[0])) // static and dynamic objects
                            {
                                _paramPart = operationContent[2].Split('|');
                                bool relativeTransform = bool.Parse(_paramPart.Last());
                                target.AddChildObject(declaredObjectsStatic[_paramPart[0]], relativeTransform);
                            }
                            else if (op == BooleanOP.UNION && declaredObjectsDynamic.ContainsKey(operationContent[2].Split('|')[0]))
                            {
                                _paramPart = operationContent[2].Split('|');
                                bool relativeTransform = bool.Parse(_paramPart.Last());
                                target.AddChildObject(declaredObjectsDynamic[_paramPart[0]], relativeTransform);
                            }
                            else
                            {
                                throw new FormatException($"Format error line {lineNum} - Boolean object declaration couldn not be parsed");
                            }

                        }
                        break;
                    default:
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                switch (operationType)
                {
                    case "rotateX":
                        throw new FormatException($"Format error line {lineNum} - rotateX operation missing arguments (correct: rotateX: object|angle|(pivot))");
                    case "rotateY":
                        throw new FormatException($"Format error line {lineNum} - rotateY operation missing arguments (correct: rotateY: object|angle|(pivot))");
                    case "rotateZ":
                        throw new FormatException($"Format error line {lineNum} - rotateZ operation missing arguments (correct: rotateZ: object|angle|(pivot))");
                    case "rotate":
                        throw new FormatException($"Format error line {lineNum} - rotate operation missing arguments (correct: rotate: object|angle|axis|(pivot))");
                    case "boolean":
                        throw new FormatException($"Format error line {lineNum} - boolean operation missing arguments (correct: boolean: object: operation([smooth_op_strenght]: operation_object_declaration:|..|..)");
                }
            }
        }

        private bool LineContainsObject(string line, int lineNum, out string[] objectPart, out string[] paramPart)
        {
            objectPart = new string[2];
            paramPart = new string[0];
            if (line.Contains(":"))
            {
                try
                {
                    var parts = line.Split(":", 2, StringSplitOptions.TrimEntries);
                    if (parts[0].Contains('[') && parts[0].Contains(']'))
                    {
                        // name part
                        objectPart[0] = parts[0].Split('[')[0];

                        // info name part
                        objectPart[1] = parts[0].Split('[', ']')[1];
                    }
                    else
                    {
                        // name part without info name
                        objectPart[0] = parts[0];
                    }

                    if (!possibleObjects.Contains(objectPart[0])) 
                        return false;

                    paramPart = parts[1].Split("|", StringSplitOptions.TrimEntries);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new FormatException($"Format error line {lineNum} - incorrect format for object (correct: object_type*[object_info_name]*:object_attribute|...|...)");
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool LineContainsOperation(string line, int lineNum, out string operationType, out string targetObject, out string[] operationContent) 
        {
            operationType = "";
            targetObject = "";
            operationContent = new string[0];

            if (line.Contains(":"))
            {
                try
                {
                    var parts = line.Split(":", 2, StringSplitOptions.TrimEntries);
                    if (possibleOperations.Contains(parts[0]))
                    {
                        operationType = parts[0];
                        operationContent = parts[1].Split('|', StringSplitOptions.TrimEntries);

                        if (operationType.StartsWith("rotate") || operationType.StartsWith("repeat") || operationType.StartsWith("symmetry")) // rotate: box: 10|(0,0,1)
                        {
                            var split = parts[1].Split(":", 2);
                            targetObject = split[0];
                            operationContent = split[1].Split('|', StringSplitOptions.TrimEntries);
                        }
                        else if (operationType == "boolean") // boolean: box: op[1.0]: plane:(,,)|(0,0,)
                        {
                            var objContentSplit = parts[1].Split(":", 3, StringSplitOptions.TrimEntries);
                            // box: op: plane: ...

                            string op = "";
                            string opStrength = "";
                            if (objContentSplit[1].StartsWith("smooth"))
                            {
                                var _opSetting = objContentSplit[1].Split('[', ']');
                                op = _opSetting[0];
                                opStrength = _opSetting[1];
                            }
                            else
                            {
                                op = objContentSplit[1];
                            }

                            targetObject = objContentSplit[0];
                            operationContent = new string[] {
                                op, 
                                opStrength,
                                objContentSplit[2]
                            };
                        }
                        else if (operationType == "connect")// connect: mButton -> inDoor
                        {
                            var objects = parts[1].Split("->", 2, StringSplitOptions.TrimEntries);
                            operationContent = objects;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    throw new FormatException($"Format error line {lineNum} - incorrect format for operation (correct: operation:...)");
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool LineContainsVariableDeclaration(string line, int lineNum, bool persistant) 
        {
            if (line.StartsWith("$"))
            {
                var split = line.Split(':',StringSplitOptions.TrimEntries);
                var name = split[0];
                var value = split[1];

                if (value.Contains("(") || value.Contains(")"))
                {
                    var vector = GetVector3FromText(value, lineNum);
                    if (persistant)
                        this.declaredVectorVarsPersistant.Add(name, vector);
                    else
                        this.declaredVectorVars.Add(name, vector);

                    return true;
                }
                else
                {
                    var color = GetColorFromText(value, lineNum);
                    if (persistant)
                        this.declaredColorVarsPersistant.Add(name, color);
                    else
                        this.declaredColorVars.Add(name, color);

                    return true;
                }
            }

            return false;
        }

        private bool LineContainsHelp(string line, int lineNum) 
        {
            if (line.StartsWith("help:"))
            {
                var info = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                string text = "";

                if (info == "config")
                {
                    text += $"Config block help found on line {lineNum}\n";
                    text += $"Changes to this block needs level restart";
                    text += $"\n";
                    text += $"  *'name: <map id>'              ... choose map id\n";
                    text += $"  *'player_spawn: <vector>'      ... choose where player spawns\n";
                    text += $"  *'bot_corner: <vector>' \n";
                    text += $"  *'top_corner: <vector>'        ... choose bot and top corner of distance map\n";
                    text += $"  'next_lvl: <next map id>'      ... choose next map id\n";
                    text += $"  'next_lvl_detail: <int>'       ... choose next map level detail\n";
                    text += $"  'next_lvl_door_color: <color>' ... choose next load level door color\n";
                    text += $"  'last_lvl_door_color: <color>' ... choose last load level door color\n";
                    text += $"  'game_lvl_order: <int>'        ... set map order for game mode\n";
                    text += $"  'game_lvl_name: <string>'      ... set map name for game mode\n";
                    text += $"  'game_lvl_inputs: <int>:'      ... set possible inputs for level (0/1/2)\n";
                }
                else if (info == "static" || info == "dynamic")
                {
                    if (info == "static")
                    {
                        text += $"Static block help found on line {lineNum}\n";
                        text += $"Changes to this block needs level restart and distance map rebuild\n";
                    }
                    else
                    {
                        text += $"Dynamic block help found on line {lineNum}\n";
                        text += $"Changes to this block can be done at runtime\n";
                    }
                    text += $"\n";
                    text += $"  '#comment'\n";
                    text += $"  '$<variable name>:<vector/color/bool> ... declare custom variable'\n";
                    text += $"  declare custom variable'\n";
                    text += $"  '<object>[<object info name>]: <object declaration>'\n";
                    text += $"Objects: box, box_frame, capsule, cylinder, plane, sphere\n";
                    text += $"\n";
                    text += $"  '<operation>: <operation declaration>'\n"; 
                    text += $"Operations: boolean, rotateX, rotateY, rotateZ, rotate\n";
                    text += $"            repeat, symmetry, connect\n";
                }
                else if (info == "lights")
                {
                    text += $"Lights block help found on line {lineNum}\n";
                    text += $"Changes to this block can be done at runtime\n";
                    text += $"\n";
                    text += $"  '#comment'\n";
                    text += $"  'light:<light declaration>'\n";
                }
                else if (info == "physics")
                {
                    text += $"Physics block help found on line {lineNum}\n";
                    text += $"Changes to this block can be done at runtime\n";
                    text += $"\n";
                    text += $"  '#comment'\n";
                    text += $"  '<physics object>[<object name>]:<object declaration>'\n";
                    text += $"Physics objects: physics_object, mirror_ball, physics_trigger\n";
                }
                else if (info == "interactables")
                {
                    text += $"Interactables block help found on line {lineNum}\n";
                    text += $"Changes to this block needs level restart and distance map rebuild\n";
                    text += $"\n";
                    text += $"  '#comment'\n";
                    text += $"  '<interactable object>[<object name>]:<object declaration>'\n";
                    text += $"Interactable objects: button, floor_button, door, lifter, jumper\n";
                    text += $"                      laser_spawner, laser_catcher\n";
                    text += $"                      portal_spawner, ball_spawner\n";
                    text += $"  'connect: <interactable ID - signal sender> -> <interactable ID singal receiver>\n'";
                }
                else if (info == "portalable")
                {
                    text += $"Portalable block help found on line {lineNum}\n";
                    text += $"Changes to this block needs level restart and distance map rebuild\n";
                    text += $"  '#comment'\n";
                    text += $"  '<object ID> ... set object to be able to receive portals'\n";
                }
                // object help
                else if (info == "box")
                {
                    text += $"Box object help found on line {lineNum}\n";
                    text += $"  'box[<object ID>]:<vector>|<vector>|<color name>|<vector>'\n";
                    text += $"                    position|size    |color       |bounding box'";
                }
                else if (info == "box_frame")
                {
                    text += $"Box_frame object help found on line {lineNum}\n";
                    text += $"  'box_frame[<object ID>]:<vector>|<vector>|<float>   |<color name>|<vector>'\n";
                    text += $"                          position|size    |frame size|color       |bounding box'";
                }
                else if (info == "capsule")
                {
                    text += $"Capsule object help found on line {lineNum}\n";
                    text += $"  'capsule[<object ID>]:<vector>|<float>|<float>|<color name>|<vector>'\n";
                    text += $"                        position|height |radius |color       |bounding box'";
                }
                else if (info == "cylinder")
                {
                    text += $"Cylinder object help found on line {lineNum}\n";
                    text += $"  'cylinder[<object ID>]:<vector>     |<vector>          |<float>|<float>|<color name>|<vector>'\n";
                    text += $"                         base position|base normal vector|height |radius |color       |bounding box'";
                }
                else if (info == "plane")
                {
                    text += $"Plane object help found on line {lineNum}\n";
                    text += $"*can be used only in static block or as part of boolean operation*";
                    text += $"  'plane[<object ID>]:<vector>|<vector>    |<color name>\n";
                    text += $"                      position|plane normal|color       ";
                }
                else if (info == "sphere")
                {
                    text += $"Sphere help found on line {lineNum}\n";
                    text += $"  'sphere[<object ID>]:<vector>|<float>|<color name>|<vector>'\n";
                    text += $"                       position|size   |color       |bounding box'";
                }
                else if (info == "physics_object")
                {
                    text += $"Physics_object help found on line {lineNum}\n";
                    text += $"  'physics_object[<object ID>]:<vector>|<color name>|<color name>'\n";
                    text += $"                               position|color1      |color2      '";
                }
                else if (info == "mirror_ball")
                {
                    text += $"Mirror_ball help found on line {lineNum}\n";
                    text += $"  'mirror_ball[<object ID>]:<vector>'\n";
                    text += $"                            position'";
                }
                else if (info == "physics_trigger")
                {
                    text += $"Trigger reacting to player and physics objects help found on line {lineNum}\n";
                    text += $"  'physics_trigger[<object ID>]:<vector>|<float>'\n";
                    text += $"                                position|trigger size'";
                }
                else if (info == "button")
                {
                    text += $"Button help found on line {lineNum}\n";
                    text += $"  'button[<object ID>]:<vector>|<vector>  |<color>'\n";
                    text += $"                       position|facing dir|color  '";
                }
                else if (info == "floor_button")
                {
                    text += $"Floor_button help found on line {lineNum}\n";
                    text += $"  'floor_button[<object ID>]:<vector>|<color>'\n";
                    text += $"                             position|color  '";
                }
                else if (info == "door")
                {
                    text += $"Door help found on line {lineNum}\n";
                    text += $"  'door[<object ID>]:<vector>|<vector>            |<object ID>   |<color>|<int>'\n";
                    text += $"                     position|facing dir (limited)|wall object ID|color  |connections needed for interaction'";
                }
                else if (info == "lifter")
                {
                    text += $"Lifter help found on line {lineNum}\n";
                    text += $"  'lifter[<object ID>]:<vector>|<float>   |<bool>       |<color>'\n";
                    text += $"                       position|max height|inverted mode|color  '";
                }
                else if (info == "jumper")
                {
                    text += $"Jumper help found on line {lineNum}\n";
                    text += $"  'jumper[<object ID>]:<vector>|<vector>|<vector> |<vector>|<float>      |<object ID>'\n";
                    text += $"                       position|normal  |arrow dir|jump dir|jump strength|ground object ID'";
                }
                else if (info == "laser_spawner")
                {
                    text += $"Laser_spawner help found on line {lineNum}\n";
                    text += $"  'laser_spawner[<object ID>]:<vector>|<vector>|<object ID>     |'\n";
                    text += $"                              position|normal  |ground object ID|'";
                }
                else if (info == "laser_catcher")
                {
                    text += $"Laser_catcher help found on line {lineNum}\n";
                    text += $"  'laser_catcher[<object ID>]:<vector>|<vector>|<object ID>     |<color>'\n";
                    text += $"                              position|normal  |ground object ID|color  '";
                }
                else if (info == "ball_spawner")
                {
                    text += $"Ball_spawner help found on line {lineNum}\n";
                    text += $"  'ball_spawner[<object ID>]:<vector>|<color>|<physics object ID>|<int>'\n";
                    text += $"                             position|color  |spawn ball ID      |ms delay from open to spawn'";
                }
                // operation info
                else if (info == "rotateX"||info == "rotateY"||info == "rotateZ")
                {
                    text += $"rotateX/Y/Z help found on line {lineNum}\n";
                    text += $"  'rotateX/Y/Z: <object ID>:      <float>|<vector>'\n";
                    text += $"                object to rotate: angle  |pivot pos\n";
                }
                else if (info == "rotate")
                {
                    text += $"rotate help found on line {lineNum}\n";
                    text += $"  'rotate: <object ID>:      <float>|<vector>|<vector>'\n";
                    text += $"           object to rotate: angle  |axis    |pivot pos\n";
                }
                else if (info == "repeat")
                {
                    text += $"repeat help found on line {lineNum}\n";
                    text += $"  'repeat: <object ID>:      <vector>        |<float>'\n";
                    text += $"           object to repeat: repetition limit|repetition distance";
                }
                else if (info == "symmetry")
                {
                    text += $"symmetry help found on line {lineNum}\n";
                    text += $"  'symmetry: <object ID>:         <symmetry axis options>  |<vector>'\n";
                    text += $"             object for symmetry: \"XYZ\" (any combination)|symmetry plane offset";
                }
                else if (info == "boolean")
                {
                    text += $"boolean help found on line {lineNum}\n";
                    text += $"Boolean operations: difference, intersect, union\n";
                    text += $"                    smooth_difference, smooth_intersect, smooth_union\n";
                    text += $"  'boolean: <object ID>: <boolean op>: <operation object decleration>'\n";
                    text += $"                 object: boolean op  : object";
                    text += $"\n";
                    text += $"  'boolean: <object ID>: <smooth_boolean op>[<float>]:         <operation object decleration>'\n";
                    text += $"                 object:   smooth boolean op[smooth strength]: object";
                }
                throw new FormatException(text);
            }
            else if (line.StartsWith("help"))
            {
                string text = "";
                text += $"Help found on line {lineNum}\n";
                text += $"Use 'help: <topic>' to choose more specific topic\n";
                text += $"\n";
                text += $"Code blocks: config, static, dynamic, lights, \n";
                text += $"             physics, interactables, portalable\n";
                text += $"Objects: box, box_frame, capsule, cylinder, plane, sphere\n";
                text += $"Lights: light\n";
                text += $"Physics objects: physics_object, mirror_ball, physics_trigger\n";
                text += $"Interactable objects: button, floor_button, door, lifter, jumper\n";
                text += $"                      laser_spawner, laser_catcher\n";
                text += $"                      portal_spawner, ball_spawner\n";
                text += $"\n";
                text += $"Operations: boolean, rotateX, rotateY, rotateZ, rotate\n";
                text += $"            repeat, symmetry, connect\n";
                text += $"\n";
                text += $"Variable declaration: '$<variable name>:<vector/color/bool>'\n";
                text += $"                       can be used instead of vector/color/bool in declaration\n";
                text += $"                       with the use of '$<variable name>'";

                throw new FormatException(text);
            }

            return false;
        }

        private Vector3 GetVector3FromText(string input, int lineNum)
        {
            if (input.Contains("$"))
            {
                if (declaredVectorVars.ContainsKey(input))
                {
                    return declaredVectorVars[input];
                }
                else if (declaredVectorVarsPersistant.ContainsKey(input))
                {
                    return declaredVectorVarsPersistant[input];
                }
                else
                {
                    throw new FormatException($"Format error line {lineNum} - Unknown variable {input}");
                }
            }
            else
            {
                input = input.Trim(new char[] {' ', '(', ')'});
                var values = input.Split(",", 3, StringSplitOptions.TrimEntries);
                try
                {
                    var vector = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                    return vector;
                }
                catch (FormatException)
                {
                    throw new FormatException($"Format error line {lineNum} - incorrect vector format (correct: (float,float,float))");;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new FormatException($"Format error line {lineNum} - vector needs 3 float values");
                }
            }
        }

        private Color GetColorFromText(string input, int lineNum)
        {
            if (input.Contains("$"))
            {
                if (declaredColorVars.ContainsKey(input))
                {
                    return declaredColorVars[input];
                }
                else if (declaredColorVarsPersistant.ContainsKey(input))
                {
                    return declaredColorVars[input];
                }
                else
                {
                    throw new FormatException($"Format error line {lineNum} - Unknown variable {input}");
                }
            }
            else
            {
                CColor cc = CColor.FromName(input);
                if (!cc.IsKnownColor) throw new FormatException($"Format error line {lineNum} - Unknown color {input}");
                return new Color(cc.R, cc.G, cc.B, cc.A);
            }
        }

        private BooleanOP GetBooleanOPFromText(string input)
        {
            switch (input)
            {
                case "difference":        return BooleanOP.DIFFERENCE;
                case "intersect":         return BooleanOP.INTERSECT;
                case "union":             return BooleanOP.UNION;
                case "smooth_difference": return BooleanOP.SDIFFERENCE;
                case "smooth_intersect":  return BooleanOP.SINTERSECT; 
                case "smooth_union":      return BooleanOP.SUNION;

                default:
                    return BooleanOP.NONE;
            }
        }
    }
}
