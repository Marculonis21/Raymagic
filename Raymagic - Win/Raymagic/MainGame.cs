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

        ConsoleMenu menu;

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

            menu = ConsoleMenu.instance;

            menu.DisplayMenu();

            player = Player.instance;

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
        

        protected override void Update(GameTime gameTime)
        {
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

            player.Controlls(this, gameTime, mouse);

            if (map.portalList[0] != null && map.portalList[1] != null)
            {
                foreach (var portal in map.portalList)
                {
                    portal.CheckTransfer();
                    portal.OnFieldExit();
                    portal.OnFieldEnter();
                }
            }

            player.Update(gameTime);
            map.Update(gameTime);

            base.Update(gameTime);

            if (map.mapPreloadingLoadingMap)
            {
                // empties thread for map preloading in threadpool */
                Thread.Sleep(10);  
            }
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            Informer.instance.AddInfo("debug", $"--- DEBUG INFO ---");

            screen.DrawGame();
            Informer.instance.ShowInfo(new Vector2(10, 10), this.font, Color.Red);
        
            base.Draw(gameTime);
        }
    }
}
