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

        Shapes shapes;

        int winWidth = 1200;
        int winHeight = 900;
        int detailSize = 10; 

        Map map;
        Player player;
        SpriteFont font;

        Stopwatch watch;

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

            map = Map.instance;
            map.SetMap("basic");
            player = Player.instance;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Fonts/MainFont");

            shapes = new Shapes(this, new Point(0, winHeight), _spriteBatch);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Up)) // F1 - change draw style
                player.position += new Vector3((float)Math.Cos(player.rotation.X*Math.PI/180)*2,(float)Math.Sin(player.rotation.X*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.Down)) // F1 - change draw style
                player.position -= new Vector3((float)Math.Cos(player.rotation.X*Math.PI/180)*2,(float)Math.Sin(player.rotation.X*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.Left)) // F1 - change draw style
                player.rotation.X -= 1f;

            if (Keyboard.GetState().IsKeyDown(Keys.Right)) // F1 - change draw style
                player.rotation.X += 1f;

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            watch = new Stopwatch();
            watch.Start();
            for(int y = 0; y < (winHeight/detailSize); y++)
                for(int x = 0; x < (winWidth/detailSize); x++)
                {
                    float azimuth     = player.rotation.X + (-player.xFOV/2 + (player.xFOV/(winWidth/detailSize)*(x+1)));
                    float inclination = player.rotation.Y + (-player.yFOV/2 + (player.yFOV/(winHeight/detailSize)*(y+1)));

                    shapes.Begin();
                    float length = 0;
                    Color color;
                    if(RayCast(player.position, inclination, azimuth, out length, out color))
                    {
                        shapes.DrawRectangle(new Point(x*detailSize,y*detailSize), 
                                            detailSize,detailSize, 
                                            new Color(color.R*(float)Math.Pow(0.995f,length)/255,
                                                      color.G*(float)Math.Pow(0.995f,length)/255,
                                                      color.B*(float)Math.Pow(0.995f,length)/255));
                    }
                    else
                    {
                        shapes.DrawRectangle(new Point(x*detailSize,y*detailSize), 
                                            detailSize,detailSize, 
                                            Color.Blue);
                    }
                    shapes.End();
                    /* shapes.DrawText($"{azimuth}:{inclination}", font, new Vector2(x*detailSize + 25,y*detailSize + 25), Color.Red, 1,1); */
                    /* shapes.DrawText($"{length}", font, new Vector2(x*detailSize + 25,y*detailSize + 37), Color.Red, 1,1); */
                }
            watch.Stop();
            shapes.DrawText($"{watch.ElapsedMilliseconds}", font, new Vector2(10,10), Color.Red, 0,0);

            base.Draw(gameTime);
        }

        //https://en.wikipedia.org/wiki/Spherical_coordinate_system
        bool RayCast(Vector3 playerPos, float inclination, float azimuth, out float length, out Color color)
        {
            length = 1000;
            color = Color.Blue;

            double R_inclination = inclination*Math.PI/180f;
            double R_azimuth = azimuth*Math.PI/180f;

            double x = Math.Cos(R_azimuth)*Math.Sin(R_inclination); 
            double y = Math.Sin(R_azimuth)*Math.Sin(R_inclination); 
            double z = Math.Cos(R_inclination);
            
            Vector3 testPos;
            Vector3 rayVector = new Vector3((float)x,(float)y,(float)z);
            for (int r = 10; r < length; r+=5)
            {
                testPos = playerPos + rayVector*r;
                if(map.Collide(testPos, out Color colColor))
                {
                    length = r;
                    color = colColor;
                    return true;
                }
            }

            return false;
        }
    }
}
