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
                RayMarchingHelper.PhysicsRayMarch(player.position, player.lookDir, 300, 0, out float length, out Vector3 hit, out Object hitObj);
                foreach(Object dObj in map.dynamicObjectList)
                {
                    if(hitObj == dObj)
                    {
                        outObj = hitObj;
                        Console.WriteLine(outObj.Info);
                    }
                }
                /* map.infoObjectList.Add(new Sphere(hit, 10, Color.Red, false, new Vector3(20,20,20))); */

                if(outObj != null)
                {
                    if(lPressed)
                        outObj.DisplayBoundingBox();
                    if(rPressed)
                        outObj.HideBoundingBox();
                }
                lPressed = rPressed = false;
            }

            player.Update(gameTime);

            // test dobj movement
            /* map.dynamicObjectList[0].Rotate(1f,"z"); */
            /* float z = (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 1000f); */
            /* map.dynamicObjectList[1].Translate(new Vector3(0,0,z)); */
            /* map.dynamicObjectList[0].Rotate(1f, "z"); */
            /* map.dynamicObjectList[1].Rotate(2f, "z"); */

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
