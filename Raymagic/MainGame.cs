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
        int detailSize = 5; 

        Map map;
        Player player;
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

        int lastMouseX = 200;
        int lastMouseY = 200;

        bool fisheyeRemover = false;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if(Keyboard.GetState().IsKeyDown(Keys.F1))
                fisheyeRemover = false; 
            if(!Keyboard.GetState().IsKeyDown(Keys.F1))
                fisheyeRemover = true;

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
                                            new Color(colors[x,y].R*50/(lengths[x,y]*lengths[x,y]),
                                                      colors[x,y].G*50/(lengths[x,y]*lengths[x,y]),
                                                      colors[x,y].B*50/(lengths[x,y]*lengths[x,y])));
                }
            shapes.End();
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            /* for(int y = 0; y < (winHeight/detailSize); y++) */
            /*     for(int x = 0; x < (winWidth/detailSize); x++) */
            /*     { */
            /*         float azimuth     = player.rotation.X + (-player.xFOV/2 + (player.xFOV/(winWidth/detailSize)*(x+1))); */
            /*         float inclination = player.rotation.Y + (-player.yFOV/2 + (player.yFOV/(winHeight/detailSize)*(y+1))); */

            /*         shapes.Begin(); */
            /*         float length = 0; */
            /*         Color color; */
            /*         /1* if(RayCast(player.position, inclination, azimuth, player.rotation.Y, player.rotation.X, out length, out color)) *1/ */
            /*         /1* { *1/ */
            /*         /1*     shapes.DrawRectangle(new Point(x*detailSize,y*detailSize), *1/ */ 
            /*         /1*                         detailSize,detailSize, *1/ */ 
            /*         /1*                         new Color(color.R*(float)Math.Pow(0.995f,length)/255, *1/ */
            /*         /1*                                   color.G*(float)Math.Pow(0.995f,length)/255, *1/ */
            /*         /1*                                   color.B*(float)Math.Pow(0.995f,length)/255)); *1/ */
            /*         /1* } *1/ */
            /*         /1* else *1/ */
            /*         /1* { *1/ */
            /*         /1*     shapes.DrawRectangle(new Point(x*detailSize,y*detailSize), *1/ */ 
            /*         /1*                         detailSize,detailSize, *1/ */ 
            /*         /1*                         Color.Blue); *1/ */
            /*         /1* } *1/ */

            /*         if(RayMarch(player.position, inclination, azimuth, out length, out color)) */
            /*         { */
            /*             shapes.DrawRectangle(new Point(x*detailSize,y*detailSize), */ 
            /*                                 detailSize,detailSize, */ 
            /*                                 new Color(color.R*(float)Math.Pow(0.995f,length)/255, */
            /*                                           color.G*(float)Math.Pow(0.995f,length)/255, */
            /*                                           color.B*(float)Math.Pow(0.995f,length)/255)); */
            /*         } */
            /*         shapes.End(); */
            /*     } */
            /* shapes.DrawText($"{watch.ElapsedMilliseconds}", font, new Vector2(10,10), Color.Red, 0,0); */
            /* shapes.DrawText($"{fisheyeRemover.ToString()}", font, new Vector2(10,25), Color.Red, 0,0); */

            base.Draw(gameTime);
        }


        float sdf_box (Vector3 p, Vector3 c, Vector3 s)
        {
            float x = Math.Max
            (   
                p.X - c.X - new Vector3(s.X / 2.0f, 0, 0).Length(),
                c.X - p.X - new Vector3(s.X / 2.0f, 0, 0).Length()
            );
            float y = Math.Max
            (   p.Y - c.Y - new Vector3(s.Y / 2.0f, 0, 0).Length(),
                c.Y - p.Y - new Vector3(s.Y / 2.0f, 0, 0).Length()
            );
            
            float z = Math.Max
            (   p.Z - c.Z - new Vector3(s.Z / 2.0f, 0, 0).Length(),
                c.Z - p.Z - new Vector3(s.Z / 2.0f, 0, 0).Length()
            );
            float d = x;
            d = Math.Max(d,y);
            d = Math.Max(d,z);
            return d;
        }

        float distToScene(Vector3 p)
        {
            float b1 = sdf_box(p, boxPos, boxSize);
            float b2 = sdf_box(p, boxPos2, boxSize2);

            return(Math.Max(b1,-b2));
        }

        Vector3 boxPos = new Vector3(550,550,250);
        Vector3 boxSize = new Vector3(100,100,300);
        Color boxColor = Color.Red;

        Vector3 boxPos2 = new Vector3(600,600,250);
        Vector3 boxSize2 = new Vector3(100,200,100);
        Color boxColor2 = Color.Yellow;
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

            const int maxSteps = 40;
            for (int iter = 0; iter < maxSteps; iter++)
            {
                float dst1 = 0;
                float dst2 = 0;
                float d;
                if(fisheyeRemover)
                {
                    dst1 = sdf_box(testPos, boxPos, boxSize);
                    dst2 = sdf_box(testPos, boxPos2, boxSize2);
                    d = Math.Min(dst1, dst2);
                }
                else
                {
                    d = distToScene(testPos);
                }

                if(d < 0)
                {
                    color = Color.Green;
                    length = 0;
                    return true;
                }
                else
                {
                    if(d<3)
                    {
                        if(fisheyeRemover)
                        {
                            if(dst1 < dst2)
                                color = boxColor;
                            else
                                color = boxColor2;
                        }
                        else
                        {
                            color = boxColor;
                        }
                        length = (position - testPos).Length();
                        return true;
                    }
                }
                testPos += rayVector*d;
            }
            return false;
        }

        //https://en.wikipedia.org/wiki/Spherical_coordinate_system
        bool RayCast(Vector3 playerPos, float inclination, float azimuth, float playerInclination, float playerAzimuth, out float length, out Color color)
        {
            length = 2000;
            color = Color.Blue;

            double R_inclination = inclination*Math.PI/180f;
            double R_azimuth = azimuth*Math.PI/180f;
            double R_pInclination = playerInclination*Math.PI/180f;
            double R_pAzimuth = playerAzimuth*Math.PI/180f;

            double x = Math.Cos(R_azimuth)*Math.Sin(R_inclination); 
            double y = Math.Sin(R_azimuth)*Math.Sin(R_inclination); 
            double z = Math.Cos(R_inclination);
            
            Vector3 testPos;
            Vector3 rayVector = new Vector3((float)x,(float)y,(float)z);

            for (int r = 0; r < length; r+=5)
            {
                if(fisheyeRemover)
                    testPos = playerPos + rayVector*r*(float)Math.Cos(R_azimuth - R_pAzimuth)*(float)Math.Cos(R_inclination - R_pInclination);
                else
                    testPos = playerPos + rayVector*r;

                if(map.CheckPointBox(testPos, out Color colColor))
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
