using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class ConsoleMenu
    {
        Map map = Map.instance;

        private ConsoleMenu() {} 

        public static readonly ConsoleMenu instance = new ConsoleMenu();

        public void DisplayMenu()
        {
            Console.CursorVisible = false;
            while (true)
            {
                int startOutput = StartMenu();
                int innerOutput = 0;
                switch (startOutput)
                {
                    case 0:
                        innerOutput = GameModeMenu();
                        break;
                    case 1:
                        innerOutput = SandBoxMenu();
                        break;
                    case 2:
                        ControlsMenu();
                        break;
                    case 3:
                        AboutMenu();
                        break;
                }

                if (innerOutput == 1)
                {
                    break;
                }
            }
            Console.Clear();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.CursorVisible = true;
        }

        int startMenuPosition = 0;
        int StartMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.CursorTop = 1;
                Console.CursorLeft = (Console.WindowWidth/2) - 15;
                Console.WriteLine("-------------------------------");
                Console.CursorLeft = (Console.WindowWidth/2) - 15;
                Console.WriteLine("| --- WELCOME TO RAYMAGIC --- |");
                Console.CursorLeft = (Console.WindowWidth/2) - 15;
                Console.WriteLine("-------------------------------");

                string[] texts = new string[4] {"    GAME MODE     ", "   SANDBOX MODE   ", "     CONTROLS     ", "      ABOUT       "};
                for (int i = 0; i < 4; i++)
                {
                    if (i < 2)
                    {
                        Console.CursorTop = 8;
                    }
                    else
                    {
                        Console.CursorTop = 12;
                    }

                    int startLeft;
                    if (i == 0 || i == 2)
                    {
                        startLeft = (Console.WindowWidth/2) - (Console.WindowWidth/3);
                    }
                    else
                    {
                        startLeft = (Console.WindowWidth/2) + (Console.WindowWidth/3)-20;
                    }

                    if (i == startMenuPosition)
                    {
                        Console.CursorLeft = startLeft;
                        Console.WriteLine("####################");
                        Console.CursorLeft = startLeft;
                        Console.WriteLine($"#{texts[i]}#");
                        Console.CursorLeft = startLeft;
                        Console.WriteLine("####################");
                    }
                    else
                    {
                        Console.CursorLeft = startLeft;
                        Console.WriteLine("--------------------");
                        Console.CursorLeft = startLeft;
                        Console.WriteLine($"|{texts[i]}|");
                        Console.CursorLeft = startLeft;
                        Console.WriteLine("--------------------");
                    }


                }

                /* Console.WriteLine(Console.ReadKey().Key == ConsoleKey.DownArrow); */
                var pressed = Console.ReadKey().Key;
                if (pressed == ConsoleKey.UpArrow || pressed == ConsoleKey.W)
                {
                    startMenuPosition -= 2;
                    if (startMenuPosition < 0)
                    {
                        startMenuPosition += 4;
                    }
                }
                if (pressed == ConsoleKey.DownArrow || pressed == ConsoleKey.S)
                {
                    startMenuPosition = (startMenuPosition + 2) % 4;
                }
                if (pressed == ConsoleKey.LeftArrow || pressed == ConsoleKey.A)
                {
                    startMenuPosition -= 1;
                    if (startMenuPosition < 0)
                    {
                        startMenuPosition += 4;
                    }
                }
                if (pressed == ConsoleKey.RightArrow || pressed == ConsoleKey.D)
                {
                    startMenuPosition = (startMenuPosition + 1) % 4;
                }
                if (pressed == ConsoleKey.Enter)
                {
                    return startMenuPosition;
                }
            }
            
            return 0;
        }

        void ControlsMenu()
        {
            int controllsMenuPosition = 0;

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.CursorTop = 1;
                Console.CursorLeft = (Console.WindowWidth/2) - 15;
                Console.WriteLine("-------------------------------");
                Console.CursorLeft = (Console.WindowWidth/2) - 15;
                Console.WriteLine("| ---       CONTROLS      --- |");
                Console.CursorLeft = (Console.WindowWidth/2) - 15;
                Console.WriteLine("-------------------------------");

                string[] lines = new string[] {
                    "Movement:",
                    "  W",
                    "  S",
                    "  A",
                    "  D",
                    "  SPACE",
                    "",
                    "Aim:",
                    "  MOUSE",
                    "  LEFT MOUSE BUTTON",
                    "  RIGHT MOUSE BUTTON",
                    "",
                    "Interaction:",
                    "  E",
                    "  F",
                };

                string[] info = new string[] {
                    "",
                    "walk forward",
                    "walk backward",
                    "walk left",
                    "walk right",
                    "jump",
                    "",
                    "",
                    "aiming",
                    "orange portal (when enabled)",
                    "blue portal (when enabled)",
                    "",
                    "",
                    "interact",
                    "pick up",
                };

                Console.CursorTop = 5;

                for (int i = 0; i < lines.Length; i++)
                {
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkYellow;

                    if (i == controllsMenuPosition)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                    Console.CursorLeft = (Console.WindowWidth/2) - 25;
                    Console.Write(lines[i]);
                    Console.CursorLeft = (Console.WindowWidth/2) - 1;
                    if (lines[i] != "" && lines[i] != "Interaction:" && lines[i] != "Movement:" && lines[i] != "Aim:")
                    {
                        Console.Write("-->");
                    }
                    Console.CursorLeft = (Console.WindowWidth/2) + 4;
                    Console.Write(info[i]);
                    Console.Write("\n");
                }

                var pressed = Console.ReadKey().Key;
                if (pressed == ConsoleKey.UpArrow || pressed == ConsoleKey.W)
                {
                    controllsMenuPosition = (controllsMenuPosition - 1);
                    if (controllsMenuPosition < 0)
                    {
                        controllsMenuPosition = lines.Length-1;
                    }
                }
                else if (pressed == ConsoleKey.DownArrow || pressed == ConsoleKey.S)
                {
                    controllsMenuPosition = (controllsMenuPosition + 1) % (lines.Length);
                }
                else if (pressed == ConsoleKey.Escape)
                {
                    return;
                }
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            }

        }

        void AboutMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.CursorTop = 1;
            Console.CursorLeft = (Console.WindowWidth/2) - 15;
            Console.WriteLine("-------------------------------");
            Console.CursorLeft = (Console.WindowWidth/2) - 15;
            Console.WriteLine("| ---   ABOUT RAYMAGIC    --- |");
            Console.CursorLeft = (Console.WindowWidth/2) - 15;
            Console.WriteLine("-------------------------------");

            Console.CursorTop = 5;
            Console.ForegroundColor = ConsoleColor.White;
            var text = new string[] {
                "Raymagic began as a small side project in september 2021,",
                "to learn about raymarching algorithm.",
                "",
                "In the end it grew to the size of this project,",
                "much further than anticipated.",
                "",
                "Final version is published as semestral project for subjects",
                "NPRG035 and NPRG038 (Programming in C#) at MFF UK.",
                "",
                "Author: Marek Bečvář - MFF UK 2021/2022",
            };

            for (int i = 0; i < text.Length; i++)
            {
                Console.CursorLeft = (Console.WindowWidth/2) - text[i].Length/2;
                Console.WriteLine(text[i]);
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.ReadKey();
        }

        int GameModeMenu()
        {
            int mapSelectPosition = 0;

            List<Tuple<int, string>> mapList = new List<Tuple<int, string>>();

            // get all maps for game and sort them to the correct order - gameMapOrder
            foreach (var key in map.maps.Keys)
            {
                var _map = map.maps[key]; 
                if (_map.gameLevelOrder != -1)
                {
                    mapList.Add(new Tuple<int, string>(_map.gameLevelOrder, key));
                }
            }
            mapList.Sort(); 

            while (true)
            {
                Console.Clear();
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.CursorTop = 1;
                Console.CursorLeft = (Console.WindowWidth/2) - 15;
                Console.WriteLine("-------------------------------");
                Console.CursorLeft = (Console.WindowWidth/2) - 15;
                Console.WriteLine("| ---      GAME MODE      --- |");
                Console.CursorLeft = (Console.WindowWidth/2) - 15;
                Console.WriteLine("-------------------------------");

                Console.CursorTop = 6;
                Console.CursorLeft = 10;
                Console.WriteLine("SELECT START LEVEL:");
                Console.CursorLeft = 10;
                Console.WriteLine("###########");


                int lineSize = 5;
                int x = 0;
                int y = 0;
                for (int i = 0; i < mapList.Count; i++)
                {
                    Console.CursorTop = 10 + 12*y;
                    string s = (i+1).ToString("D2");
                    if (mapSelectPosition == i)
                    {
                        Console.CursorLeft = (Console.WindowWidth/2) - 27 + 12*x;
                        Console.WriteLine($"########");
                        Console.CursorLeft = (Console.WindowWidth/2) - 27 + 12*x;
                        Console.WriteLine($"#      #");
                        Console.CursorLeft = (Console.WindowWidth/2) - 27 + 12*x;
                        Console.WriteLine($"#  {s}  #");
                        Console.CursorLeft = (Console.WindowWidth/2) - 27 + 12*x;
                        Console.WriteLine($"#      #");
                        Console.CursorLeft = (Console.WindowWidth/2) - 27 + 12*x;
                        Console.WriteLine($"########");
                    }
                    else
                    {
                        Console.CursorLeft = (Console.WindowWidth/2) - 27 + 12*x;
                        Console.WriteLine($"--------");
                        Console.CursorLeft = (Console.WindowWidth/2) - 27 + 12*x;
                        Console.WriteLine($"|      |");
                        Console.CursorLeft = (Console.WindowWidth/2) - 27 + 12*x;
                        Console.WriteLine($"|  {s}  |");
                        Console.CursorLeft = (Console.WindowWidth/2) - 27 + 12*x;
                        Console.WriteLine($"|      |");
                        Console.CursorLeft = (Console.WindowWidth/2) - 27 + 12*x;
                        Console.WriteLine($"--------");
                    }

                    x++;
                    if (x == lineSize)
                    {
                        x = 0;
                        y++;
                    }
                }

                string mapName = map.maps[mapList[mapSelectPosition].Item2].gameLevelName;
                Console.CursorTop = Console.WindowHeight - 8;
                Console.CursorLeft = (Console.WindowWidth/2) - (int)(mapName.Length/2);
                Console.WriteLine(mapName);

                var pressed = Console.ReadKey().Key;
                if (pressed == ConsoleKey.LeftArrow|| pressed == ConsoleKey.A)
                {
                    mapSelectPosition = (mapSelectPosition - 1);
                    if (mapSelectPosition < 0)
                    {
                        mapSelectPosition += mapList.Count;
                    }
                }
                else if (pressed == ConsoleKey.RightArrow || pressed == ConsoleKey.D)
                {
                    mapSelectPosition = (mapSelectPosition + 1) % (mapList.Count);
                }
                else if (pressed == ConsoleKey.Enter)
                {
                    map.SetMap(mapList[mapSelectPosition].Item2, gameMode:true);
                    return 1;
                }
                else if (pressed == ConsoleKey.Escape)
                {
                    return 0;
                }
            }
        }

        int SandBoxMenu()
        {
            int stage = 0;
            int mapSelectPosition = 0;

            while (true)
            {
                Console.Clear();
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.CursorTop = 1;
                Console.CursorLeft = (Console.WindowWidth/2) - 15;
                Console.WriteLine("-------------------------------");
                Console.CursorLeft = (Console.WindowWidth/2) - 15;
                Console.WriteLine("| ---        SANDBOX       --- |");
                Console.CursorLeft = (Console.WindowWidth/2) - 15;
                Console.WriteLine("-------------------------------");

                Console.CursorTop = 5;
                Console.CursorLeft = (Console.WindowWidth/2) - 20;
                Console.WriteLine("MAP SELECT:");

                int i = 0;
                foreach(string key in map.maps.Keys)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;

                    if (i == mapSelectPosition)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                    Console.CursorLeft = (Console.WindowWidth/2) - 17;
                    Console.WriteLine($"{i+1}: {key}");
                    i++;

                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }
                
                if (stage == 0)
                {
                    var pressed = Console.ReadKey().Key;
                    if (pressed == ConsoleKey.UpArrow || pressed == ConsoleKey.W)
                    {
                        mapSelectPosition = (mapSelectPosition - 1);
                        if (mapSelectPosition < 0)
                        {
                            mapSelectPosition = map.maps.Keys.Count-1;
                        }
                    }
                    else if (pressed == ConsoleKey.DownArrow || pressed == ConsoleKey.S)
                    {
                        mapSelectPosition = (mapSelectPosition + 1) % (map.maps.Keys.Count);
                    }
                    else if (pressed == ConsoleKey.Enter)
                    {
                        stage++;
                        continue;
                    }
                    else if (pressed == ConsoleKey.Escape)
                    {
                        return 0;
                    }
                }
                if (stage == 1)
                {
                    Console.Clear();
                    Console.WriteLine($"MAP SELECTED: {map.maps.Keys.ElementAt(mapSelectPosition)}");
                    map.SetMap(map.maps.Keys.ElementAt(mapSelectPosition));
                    return 1;
                }
            }
        }
    }
}
