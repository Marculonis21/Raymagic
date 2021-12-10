using System;
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

        int winWidth = 36;
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

        Sphere[] centerSphere;
        private void Prep()
        {
            //interesting
            /* Sphere sphere = new Sphere(player.position + player.lookDir*300, */ 
            /*                           0, */
            /*                           Color.White, */
            /*                           false); */

            /* Box pillar = new Box(new Vector3(0,100,0), */
            /*                      new Vector3(50,50,100), */
            /*                      Color.White, */
            /*                      false); */
            /* pillar.Rotate(50,"x"); */

            /* Box pillar2 = new Box(new Vector3(-100,0,0), */
            /*                      new Vector3(50,50,100), */
            /*                      Color.White, */
            /*                      false); */
            /* pillar2.Rotate(-50,"x"); */
            /* sphere.AddBoolean(BooleanOP.UNION, pillar); */
            /* sphere.AddBoolean(BooleanOP.UNION, pillar2); */

            centerSphere = new Sphere[2];
            for (int i = 0; i < centerSphere.Length; i++)
            {
                centerSphere[i] = new Sphere(player.position + player.lookDir*320, 
                                             0,
                                             ConsoleColor.White,
                                             false);
                
            }

            Box planeBox = new Box(new Vector3(0,0,-185),
                                   new Vector3(320,320,100),
                                   ConsoleColor.White,
                                   false);

            planeBox.AddBoolean(BooleanOP.INTERSECT, new Sphere(new Vector3(0,0,300),
                                                                290,
                                                                ConsoleColor.White,
                                                                false));

            centerSphere[0].AddBoolean(BooleanOP.UNION, planeBox);

            Box pillar = new Box(new Vector3(0,-75,0),
                                 new Vector3(50,50,120),
                                 ConsoleColor.White,
                                 false);
            pillar.Rotate(60,"x");
            centerSphere[1].AddBoolean(BooleanOP.UNION, pillar);

            Box pillar2 = new Box(new Vector3(0,75,0),
                                 new Vector3(50,50,120),
                                 ConsoleColor.White,
                                 false);
            pillar2.Rotate(-60,"x");
            centerSphere[1].AddBoolean(BooleanOP.UNION, pillar2);

            Box pillar3 = new Box(new Vector3(0,-100,-75),
                                 new Vector3(50,50,120),
                                 ConsoleColor.White,
                                 false);
            centerSphere[1].AddBoolean(BooleanOP.UNION, pillar3);

            Box pillar4 = new Box(new Vector3(0,100,-75),
                                 new Vector3(50,50,120),
                                 ConsoleColor.White,
                                 false);
            centerSphere[1].AddBoolean(BooleanOP.UNION, pillar4);

            centerSphere[1].SetColor(ConsoleColor.DarkRed);

            for (int i = 0; i < centerSphere.Length; i++)
            {
                centerSphere[i].Translate(new Vector3(0,0,75));
                objectList.Add(centerSphere[i]);
            }

            lightList.Add(new Light(player.position + new Vector3(0,300,300),
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

            for (int i = 0; i < centerSphere.Length; i++)
            {
                
                centerSphere[i].Rotate(2.5f, "z");
            }


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
            ConsoleColor[,] textColor = new ConsoleColor[(int)(winWidth/detailSize),(int)(winHeight/detailSize)];
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

                if(RayMarch(player.position, rayDir, out float length, out Color color, out ConsoleColor tColor))
                {
                    colors[x,y] = color;
                    textColor[x,y] = tColor;
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

                    Console.ForegroundColor = textColor[x,y];
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
                    Console.ResetColor();
                }
                Console.Write("\n");
            }
            Console.CursorVisible = true;
        }

        public bool RayMarch(Vector3 position, Vector3 dir, out float length, out Color color, out ConsoleColor tColor)
        {
            color = Color.Pink;
            tColor = ConsoleColor.Magenta;
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
                    ConsoleColor bestColor = tColor;
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

                    color = new Color(255*lightIntensity,
                                      255*lightIntensity,
                                      255*lightIntensity);
                    tColor = bestColor;

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
