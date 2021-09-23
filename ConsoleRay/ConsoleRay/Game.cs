﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ConsoleRay
{
    public class Game
    {
        GameTime gameTime;
        KeyboardController controller;

        Player player;

        int winWidth = 35;
        int winHeight = 20;
        float detailSize = 0.5f; 

        int zoom = 25;

        char[] lumimanceValue = new char[] {'.',',','-','~',':',';','=','!','*','#','$','@'};

        List<IObject> objectList = new List<IObject>();
        List<Light> lightList = new List<Light>();

        Stopwatch gameWatch = new Stopwatch();
        public Game()
        {
            Console.CursorVisible = true;
            Console.Clear();

            gameTime = new GameTime(60);
            controller = new KeyboardController();

            player = Player.instance;

            Prep();

            gameWatch.Start();
            Run();
        }

        private void Prep()
        {
            Sphere sphere = new Sphere(player.position + player.lookDir*300,
                                      0,
                                      Color.White,
                                      false);

            sphere.AddBoolean(BooleanOP.UNION, new Sphere(new Vector3(30,30,0),
                                                          50,
                                                          Color.White,
                                                          false));
            sphere.AddBoolean(BooleanOP.UNION, new Sphere(new Vector3(-40,-20,-20),
                                                          30,
                                                          Color.White,
                                                          false));

            sphere.AddBoolean(BooleanOP.UNION, new Sphere(new Vector3(-50,-40,40),
                                                          20,
                                                          Color.White,
                                                          false));

            objectList.Add(sphere);

            lightList.Add(new Light(player.position + player.lookDir*200 + new Vector3(0,300,300),
                                    500));
        }

        void Run()
        {
            gameTime.Start();
            while (true)
            {
                gameTime.Update();
                if (gameTime.GameUpdate()) //time is right 
                    Update();

                if (controller.IsEndGame())
                    break;
            }

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("=== END ===");
        }

        private void Update()
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(0,0);

            /* float z = (float)Math.Sin(gameWatch.Elapsed.TotalMilliseconds / 1000f); */
            /* objectList[0].Translate(new Vector3(0,0,z)); */

            objectList[0].Rotate(5f,"z");


            Vector3 playerLookDir = player.lookDir;

            // Get one of player look dir perpendicular vectors (UP)
            double R_inclinPerpen = (player.rotation.Y+90)*Math.PI/180f;
            double R_azimPerpen = (player.rotation.X)*Math.PI/180f;
            double _x = Math.Cos(R_azimPerpen)*Math.Sin(R_inclinPerpen); 
            double _y = Math.Sin(R_azimPerpen)*Math.Sin(R_inclinPerpen); 
            double _z = Math.Cos(R_inclinPerpen);
            Vector3 playerLookPerpenUP = new Vector3((float)_x,(float)_y,(float)_z);

            // Cross product look dir with perpen to get normal of a plane ->
            // side perpen to the view dir
            Vector3 playerLookPerpenSIDE = Vector3.Cross(playerLookDir, playerLookPerpenUP);

            playerLookPerpenUP.Normalize();
            playerLookPerpenSIDE.Normalize();


            Color[,] colors = new Color[(int)(winWidth/detailSize),(int)(winHeight/detailSize)];
            Parallel.For(0, (int)(winHeight/detailSize) * (int)(winWidth/detailSize),new ParallelOptions{ MaxDegreeOfParallelism = Environment.ProcessorCount}, i =>
            { 
            /* for(int y = 0; y < winHeight/detailSize; y++) */
            /* { */
            /*     for(int x = 0; x < winWidth/detailSize; x++) */
            /*     { */
                int y = i / (int)(winWidth/detailSize);
                int x = i % (int)(winWidth/detailSize);

                // get ray dir from camera through view plane (detailSize) "rectangles"
                float X = x - (winWidth /detailSize)/2 + detailSize/2;
                float Y = y - (winHeight/detailSize)/2 + detailSize/2;

                Vector3 rayDir = (player.position + playerLookDir*zoom + playerLookPerpenSIDE*X*detailSize + playerLookPerpenUP*Y*detailSize) - player.position;

                if(RayMarch(player.position, rayDir, out float length, out Color color))
                {
                    colors[x,y] = color;
                }
            });

            Console.CursorVisible = false;
            for(int y = 0; y < (winHeight/detailSize); y++)
            {
                for(int x = 0; x < (winWidth/detailSize); x++)
                {
                    if(colors[x,y].A == 0)
                    {
                        Console.Write(" ");
                        continue;
                    }

                    int intensity = colors[x,y].R;

                    if(intensity <= 21)
                    {
                        Console.Write(lumimanceValue[0]);
                    }
                    else if(intensity < 42)
                    {
                        Console.Write(lumimanceValue[1]);
                    }
                    else if(intensity < 65)
                    {
                        Console.Write(lumimanceValue[2]);
                    }
                    else if(intensity < 86)
                    {
                        Console.Write(lumimanceValue[3]);
                    }
                    else if(intensity < 108)
                    {
                        Console.Write(lumimanceValue[4]);
                    }
                    else if(intensity < 130)
                    {
                        Console.Write(lumimanceValue[5]);
                    }
                    else if(intensity < 151)
                    {
                        Console.Write(lumimanceValue[6]);
                    }
                    else if(intensity < 173)
                    {
                        Console.Write(lumimanceValue[7]);
                    }
                    else if(intensity < 195)
                    {
                        Console.Write(lumimanceValue[8]);
                    }
                    else if(intensity < 216)
                    {
                        Console.Write(lumimanceValue[9]);
                    }
                    else if(intensity < 237)
                    {
                        Console.Write(lumimanceValue[10]);
                    }
                    else if(intensity <= 255)
                    {
                        Console.Write(lumimanceValue[11]);
                    }
                }
                Console.Write("|\n");
            }
            Console.CursorVisible = true;
        }

        public bool RayMarch(Vector3 position, Vector3 dir, out float length, out Color color)
        {
            color = Color.Pink;
            length = 2000;

            dir.Normalize();

            Vector3 testPos = position;
            const int maxSteps = 100;
            for (int iter = 0; iter < maxSteps; iter++)
            {
                float dst = 9999;
                float test;
                foreach(IObject obj in this.objectList)
                {
                    test = obj.SDF(testPos);
                    if(test < dst)
                    {
                        dst = test;
                    }
                }

                if(dst < 0.1f)
                {
                    float bestDst = 9999;
                    Color bestColor = color;
                    IObject bestObj = null; 
                    foreach(IObject obj in this.objectList)
                    {
                        test = obj.SDF(testPos);
                        if(test < bestDst)
                        {
                            bestDst = test;
                            bestColor = obj.GetColor();
                            bestObj = obj;
                        }
                    }
                    length = (position - testPos).Length();

                    Vector3 startPos;
                    float lightIntensity = 0;
                    foreach(Light light in this.lightList)
                    {
                        startPos = testPos+bestObj.SDF_normal(testPos)*2;

                        lightIntensity += LightRayMarch(startPos, light);
                    }

                    color = new Color(bestColor.R*lightIntensity,
                                      bestColor.G*lightIntensity,
                                      bestColor.B*lightIntensity);

                    return true;
                }

                testPos += dir*dst;
            }
            return false;
        }

        public float LightRayMarch(Vector3 position, Light light)
        {
            Vector3 dir = (light.position - position);
            dir.Normalize();

            float length = 1f;
            float test;
            float intensity = light.intensity;
            while(length < (position - light.position).Length() - 0.1f)
            {
                float dst = 9999;

                foreach(IObject dObj in this.objectList) 
                {
                    test = dObj.SDF(position + dir*length);
                    if(test < dst)
                        dst = test;
                }

                test = light.SDF(position + dir*length);
                if(test < dst)
                {
                    break;
                }

                if( dst<0.01f )
                    return 0.0f;

                length += dst;
            }
            length = (position - light.position).Length();
            return intensity/(length*length);
        }
    }
}
