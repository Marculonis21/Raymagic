using System;
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

        Shapes shapes;

        int winWidth = 800;
        int winHeight = 500;
        int detailSize = 2; 

        Map map = Map.instance;
        Player player = Player.instance;

        public MainGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = winWidth;
            _graphics.PreferredBackBufferHeight = winHeight;
            _graphics.ApplyChanges();

            shapes = new Shapes(this, new Point(0, winHeight), _spriteBatch);

            map.SetMap("basic");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            shapes.Begin();
            for(int y = 0; y < (winHeight/detailSize); y++)
                for(int x = 0; x < (winWidth/detailSize); x++)
                {
                    float xRayAngle = player.rotation.X + (-player.xFOV/2 + x);
                    float yRayAngle = player.rotation.Y + (-player.yFOV/2 + y);

                    shapes.DrawRectangle(new Point(x*detailSize,y*detailSize), 
                                         detailSize,detailSize, 
                                         RayCast(xRayAngle,yRayAngle));
                }
            shapes.End();

            base.Draw(gameTime);
        }

        Color RayCast(float xAngle, float yAngle)
        {
            return new Color();
        }
    }
}
