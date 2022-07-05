using System;
using System.Collections.Generic;
using System.IO;
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

            // DISTANCE MAP SLICE 
            /* Texture2D texture1 = new Texture2D(_graphics.GraphicsDevice, map.distanceMap.GetLength(0), map.distanceMap.GetLength(1)); */
            /* Stream stream1 = File.Create("./DistanceMapTestTextures/texture.png"); */

            /* List<Color> colorList1 = new List<Color>(); */
            /* for (int y = 0; y < texture1.Height; y++) */
            /* { */
            /*     for (int x = 0; x < texture1.Width; x++) */
            /*     { */
            /*         int bw = (int)Math.Clamp(map.distanceMap[x,y,50].distance, 0,255); */
            /*         colorList1.Add(new Color(bw,bw,bw)); */

            /*         /1* colorList.Add(map.distanceMap[x,y,100].color); *1/ */
            /*     } */
            /* } */
            /* texture1.SetData<Color>(colorList1.ToArray()); */
            /* texture1.SaveAsPng(stream1, map.distanceMap.GetLength(0), map.distanceMap.GetLength(1)); */
            /* stream1.Close(); */

            player = Player.instance;
            map.portalableObjectList.Add(player);
            base.Initialize();

            GC.Collect();
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

                RayMarchingHelper.PhysicsRayMarch(new Ray(player.position, player.lookDir), 300, 0, out float length, out Vector3 hit, out Object hitObj, caller:player.model);
                if (hitObj.IsSelectable)
                {
                    int type = lPressed ? 0 : 1;
                    map.portalList[type] = (new Portal(hit, hitObj.SDF_normal(hit), type));
                    Console.WriteLine("added");
                    /* foreach(Object dObj in map.dynamicObjectList) */
                    /* { */
                    /*     if(hitObj == dObj) */
                    /*     { */
                    /*         outObj = hitObj; */
                    /*         Console.WriteLine(outObj.Info); */
                    /*     } */
                    /* } */
                    /* map.infoObjectList.Add(new Sphere(hit, 10, Color.Red, false)); */

                    /* if(outObj != null) */
                    /* { */
                    /*     if(lPressed) */
                    /*         outObj.DisplayBoundingBox(); */
                    /*     if(rPressed) */
                    /*         outObj.HideBoundingBox(); */
                    /* } */
                }
                lPressed = rPressed = false;
            }
            map.Update(gameTime);

            player.Update(gameTime);

            /* float x = (float)Math.Cos(gameTime.TotalGameTime.TotalMilliseconds / 1000f); */
            /* float y = (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 1000f); */
            /* map.lightList[0].position += new Vector3(x,y,0); */

            if (map.portalList[0] != null && map.portalList[1] != null)
            {
                foreach (var portal in map.portalList)
                {
                    portal.CheckTransfer();
                    portal.OnFieldExit();
                    portal.OnFieldEnter();
                }
            }

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Pink);

            Informer.instance.AddInfo("debug", $"--- DEBUG INFO ---");
            screen.DrawGame();

            Informer.instance.ShowInfo(new Vector2(10,10), this.font, Color.Red);
            base.Draw(gameTime);
        }
    }
}
