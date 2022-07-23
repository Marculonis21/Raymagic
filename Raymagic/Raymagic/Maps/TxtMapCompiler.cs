using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using CColor = System.Drawing.Color;

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
                "door"
            };

            possibleOperations = new List<string>() {
                "rotateX",
                "rotateY",
                "rotateZ",
                "rotate",
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

            /* var tasks = new Task[6]; */
            List<Task> tasks = new List<Task>();

            tasks.Add(ParseConfigAsync(input));
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
            }
            catch (FormatException e)
            {
                Console.WriteLine($"Error while compiling {path}");
                Console.WriteLine(e.Message);
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

            for (int i = start+1; i < end; i++)
            {
                /* Console.WriteLine($"config {i}"); */
                await Task.Yield();

                var line = input[i];

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
                    data.playerSpawn = GetVector3FromText(spawn);
                }
                else if (line.StartsWith("top_corner:"))
                {
                    partsFound["top_corner"] = true;
                    var tc = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.topCorner = GetVector3FromText(tc);
                }
                else if (line.StartsWith("bot_corner:"))
                {
                    partsFound["bot_corner"] = true;
                    var bc = line.Split(":", 2, StringSplitOptions.TrimEntries)[1];
                    data.botCorner = GetVector3FromText(bc);
                }
                else    
                {
                    throw new FormatException($"Format warning line {i+1} - Unknown input");
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

            for (int i = start+1; i < end; i++)
            {
                /* Console.WriteLine($"static {i}"); */
                await Task.Yield();

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;

                if (LineContainsObject(line, i, out objectPart, out paramPart))
                {
                    objectInfoName = objectPart[1];

                    Object obj = GetObjectFromData(ObjectCategory.STATIC, i, objectPart, paramPart, BooleanOP.NONE, 0, out Type type);

                    try
                    {
                        declaredObjects.Add(objectInfoName, obj);
                    }
                    catch (ArgumentException)
                    {
                        throw new FormatException($"Format error line {i+1} - multiple objects cannot share info name");
                    }
                    objectList.Add(obj);
                }
                else if (LineContainsOperation(line, i, out string operationType, out string targetObject, out string[] operationContent))
                {
                    if (!declaredObjects.ContainsKey(targetObject)) throw new NullReferenceException($"NullReference error line {i+1} - object {targetObject} was not yet declared");

                    AssignOperationFromData(ObjectCategory.STATIC, i, operationType, targetObject, operationContent);
                }
                else    
                {
                    throw new FormatException($"Format warning line {i+1} - Unknown input");
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

            for (int i = start+1; i < end; i++)
            {
                /* Console.WriteLine($"dynamic {i}"); */
                await Task.Yield();

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;

                if (LineContainsObject(line, i, out objectPart, out paramPart))
                {
                    Object obj = GetObjectFromData(ObjectCategory.DYNAMIC, i, objectPart, paramPart, BooleanOP.NONE, 0, out Type type);

                    objectType = objectPart[0];
                    objectInfoName = objectPart[1];

                    try
                    {
                        declaredObjects.Add(objectInfoName, obj);
                    }
                    catch (ArgumentException)
                    {
                        throw new FormatException($"Format error line {i+1} - multiple objects cannot share info name");
                    }

                    objectList.Add(obj);
                }
                else if (LineContainsOperation(line, i, out string operationType, out string targetObject, out string[] operationContent))
                {
                    if (!declaredObjects.ContainsKey(targetObject)) throw new NullReferenceException($"NullReference error line {i+1} - object {targetObject} was not yet declared");

                    AssignOperationFromData(ObjectCategory.DYNAMIC, i, operationType, targetObject, operationContent);
                }
                else    
                {
                    throw new FormatException($"Format warning line {i+1} - Unknown input");
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

            for (int i = start+1; i < end; i++)
            {
                /* Console.WriteLine($"lights {i}"); */
                await Task.Yield();

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;

                if (LineContainsObject(line, i, out objectPart, out paramPart))
                {
                    objectType = objectPart[0];

                    if (objectType == "light")
                    {
                        Vector3 position = GetVector3FromText(paramPart[0]);
                        float intensity = float.Parse(paramPart[1]);
                        Light light = new Light(position, intensity);
                        data.mapLights.Add(light);
                    }
                    else
                    {
                        throw new FormatException($"Format error line {i+1} - Cannot add object type '{objectType}' into lights block)");
                    }
                }
                else
                {
                    throw new FormatException($"Format warning line {i+1} - Unknown input");
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

            for (int i = start+1; i < end; i++)
            {
                /* Console.WriteLine($"physics {i}"); */
                await Task.Yield();

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;

                if (LineContainsObject(line, i, out objectPart, out paramPart))
                {
                    objectType = objectPart[0];
                    objectInfoName = objectPart[1];

                    PhysicsObject pObj = GetPhysicsObjectFromData(ObjectCategory.PHYSICS, i, objectPart, paramPart, out Type type);

                    try
                    {
                        declaredPhysicsObjects.Add(objectInfoName, new Tuple<PhysicsObject, Type>(pObj, type));
                    }
                    catch (ArgumentException)
                    {
                        throw new FormatException($"Format error line {i+1} - multiple objects cannot share info name");
                    }
                    data.physicsMapObjects.Add(pObj);
                }
                else
                {
                    throw new FormatException($"Format warning line {i+1} - Unknown input");
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

            for (int i = start+1; i < end; i++)
            {
                await Task.Yield();

                var line = input[i];

                if (line.StartsWith("#")) continue;
                if (line == "") continue;

                if (LineContainsObject(line, i, out objectPart, out paramPart))
                {
                    objectType = objectPart[0];
                    objectInfoName = objectPart[1];

                    Interactable iObj = GetInteractableObjectFromData(ObjectCategory.INTERACTABLE, i, objectPart, paramPart, out Type type);
                        
                    try
                    {
                        declaredInteractableObjects.Add(objectInfoName, new Tuple<Interactable, Type>(iObj, type));
                    }
                    catch (ArgumentException)
                    {
                        throw new FormatException($"Format error line {i+1} - multiple objects cannot share info name");
                    }
                    data.interactableObjectList.Add(iObj);
                }
                else if (LineContainsOperation(line, i, out string operationType, out string _, out string[] operationContent))
                {
                    if (operationType != "connect") throw new FormatException($"Format error line {i+1} - Interactable object allow only 'connect' operation");

                    string from = operationContent[0];
                    string to = operationContent[1];

                    // connect: from -> to 
                    // input can be interactable or trigger - output must be interactabl
                    if (!declaredInteractableObjects.ContainsKey(from) && !declaredPhysicsObjects.ContainsKey(from)) 
                        throw new FormatException($"Format error line {i+1} - 'From' connection object not found");

                    if (!declaredInteractableObjects.ContainsKey(to))
                        throw new FormatException($"Format error line {i+1} - 'To' connection object not found");

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
                            /* else if (toType == typeof(Spawner)) */
                            /* { */
                            /* } */
                        }
                        else
                        {
                            throw new FormatException($"Format error line {i+1} - Cannot use physics ball as an event trigger");
                        }
                        
                    }
                }
                else
                {
                    throw new FormatException($"Format warning line {i+1} - Unknown input");
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
            lineNum += 1;

            Object obj;

            var objectType = objectPart[0];
            var objectInfoName = objectPart[1];

            try
            {
                switch (objectType)
                {
                    case "box":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0]);
                            Vector3 size = GetVector3FromText(paramPart[1]);
                            Color color = GetColorFromText(paramPart[2]);
                            Vector3 boundingSize = new Vector3();
                            if (category == ObjectCategory.DYNAMIC)
                            {
                                boundingSize = GetVector3FromText(paramPart[3]);
                            }
                            obj = new Box(position, size, color, boolean, booleanStrength, boundingSize, info:objectInfoName);
                        }
                        break;
                    case "box_frame":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0]);
                            Vector3 size = GetVector3FromText(paramPart[1]);
                            float frameSize = float.Parse(paramPart[2]);
                            Color color = GetColorFromText(paramPart[3]);
                            Vector3 boundingSize = new Vector3();
                            if (category == ObjectCategory.DYNAMIC)
                            {
                                boundingSize = GetVector3FromText(paramPart[4]);
                            }
                            obj = new BoxFrame(position, size, frameSize, color, boolean, booleanStrength, boundingSize, info:objectInfoName);
                        }
                        break;
                    case "capsule":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0]);
                            float height = float.Parse(paramPart[1]);
                            float radius = float.Parse(paramPart[2]);
                            Color color = GetColorFromText(paramPart[3]);
                            Vector3 boundingSize = new Vector3();
                            if (category == ObjectCategory.DYNAMIC)
                            {
                                boundingSize = GetVector3FromText(paramPart[4]);
                            }
                            obj = new Capsule(position, height, radius, color, boolean, booleanStrength, boundingSize, info:objectInfoName);
                        }
                        break;
                    case "cylinder":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0]);
                            Vector3 baseNormal = GetVector3FromText(paramPart[1]);
                            float height = float.Parse(paramPart[2]);
                            float radius = float.Parse(paramPart[3]);
                            Color color = GetColorFromText(paramPart[4]);
                            Vector3 boundingSize = new Vector3();
                            if (category == ObjectCategory.DYNAMIC)
                            {
                                boundingSize = GetVector3FromText(paramPart[5]);
                            }
                            obj = new Cylinder(position, baseNormal, height, radius, color, boolean, booleanStrength, boundingSize, info:objectInfoName);
                        }
                        break;
                    case "plane":
                        {
                            if (category == ObjectCategory.DYNAMIC) throw new FormatException($"Format error line {lineNum} - Cannot add plane as dynamic object");

                            Vector3 position = GetVector3FromText(paramPart[0]);
                            Vector3 normal = GetVector3FromText(paramPart[1]);
                            Color color = GetColorFromText(paramPart[2]);

                            obj = new Plane(position, normal, color, boolean, booleanStrength, info:objectInfoName);
                        }
                        break;
                    case "sphere":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0]);
                            float size = float.Parse(paramPart[1]);
                            Color color = GetColorFromText(paramPart[2]);
                            Vector3 boundingSize = new Vector3();
                            if (category == ObjectCategory.DYNAMIC)
                            {
                                boundingSize = GetVector3FromText(paramPart[3]);
                            }
                            obj = new Sphere(position, size, color, boolean, booleanStrength, boundingSize, info:objectInfoName);
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
            lineNum += 1;

            PhysicsObject pObj;

            var objectType = objectPart[0];
            var objectInfoName = objectPart[1];

            try
            {
                if (objectType == "physics_object")
                {
                    Vector3 position = GetVector3FromText(paramPart[0]);
                    Color color1 = GetColorFromText(paramPart[1]);
                    Color color2 = GetColorFromText(paramPart[2]);
                    pObj = new PhysicsObject(position, 25, color1, color2);
                }
                else if (objectType == "physics_trigger")
                {
                    Vector3 position = GetVector3FromText(paramPart[0]);
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
            lineNum += 1;

            Interactable iObj;

            var objectType = objectPart[0];
            var objectInfoName = objectPart[1];

            try
            {
                switch (objectType)
                {
                    case "button":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0]);
                            Vector3 facing = GetVector3FromText(paramPart[1]);
                            Color color = GetColorFromText(paramPart[2]);
                            iObj = new Button(position, facing, color);
                        }
                        break;

                    case "floor_button":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0]);
                            Color color = GetColorFromText(paramPart[1]);
                            iObj = new FloorButton(position, color);
                        }
                        break;

                    case "door":
                        {
                            Vector3 position = GetVector3FromText(paramPart[0]);
                            Vector3 facing = GetVector3FromText(paramPart[1]);
                            Color color = GetColorFromText(paramPart[2]);
                            iObj = new Door(position, facing, color);
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
                            target.Rotate(angle, "x");
                        }
                        break;
                    case "rotateY":
                        {
                            float angle = float.Parse(operationContent[0]);
                            target.Rotate(angle, "y");
                        }
                        break;
                    case "rotateZ":
                        {
                            float angle = float.Parse(operationContent[0]);
                            target.Rotate(angle, "z");
                        }
                        break;
                    case "rotate":
                        {
                            float angle = float.Parse(operationContent[0]);
                            Vector3 axis = GetVector3FromText(operationContent[1]);
                            target.Rotate(angle, axis);
                        }
                        break;
                    case "boolean":
                        {
                            BooleanOP op = GetBooleanOPFromText(operationContent[0]);
                            float opStrength = 1;
                            float.TryParse(operationContent[1], out opStrength);

                            if (LineContainsObject(operationContent[2], lineNum, out string[] _objectPart, out string[] _paramPart))
                            {
                                Object opObj = GetObjectFromData(category, lineNum, _objectPart, _paramPart, op, opStrength, out Type type);

                                target.AddChildObject(opObj, true);
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
            catch (IndexOutOfRangeException e)
            {
                throw new FormatException($"Format error line {lineNum} - Missing arguments");
            }
        }

        private bool LineContainsObject(string line, int lineNum, out string[] objectPart, out string[] paramPart)
        {
            lineNum += 1;

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
            lineNum += 1;
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

                        if (operationType.StartsWith("rotate")) // rotate: box: 10|(0,0,1)
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
                                
                                var _opSetting = objContentSplit[1].Split('[');
                                op = _opSetting[0];
                                opStrength = _opSetting[1].Remove(']');
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

        private Vector3 GetVector3FromText(string input)
        {
            input = input.Trim(new char[] {' ', '(', ')'});
            var values = input.Split(",", 3, StringSplitOptions.TrimEntries);
            return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        }

        private Color GetColorFromText(string input)
        {
            CColor cc = CColor.FromName(input);
            if (!cc.IsKnownColor) throw new FormatException($"Unknown color {input}");
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
