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

        Dictionary<string, Object> declaredObjects;
        Dictionary<string, Tuple<PhysicsObject, Type>> declaredPhysicsObjects;
        Dictionary<string, Tuple<Interactable, Type>> declaredInteractableObjects;

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

                "button",
                "floor_button",
                "door",
                "lifter"
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

            declaredObjects = new Dictionary<string, Object>();
            declaredPhysicsObjects = new Dictionary<string, Tuple<PhysicsObject, Type>>();
            declaredInteractableObjects = new Dictionary<string, Tuple<Interactable, Type>>();

            data = new MapData();
            data.isCompiled = true;
            data.path = path;

            await ParseConfigAsync(input);
            List<Task> tasks = new List<Task>();

            tasks.Add(ParseLightsAsync(input));
            tasks.Add(ParseDynamicAsync(input));
            tasks.Add(ParsePhysicsAsync(input));
            if (full)
            {
                // they contain static elements - needs full recompile
                tasks.Add(ParseStaticAsync(input));
                tasks.Add(ParseInteractableAsync(input));
            }

            try
            {
                await Task.WhenAll(tasks);

                if (!full)
                {
                    Informer.instance.AddInfo("map_compilation1", $"Compiling done", true);
                    Informer.instance.AddInfo("map_compilation2", "", true);
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
                allLines[i] = allLines[i].TrimStart();
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
            partsFound.Add("anchor_start",false);
            partsFound.Add("anchor_end",false);

            int lineNum;
            for (int i = start+1; i < end; i++)
            {
                /* Console.WriteLine($"config {i}"); */
                await Task.Yield();

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
                    partsFound["anchor_start"] = true;
                    var As = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.levelStartAnchor = GetVector3FromText(As, lineNum);
                }
                else if (line.StartsWith("anchor_end:"))
                {
                    partsFound["anchor_end"] = true;
                    var ae = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.levelEndAnchor = GetVector3FromText(ae, lineNum);
                }
                else    
                {
                    throw new FormatException($"Format warning line {lineNum} - Unknown config input - needs name, player_spawn, top_corner, bot_corner");
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
            if (!partsFound["anchor_start"])
            {
                throw new FormatException("config - missing 'anchor_start' attribute");
            }
            if (!partsFound["anchor_end"])
            {
                throw new FormatException("config - missing 'anchor_end' attribute");
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
                /* Console.WriteLine($"static {i}"); */
                await Task.Yield();
                lineNum = i+1;

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;

                if (LineContainsObject(line, lineNum, out objectPart, out paramPart))
                {
                    objectInfoName = objectPart[1];

                    Object obj = GetObjectFromData(ObjectCategory.STATIC, lineNum, objectPart, paramPart, BooleanOP.NONE, 0, out Type type);

                    try
                    {
                        declaredObjects.Add(objectInfoName, obj);
                    }
                    catch (ArgumentException)
                    {
                        throw new FormatException($"Format error line {lineNum} - multiple objects cannot share info name");
                    }
                    objectList.Add(obj);
                }
                else if (LineContainsOperation(line, lineNum, out string operationType, out string targetObject, out string[] operationContent))
                {
                    if (!declaredObjects.ContainsKey(targetObject)) throw new FormatException($"NullReference error line {lineNum} - object {targetObject} was not yet declared");

                    AssignOperationFromData(ObjectCategory.STATIC, lineNum, operationType, targetObject, operationContent);
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
            Tuple<int,int> startEnd = GetBlockStartEnd("dynamic", input, false);
            (int start, int end) = startEnd;

            List<Object> objectList = new List<Object>();

            string[] objectPart;
            string[] paramPart;

            string objectType;
            string objectInfoName;

            int lineNum;
            for (int i = start+1; i < end; i++)
            {
                /* Console.WriteLine($"dynamic {i}"); */
                await Task.Yield();
                lineNum = i+1;

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;

                if (LineContainsObject(line, lineNum, out objectPart, out paramPart))
                {
                    Object obj = GetObjectFromData(ObjectCategory.DYNAMIC, lineNum, objectPart, paramPart, BooleanOP.NONE, 0, out Type type);

                    objectType = objectPart[0];
                    objectInfoName = objectPart[1];

                    try
                    {
                        declaredObjects.Add(objectInfoName, obj);
                    }
                    catch (ArgumentException)
                    {
                        throw new FormatException($"Format error line {lineNum} - multiple objects cannot share info name");
                    }
                    
                    objectList.Add(obj);
                }
                else if (LineContainsOperation(line, lineNum, out string operationType, out string targetObject, out string[] operationContent))
                {
                    if (!declaredObjects.ContainsKey(targetObject)) throw new FormatException($"NullReference error line {lineNum} - object {targetObject} was not yet declared");

                    AssignOperationFromData(ObjectCategory.STATIC, lineNum, operationType, targetObject, operationContent);
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

                /* Console.WriteLine($"lights {i}"); */
                await Task.Yield();

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;

                if (LineContainsObject(line, lineNum, out objectPart, out paramPart))
                {
                    objectType = objectPart[0];

                    if (objectType == "light")
                    {
                        Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                        Color color = GetColorFromText(paramPart[1], lineNum);
                        float intensity = float.Parse(paramPart[2]);
                        Light light = new Light(position, color, intensity, data.botCorner, data.topCorner);
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
                /* Console.WriteLine($"physics {i}"); */
                await Task.Yield();
                lineNum = i+1;

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;

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
                await Task.Yield();
                lineNum = i+1;

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;

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

                            if (toType == typeof(Door))
                            {
                                var door = (toObj as Door);

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
                else
                {
                    throw new FormatException($"Format warning line {lineNum} - Unknown input");
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
                            iObj = new Door(position, facing, declaredObjects[paramPart[2]], color);
                        }
                        break;

                    case "lifter":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0], lineNum);
                            float maxHeight = float.Parse(paramPart[1]);
                            Color secondaryColor = GetColorFromText(paramPart[2], lineNum);
                            iObj = new Lifter(position, maxHeight, secondaryColor);
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
            Object target = declaredObjects[targetObject];

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
                            else if (op == BooleanOP.UNION && declaredObjects.ContainsKey(operationContent[2].Split('|')[0]))
                            {
                                _paramPart = operationContent[2].Split('|');
                                bool relativeTransform = bool.Parse(_paramPart.Last());
                                target.AddChildObject(declaredObjects[_paramPart[0]], relativeTransform);
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
                        if (operationType == "boolean") // boolean: box: op[1.0]: plane:(,,)|(0,0,)
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

        private Vector3 GetVector3FromText(string input, int lineNum)
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

        private Color GetColorFromText(string input, int lineNum)
        {
            CColor cc = CColor.FromName(input);
            if (!cc.IsKnownColor) throw new FormatException($"Format error line {lineNum} - Unknown color {input}");
            return new Color(cc.R, cc.G, cc.B, cc.A);
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
