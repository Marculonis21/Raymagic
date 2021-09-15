using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Raymagic
{
    public class MainGame : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        public static Random random = new Random();

        Shapes shapes;

        int winWidth = 1200;
        int winHeight = 900;
        int detailSize = 10; 

        Map map;
        Player player;
        int zoom = 450;
        SpriteFont font;

        Stopwatch watch;

        public MainGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = winWidth;
            _graphics.PreferredBackBufferHeight = winHeight;
            _graphics.ApplyChanges();

            map = Map.instance;
            map.LoadMaps();

            UserInit();
            /* map.SetMap("basic"); */

            player = Player.instance;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Fonts/MainFont");

            shapes = new Shapes(this, new Point(0, winHeight), _spriteBatch);

            Informer.instance.SetShapes(this.shapes);
        }
        
        private void UserInit()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            /* Console.Clear(); */

            Console.CursorLeft = (Console.WindowWidth/2) - 15;
            Console.WriteLine("-------------------------------");
            Console.CursorLeft = (Console.WindowWidth/2) - 15;
            Console.WriteLine("| --- WELCOME TO RAYMAGIC --- |");
            Console.CursorLeft = (Console.WindowWidth/2) - 15;
            Console.WriteLine("-------------------------------");

            Console.WriteLine("MAP SELECT:");

            int i = 1;
            foreach(string key in map.maps.Keys)
            {
                Console.CursorLeft = 3;
                Console.WriteLine($"{i}: {key}");
                i++;
            }
            Console.WriteLine();

            int input;
            while(true)
            {
                if(int.TryParse(Console.ReadLine(), out input))
                {
                    if(input > 0 && input < map.maps.Count + 1)
                    {
                        i = 1;
                        foreach(string key in map.maps.Keys)
                        {
                            if(input == i)
                            {
                                Console.WriteLine($"Map chosen: {key}");
                                map.SetMap(key);
                            }
                            i++;
                        }
                        break;
                    }
                }
            }
        }

        int lastMouseX = 200;
        int lastMouseY = 200;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.D1)) 
            {
                detailSize = 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) 
            {
                detailSize = 2;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) 
            {
                detailSize = 3;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) 
            {
                detailSize = 4;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) 
            {
                detailSize = 5;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D6)) 
            {
                detailSize = 6;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D7)) 
            {
                detailSize = 7;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D8)) 
            {
                detailSize = 8;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D9)) 
            {
                detailSize = 9;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D0)) 
            {
                detailSize = 10;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W)) 
                player.position += new Vector3((float)Math.Cos(player.rotation.X*Math.PI/180)*2,(float)Math.Sin(player.rotation.X*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.S)) 
                player.position -= new Vector3((float)Math.Cos(player.rotation.X*Math.PI/180)*2,(float)Math.Sin(player.rotation.X*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.A))
                player.position -= new Vector3((float)Math.Cos((90+player.rotation.X)*Math.PI/180)*2,(float)Math.Sin((90+player.rotation.X)*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.D)) 
                player.position += new Vector3((float)Math.Cos((90+player.rotation.X)*Math.PI/180)*2,(float)Math.Sin((90+player.rotation.X)*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.Space)) 
                player.Jump(gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.PageUp)) 
                zoom+=10;
            if (Keyboard.GetState().IsKeyDown(Keys.PageDown)) 
                zoom-=10;

            MouseState mouse = Mouse.GetState(this.Window);
            if(lastMouseX != -1)
                player.Rotate(new Vector2(mouse.X - lastMouseX, mouse.Y - lastMouseY));

            Mouse.SetPosition(200,200);
            lastMouseX = 200;
            lastMouseY = 200;

            player.Update(this, gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            watch = new Stopwatch();
            watch.Start();
            Color[,] colors = new Color[winWidth/detailSize,winHeight/detailSize];
            float[,] lengths = new float[winWidth/detailSize,winHeight/detailSize];

            // Get player look dir vector
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

            // Parallel RAYMARCHING!!!
            Parallel.For(0, (winHeight/detailSize) * (winWidth/detailSize),new ParallelOptions{ MaxDegreeOfParallelism = Environment.ProcessorCount}, i =>
            {
                int y = i / (winWidth/detailSize);
                int x = i % (winWidth/detailSize);

                // get ray dir from camera through view plane (detailSize) "rectangles"
                int _x = x - (winWidth /detailSize)/2;
                int _y = y - (winHeight/detailSize)/2;

                Vector3 rayDir = (player.position + playerLookDir*zoom + playerLookPerpenSIDE*_x*detailSize + playerLookPerpenUP*_y*detailSize) - player.position;

                if(RayMarch(player.position, rayDir, out float length, out Color color))
                {
                    colors[x,y] = color;
                    lengths[x,y] = length;
                }
            });
            watch.Stop();
            Informer.instance.AddInfo("debug", $"--- DEBUG INFO ---");
            Informer.instance.AddInfo("debug rays", $" ray phase: {watch.ElapsedMilliseconds}");

            watch = new Stopwatch();
            watch.Start();
            shapes.Begin();
            for(int y = 0; y < (winHeight/detailSize); y++)
                for(int x = 0; x < (winWidth/detailSize); x++)
                {
                    shapes.DrawRectangle(new Point(x*detailSize,y*detailSize), 
                                         detailSize,detailSize, 
                                         new Color(colors[x,y].R,
                                                   colors[x,y].G,
                                                   colors[x,y].B));
                }

            for(int y = -player.cursorSize; y < player.cursorSize; y++)
                for(int x = -player.cursorSize; x < player.cursorSize; x++)
                {
                    if(x*x + y*y < player.cursorSize+3 && x*x + y*y > player.cursorSize-3)
                    {
                        shapes.DrawRectangle(new Point(winWidth/2 + x*detailSize,winHeight/2 + y*detailSize), 
                                             detailSize,detailSize, 
                                             Color.Gold);
                    }
                }
            shapes.End();
            watch.Stop();

            Informer.instance.AddInfo("debug draw", $" draw phase: {watch.ElapsedMilliseconds}");
            Informer.instance.AddInfo("details", $"details: {detailSize}");

            Informer.instance.ShowInfo(new Vector2(10,10), this.font, Color.Red);
            base.Draw(gameTime);
        }

        public bool RayMarch(Vector3 position, Vector3 dir, out float length, out Color color)
        {
            color = Color.Blue;
            length = 2000;

            dir.Normalize();

            Vector3 testPos = position;
            const int maxSteps = 100;
            for (int iter = 0; iter < maxSteps; iter++)
            {
                Vector3 cords = testPos - map.mapOrigin;
                float dst;
                bool sObj = true;
                try
                {
                    dst = map.distanceMap[(int)(cords.X/map.distanceMapDetail),
                                          (int)(cords.Y/map.distanceMapDetail),
                                          (int)(cords.Z/map.distanceMapDetail)];
                }
                catch (IndexOutOfRangeException e)
                {
                    return false;
                }

                float test;
                foreach(IObject dObj in map.dynamicObjectList)
                {
                    test = dObj.SDF(testPos);
                    if(test < dst)
                    {
                        dst = test;
                        sObj = false;
                    }
                }

                if(dst < 0.1f)
                {
                    length = (position - testPos).Length();

                    float bestDst = length;
                    Color bestColor = color;
                    IObject bestObj = null; 
                    foreach(IObject obj in (sObj ? map.staticObjectList : map.dynamicObjectList))
                    {
                        test = obj.SDF(testPos);
                        if(test < bestDst)
                        {
                            bestDst = test;
                            bestColor = obj.GetColor();
                            bestObj = obj;
                        }
                    }

                    Vector3 startPos;
                    float lightIntensity = 0;
                    foreach(Light light in map.lightList)
                    {
                        startPos = testPos+bestObj.SDF_normal(testPos)*5;

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

            float length = 0.1f;
            float test;
            float intensity = light.intensity;
            float k = 16f*intensity;
            while(length < (position - light.position).Length() - 0.1f)
            {
                Vector3 cords = position + dir*length - map.mapOrigin;
                float dst = map.distanceMap[(int)Math.Abs(cords.X/map.distanceMapDetail),
                                            (int)Math.Abs(cords.Y/map.distanceMapDetail),
                                            (int)Math.Abs(cords.Z/map.distanceMapDetail)];

                foreach(IObject dObj in map.dynamicObjectList)
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

                intensity = Math.Min(intensity, k*dst/length);
                if(intensity < 0.001f)
                    return 0.0f;

                length += dst;
            }
            length = (position - light.position).Length();
            return intensity/(length*length);
        }

        public void PhysicsRayMarch(Vector3 position, Vector3 dir, int maxSteps, float stepMinSize, out float length, out Vector3 hit, out IObject hitObj)
        {
            hit = position;
            length = 2000;
            hitObj = null;

            dir.Normalize();

            Vector3 testPos = position;
            float test;
            float dst;
            for (int iter = 0; iter < maxSteps; iter++)
            {
                dst = 9999;
                foreach(IObject obj in map.staticObjectList)
                {
                    test = obj.SDF(testPos);
                    if(test < dst)
                    {
                        dst = test;
                        hitObj = obj;
                    }
                }

                foreach(IObject dObj in map.dynamicObjectList)
                {
                    test = dObj.SDF(testPos);
                    if(test < dst)
                    {
                        dst = test;
                        hitObj = dObj;
                    }
                }

                if(dst <= stepMinSize)
                {
                    if(dst < 0)
                        length = -1;
                    else
                        length = (position - testPos).Length(); 
                    hit = testPos;
                    return;
                }

                testPos += dir*dst;
            }

            length = (position - testPos).Length();
            hit = testPos;
        }
    }
}
