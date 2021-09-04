using System;
using System.Collections.Generic;
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
        int detailSize = 8; 

        Map map;
        Player player;
        SpriteFont font;

        Stopwatch watch;

        public List<IObject> objectList = new List<IObject>();

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
            map.SetMap("basic", this);
            player = Player.instance;

            /* objectList.Add(new Box(new Vector3(350,350,250), */
            /*                        new Vector3(100,100,300), */
            /*                        Color.Red)); */

            /* Box b2 = new Box(new Vector3(175,150,250), */
            /*                  new Vector3(50,50,200), */
            /*                  Color.Yellow); */

            /* b2.bDiffList.Add(new Sphere(new Vector3(220,150,250), */
            /*                             50, */
            /*                             Color.Black)); */
            /* objectList.Add(b2); */

            /* objectList.Add(new Box(new Vector3(500,500,250), */
            /*                        new Vector3(200,20,100), */
            /*                        Color.Green)); */


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Fonts/MainFont");

            shapes = new Shapes(this, new Point(0, winHeight), _spriteBatch);
        }

        int lastMouseX = 200;
        int lastMouseY = 200;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.W)) 
                player.position += new Vector3((float)Math.Cos(player.rotation.X*Math.PI/180)*2,(float)Math.Sin(player.rotation.X*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.S)) 
                player.position -= new Vector3((float)Math.Cos(player.rotation.X*Math.PI/180)*2,(float)Math.Sin(player.rotation.X*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.A))
                player.position -= new Vector3((float)Math.Cos((90+player.rotation.X)*Math.PI/180)*2,(float)Math.Sin((90+player.rotation.X)*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.D)) 
                player.position += new Vector3((float)Math.Cos((90+player.rotation.X)*Math.PI/180)*2,(float)Math.Sin((90+player.rotation.X)*Math.PI/180)*2,0);

            MouseState mouse = Mouse.GetState(this.Window);
            if(lastMouseX != -1)
                player.Rotate(new Vector2(mouse.X - lastMouseX, mouse.Y - lastMouseY));

            Mouse.SetPosition(200,200);
            lastMouseX = 200;
            lastMouseY = 200;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            watch = new Stopwatch();

            watch.Start();
            Color[,] colors = new Color[winWidth/detailSize,winHeight/detailSize];
            float[,] lengths = new float[winWidth/detailSize,winHeight/detailSize];

            Parallel.For(0, winHeight/detailSize, y => 
            {
            Parallel.For(0, winWidth/detailSize, x =>
            {
                    float azimuth     = player.rotation.X + (-player.xFOV/2 + (player.xFOV/(winWidth/detailSize)*(x+1)));
                    float inclination = player.rotation.Y + (-player.yFOV/2 + (player.yFOV/(winHeight/detailSize)*(y+1)));

                    float length = 0;
                    Color color;

                    if(RayMarch(player.position, inclination, azimuth, out length, out color))
                    {
                        colors[x,y] = color;
                        lengths[x,y] = length;
                    }
            });
            });

            shapes.Begin();
            for(int y = 0; y < (winHeight/detailSize); y++)
                for(int x = 0; x < (winWidth/detailSize); x++)
                {
                    shapes.DrawRectangle(new Point(x*detailSize,y*detailSize), 
                                         detailSize,detailSize, 
                                         new Color(colors[x,y].R*55/(lengths[x,y]*lengths[x,y]),
                                                   colors[x,y].G*55/(lengths[x,y]*lengths[x,y]),
                                                   colors[x,y].B*55/(lengths[x,y]*lengths[x,y])));
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
            shapes.DrawText($"{watch.ElapsedMilliseconds}", font, new Vector2(10,10), Color.Red, 0,0);

            base.Draw(gameTime);
        }


        bool RayMarch(Vector3 position, float inclination, float azimuth, out float length, out Color color)
        {
            color = Color.Blue;
            length = 2000;

            double R_inclination = inclination*Math.PI/180f;
            double R_azimuth = azimuth*Math.PI/180f;

            double x = Math.Cos(R_azimuth)*Math.Sin(R_inclination); 
            double y = Math.Sin(R_azimuth)*Math.Sin(R_inclination); 
            double z = Math.Cos(R_inclination);

            Vector3 rayVector = new Vector3((float)x,(float)y,(float)z);
            rayVector.Normalize();

            Vector3 testPos = position;
            float test;

            const int maxSteps = 60;
            for (int iter = 0; iter < maxSteps; iter++)
            {
                float bestDst = length;
                Color bestColor = color;
                foreach(IObject obj in objectList)
                {
                    test = obj.SDF(testPos);
                    if(test < bestDst)
                    {
                        bestDst = test;
                        bestColor = obj.GetColor();
                    }
                }

                if(bestDst < 0)
                {
                    color = Color.Pink;
                    length = 0;
                    return true;
                }
                else
                {
                    if(bestDst<1)
                    {
                        length = (position - testPos).Length();
                        color = bestColor;
                        return true;
                    }
                }

                testPos += rayVector*bestDst;
            }
            return false;
        }
    }
}
