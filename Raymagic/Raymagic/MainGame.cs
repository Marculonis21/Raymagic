using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

        Graphics graphics;

        int winWidth = 1024;
        int winHeight = 1024;
        int detailSize = 10; 

        Map map;
        Player player;
        int zoom = 450;

        SpriteFont font;

        Screen screen;

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

            player = Player.instance;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Fonts/MainFont");

            graphics = new Graphics(this, new Point(0, winHeight), _spriteBatch);

            screen = Screen.instance;
            screen.Init(graphics, new Point(winWidth, winHeight), detailSize);

            Informer.instance.SetGraphics(this.graphics);
        }
        
        private void UserInit()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;

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

        bool lPressed = false;
        bool rPressed = false;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.D1)) detailSize = 1;
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) detailSize = 2; 
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) detailSize = 3; 
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) detailSize = 4; 
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) detailSize = 5; 
            if (Keyboard.GetState().IsKeyDown(Keys.D6)) detailSize = 6; 
            if (Keyboard.GetState().IsKeyDown(Keys.D7)) detailSize = 7; 
            if (Keyboard.GetState().IsKeyDown(Keys.D8)) detailSize = 8; 
            if (Keyboard.GetState().IsKeyDown(Keys.D9)) detailSize = 9; 
            if (Keyboard.GetState().IsKeyDown(Keys.D0)) detailSize = 10;

            screen.SetDetailSize(detailSize);

            MouseState mouse = Mouse.GetState(this.Window);
            player.Controlls(gameTime, mouse);

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                lPressed = true;
            }

            if (mouse.RightButton == ButtonState.Pressed)
            {
                rPressed = true;
            }


            Object outObj = null;
            if ((mouse.LeftButton == ButtonState.Released && lPressed) || (mouse.RightButton == ButtonState.Released && rPressed))
            {
                PhysicsRayMarch(player.position, player.lookDir, 300, 0, out float length, out Vector3 hit, out Object hitObj);
                foreach(Object dObj in map.dynamicObjectList)
                {
                    if(hitObj == dObj)
                    {
                        outObj = hitObj;
                        Console.WriteLine(outObj.Info);
                    }
                }
                map.infoObjectList.Add(new Sphere(hit, 10, Color.Red, false, new Vector3(20,20,20)));

                if(outObj != null)
                {
                    if(lPressed)
                        outObj.DisplayBoundingBox();
                    if(rPressed)
                        outObj.HideBoundingBox();
                }
                lPressed = rPressed = false;
            }

            player.Update(this, gameTime);

            // test dobj movement
            /* map.dynamicObjectList[0].Rotate(1f,"z"); */
            /* float z = (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 1000f); */
            /* map.dynamicObjectList[1].Translate(new Vector3(0,0,z)); */
            map.dynamicObjectList[0].Rotate(1f, "z");
            /* map.dynamicObjectList[1].Rotate(2f, "z"); */

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Pink);

            Informer.instance.AddInfo("debug", $"--- DEBUG INFO ---");
            screen.DrawGame(this);


            Informer.instance.ShowInfo(new Vector2(10,10), this.font, Color.Red);
            base.Draw(gameTime);
        }

        public void RayMarch(Vector3 position, Vector3 dir, out float length, out Color color)
        {
            color = Color.Black;
            length = float.MaxValue;

            dir.Normalize();

            Vector3 testPos = position;
            const int maxSteps = 100;
            for (int iter = 0; iter < maxSteps; iter++)
            {
                bool sObj = true;

                Vector3 coords = testPos - map.mapOrigin;

                if((int)(coords.X/map.distanceMapDetail) >= map.distanceMap.GetLength(0) ||
                   (int)(coords.Y/map.distanceMapDetail) >= map.distanceMap.GetLength(1) ||
                   (int)(coords.Z/map.distanceMapDetail) >= map.distanceMap.GetLength(2) || 
                   (int)(coords.X/map.distanceMapDetail) < 0 ||
                   (int)(coords.Y/map.distanceMapDetail) < 0 ||
                   (int)(coords.Z/map.distanceMapDetail) < 0)
                {
                    return;
                }

                float dst = map.distanceMap[(int)Math.Abs(coords.X/map.distanceMapDetail),
                                            (int)Math.Abs(coords.Y/map.distanceMapDetail),
                                            (int)Math.Abs(coords.Z/map.distanceMapDetail)];

                Object bestDObj = null;
                float test = map.BVH.Test(testPos, dst, out Object dObj);
                if(test < dst)
                {
                    dst = test;
                    bestDObj = dObj;
                    sObj = false;
                }

                foreach(Object iObj in map.infoObjectList)
                {
                    test = iObj.SDF(testPos, dst);
                    if(test < dst)
                    {
                        dst = test;
                        if(dst < 0.1f)
                        {
                            color = Color.Red;
                            return;
                        }
                    }
                }

                if(dst < 0.1f)
                {
                    float bestDst = float.MaxValue;
                    Color bestColor = color;
                    Object bestObj = null; 

                    if(sObj)
                    {
                        foreach(Object obj in map.staticObjectList)
                        {
                            test = obj.SDF(testPos, dst);
                            if(test <= bestDst)
                            {
                                bestDst = test;
                                bestColor = obj.Color;
                                bestObj = obj;
                            }
                        }
                    }
                    else
                    {
                        bestDst = dst;
                        bestColor = bestDObj.Color;
                        bestObj = bestDObj;
                    }

                    Vector3 startPos;
                    float lightIntensity = 0;
                    foreach(Light light in map.lightList)
                    {
                        startPos = testPos+bestObj.SDF_normal(testPos)*2;
                        
                        lightIntensity += LightRayMarch(startPos, light);
                    }

                    lightIntensity = Math.Max(lightIntensity, 0.0001f); // try around something
                    color = new Color(bestColor.R*lightIntensity,
                                      bestColor.G*lightIntensity,
                                      bestColor.B*lightIntensity);

                    return;
                }

                testPos += dir*dst;
            }
        }

        public float LightRayMarch(Vector3 position, Light light)
        {
            Vector3 dir = (light.position - position);
            dir.Normalize();

            float length = 1f;
            float test;
            float intensity = light.intensity;
            float k = 16f*intensity;
            while(length < (position - light.position).Length() - 0.1f)
            {
                Vector3 coords = position + dir*length - map.mapOrigin;

                float dst = map.distanceMap[(int)Math.Abs(coords.X/map.distanceMapDetail),
                                            (int)Math.Abs(coords.Y/map.distanceMapDetail),
                                            (int)Math.Abs(coords.Z/map.distanceMapDetail)];

                test = map.BVH.Test(position + dir*length, dst, out Object dObj);
                if(test < dst)
                {
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

        public void PhysicsRayMarch(Vector3 position, Vector3 dir, int maxSteps, float stepMinSize, out float length, out Vector3 hit, out Object hitObj)
        {
            hit = position;
            length = float.MaxValue;
            hitObj = null;

            dir.Normalize();

            Vector3 testPos = position;
            float test;
            float dst;
            for (int iter = 0; iter < maxSteps; iter++)
            {
                dst = float.MaxValue;
                foreach(Object obj in map.staticObjectList)
                {
                    test = obj.SDF(testPos, dst);
                    if(test < dst)
                    {
                        dst = test;
                        hitObj = obj;
                    }
                }

                foreach(Object dObj in map.dynamicObjectList)
                {
                    test = dObj.SDF(testPos, dst, physics:true);
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
