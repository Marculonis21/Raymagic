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

        /* int winWidth = 1200; */
        /* int winHeight = 900; */
        int winWidth = 1024;
        int winHeight = 1024;
        int detailSize = 10; 

        Map map;
        Player player;
        int zoom = 450;
        SpriteFont font;

        Stopwatch watch;

        List<DrawPlanePart> DPPList = new List<DrawPlanePart>();

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

            map.dynamicObjectList[0].Rotate(1f,"z");

            float z = (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 1000f);
            map.dynamicObjectList[1].Translate(new Vector3(0,0,z));

            base.Update(gameTime);
        }
        
        private readonly object lockObj = new object();

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Pink);

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
            /* Parallel.For(0, (winHeight/detailSize) * (winWidth/detailSize),new ParallelOptions{ MaxDegreeOfParallelism = Environment.ProcessorCount}, i => */
            /* { */
            /*     int y = i / (winWidth/detailSize); */
            /*     int x = i % (winWidth/detailSize); */

            /*     // get ray dir from camera through view plane (detailSize) "rectangles" */
            /*     float _x = x - (winWidth /detailSize)/2 + detailSize/2; */
            /*     float _y = y - (winHeight/detailSize)/2 + detailSize/2; */

            /*     Vector3 rayDir = (player.position + playerLookDir*zoom + playerLookPerpenSIDE*_x*detailSize + playerLookPerpenUP*_y*detailSize) - player.position; */

            /*     if(RayMarch(player.position, rayDir, out float length, out Color color)) */
            /*     { */
            /*         colors[x,y] = color; */
            /*         lengths[x,y] = length; */
            /*     } */
            /* }); //35ms from in front of the blue thingy */
            watch.Stop();
            Informer.instance.AddInfo("debug", $"--- DEBUG INFO ---");
            Informer.instance.AddInfo("debug rays", $" ray phase: {watch.ElapsedMilliseconds}");

            /* watch = new Stopwatch(); */
            /* watch.Start(); */
            /* shapes.Begin(); */
            /* for(int y = 0; y < (winHeight/detailSize); y++) */
            /*     for(int x = 0; x < (winWidth/detailSize); x++) */
            /*     { */
            /*         shapes.DrawRectangle(new Point(x*detailSize,y*detailSize), */ 
            /*                              detailSize,detailSize, */ 
            /*                              new Color(colors[x,y].R, */
            /*                                        colors[x,y].G, */
            /*                                        colors[x,y].B)); */
            /*     } */

            /* for(int y = -player.cursorSize; y < player.cursorSize; y++) */
            /*     for(int x = -player.cursorSize; x < player.cursorSize; x++) */
            /*     { */
            /*         if(x*x + y*y < player.cursorSize+3 && x*x + y*y > player.cursorSize-3) */
            /*         { */
            /*             shapes.DrawRectangle(new Point(winWidth/2 + x*detailSize,winHeight/2 + y*detailSize), */ 
            /*                                  detailSize,detailSize, */ 
            /*                                  Color.Gold); */
            /*         } */
            /*     } */
            /* shapes.End(); */
            /* watch.Stop(); */
            /* Informer.instance.AddInfo("debug draw", $" draw phase: {watch.ElapsedMilliseconds}"); */
            /* Informer.instance.AddInfo("details", $"details: {detailSize}"); */


            ////////////////////////////////////////////////////


            /* DrawPlanePart DPP; */
            /* Vector3 rayDir = (player.position + playerLookDir*zoom + playerLookPerpenSIDE*DPP.centerPartPos.X + playerLookPerpenUP*DPP.centerPartPos.Y) - player.position; */

            watch = new Stopwatch();
            watch.Start();
            const int SUBHARDLIMIT = 5;
            DPPList.Clear();

            DrawPlanePart parent = new DrawPlanePart(new Point(0,0),winWidth);
            DrawPlanePart[] subs = parent.Subdivide();
            for(int i = 0; i < subs.Length; i++)
            {
                DrawPlanePart[] subs2 = subs[i].Subdivide();

                for(int i2 = 0; i2 < subs2.Length; i2++)
                {
                    DrawPlanePart[] subs3 = subs2[i2].Subdivide();

                    for(int i3 = 0; i3 < subs3.Length; i3++)
                    {
                        DrawPlanePart[] subs4 = subs3[i3].Subdivide();

                        for(int i4 = 0; i4 < subs4.Length; i4++)
                        {
                            DrawPlanePart[] subs5 = subs4[i4].Subdivide();

                            for(int i5 = 0; i5 < subs5.Length; i5++)
                            {
                                DrawPlanePart[] subs6 = subs5[i5].Subdivide();
                                DPPList.AddRange(subs6);

                                for(int i6 = 0; i6 < subs6.Length; i6++)
                                {
                                    DrawPlanePart[] subs7 = subs6[i6].Subdivide();
                                    DPPList.AddRange(subs7);
                                }
                            }
                        }
                    }
                }
            }

            Parallel.For(0, DPPList.Count, i => 
            {
                var part = DPPList[i];
                if(part.isLeaf)
                {
                    Vector3 rayDir = (player.position + playerLookDir*zoom + playerLookPerpenSIDE*part.centerPartPos.X + playerLookPerpenUP*part.centerPartPos.Y) - player.position;

                    if(RayMarch(player.position, rayDir, out float length, out Color color))
                    {
                        part.color = color;
                    }
                }
            });

            List<DrawPlanePart> update = new List<DrawPlanePart>();
            int iterCount = DPPList.Count;
            Parallel.For(0, iterCount, i => 
            {
                var part = DPPList[i];

                List<DrawPlanePart> neighborsList = new List<DrawPlanePart>();
                bool allColor = true;

                List<DrawPlanePart> wNeighb = part.GetNeighbors("w");
                foreach(var wPart in wNeighb)
                {
                    if(wPart.color != Color.Black)
                        neighborsList.Add(wPart);
                }
                List<DrawPlanePart> sNeighb = part.GetNeighbors("s");
                foreach(var sPart in sNeighb)
                {
                    if(sPart.color != Color.Black)
                        neighborsList.Add(sPart);
                }
                List<DrawPlanePart> aNeighb = part.GetNeighbors("a");
                foreach(var aPart in aNeighb)
                {
                    if(aPart.color != Color.Black)
                        neighborsList.Add(aPart);
                }
                List<DrawPlanePart> dNeighb = part.GetNeighbors("d");
                foreach(var dPart in dNeighb)
                {
                    if(dPart.color != Color.Black)
                        neighborsList.Add(dPart);
                }

                if(neighborsList.Count > 0)
                {
                    Vector3 colorAvg = new Vector3();
                    foreach(var neighb in neighborsList)
                    {
                        colorAvg += new Vector3(neighb.color.R,
                                                neighb.color.G,
                                                neighb.color.B);
                    }
                    colorAvg /= neighborsList.Count;

                    float avgDist = (Math.Abs(colorAvg.X-part.color.R) + Math.Abs(colorAvg.Y-part.color.G) + Math.Abs(colorAvg.Z-part.color.B)) / 3f;
                    part.dst = avgDist;

                    if(avgDist < 0.25f || part.sizeWH < 4)
                    {
                        part.safe = true;
                        part.color = new Color((int)colorAvg.X, (int)colorAvg.Y, (int)colorAvg.Z);
                    }
                    else
                    {
                        lock(lockObj)
                            update.AddRange(part.Subdivide());
                    }
                }
                else // black = fully in shadow
                {
                    part.dst = 0;
                    part.safe = true;
                }
            });

            DPPList.AddRange(update);

            Parallel.For(0, DPPList.Count, i => 
            {
                var part = DPPList[i];
                if(part.isLeaf && !part.safe)
                {
                    Vector3 rayDir = (player.position + playerLookDir*zoom + playerLookPerpenSIDE*part.centerPartPos.X + playerLookPerpenUP*part.centerPartPos.Y) - player.position;

                    if(RayMarch(player.position, rayDir, out float length, out Color color))
                    {
                        part.color = color;
                    }
                }
            });

            update = new List<DrawPlanePart>();
            iterCount = DPPList.Count;
            Parallel.For(0, iterCount, i => 
            {
                var part = DPPList[i];
                if(part.isLeaf)
                {

                    List<DrawPlanePart> neighborsList = new List<DrawPlanePart>();
                    Color NULLCOLOR = new Color(-1,-1,-1);
                    bool allColor = true;

                    List<DrawPlanePart> wNeighb = part.GetNeighbors("w");
                    foreach(var wPart in wNeighb)
                    {
                        if(wPart.color != NULLCOLOR)
                            neighborsList.Add(wPart);
                    }
                    List<DrawPlanePart> sNeighb = part.GetNeighbors("s");
                    foreach(var sPart in sNeighb)
                    {
                        if(sPart.color != NULLCOLOR)
                            neighborsList.Add(sPart);
                    }
                    List<DrawPlanePart> aNeighb = part.GetNeighbors("a");
                    foreach(var aPart in aNeighb)
                    {
                        if(aPart.color != NULLCOLOR)
                            neighborsList.Add(aPart);
                    }
                    List<DrawPlanePart> dNeighb = part.GetNeighbors("d");
                    foreach(var dPart in dNeighb)
                    {
                        if(dPart.color != NULLCOLOR)
                            neighborsList.Add(dPart);
                    }

                    if(neighborsList.Count > 0)
                    {
                        Vector3 colorAvg = new Vector3();
                        foreach(var neighb in neighborsList)
                        {
                            colorAvg += new Vector3(neighb.color.R,
                                                    neighb.color.G,
                                                    neighb.color.B);
                        }
                        colorAvg /= neighborsList.Count;

                        float avgDist = (Math.Abs(colorAvg.X-part.color.R) + Math.Abs(colorAvg.Y-part.color.G) + Math.Abs(colorAvg.Z-part.color.B)) / 3f;
                        part.dst = avgDist;

                        if(avgDist < 0.25f || part.sizeWH < 4)
                        {
                            part.safe = true;
                            part.color = new Color((int)colorAvg.X, (int)colorAvg.Y, (int)colorAvg.Z);
                        }
                        else
                        {
                            /* lock(lockObj) */
                            /*     update.AddRange(part.Subdivide()); */
                        }
                    }
                    else // black = fully in shadow
                    {
                        part.dst = 0;
                        part.safe = true;
                    }
                }
            });

            watch.Stop();
            Informer.instance.AddInfo("debug quadtree", $" quadtree phase: {watch.ElapsedMilliseconds}");

            watch = new Stopwatch();
            watch.Start();
            shapes.Begin();

            foreach(var part in DPPList)
            {
                if(part.isLeaf)
                {
                    shapes.DrawRectangle(new Point(winWidth/2 + part.centerPartPos.X - part.sizeWH/2, winHeight/2+part.centerPartPos.Y - part.sizeWH/2), 
                                         part.sizeWH,part.sizeWH, 
                                         part.color);
                }
            }

            /* List<DrawPlanePart> N = new List<DrawPlanePart>(); */
            /* N.AddRange(chosen.GetNeighbors("w")); */
            /* N.AddRange(chosen.GetNeighbors("s")); */
            /* N.AddRange(chosen.GetNeighbors("d")); */
            /* N.AddRange(chosen.GetNeighbors("a")); */
            /* Console.WriteLine("START"); */
            /* Console.WriteLine(chosen.color); */
            /* Vector3 colorAVG = new Vector3(); */
            /* foreach(var part in N) */
            /* { */
            /*     if(part.isLeaf) */
            /*     { */
            /*         Console.WriteLine(part.color); */
            /*         colorAVG += new Vector3(part.color.R, part.color.G, part.color.B); */
            /*         Console.WriteLine(colorAVG); */
            /*     } */
            /* } */
            /* colorAVG /= N.Count; */
            /* Console.WriteLine(colorAVG); */
            /* float AVGdist = (Math.Abs(colorAVG.X-chosen.color.R) + Math.Abs(colorAVG.Y-chosen.color.G) + Math.Abs(colorAVG.Z-chosen.color.B)) / 3f; */
            /* Console.WriteLine(AVGdist); */
            /* Console.WriteLine(chosen.dst); */
            /* Console.WriteLine("END"); */

            shapes.End();
            foreach(var part in DPPList)
            {
                if(part.safe)
                    shapes.DrawText(".", this.font, new Vector2(winWidth/2 + part.centerPartPos.X,winHeight/2+part.centerPartPos.Y), Color.Red);

                if(part.isLeaf && !part.safe)
                {
                    /* shapes.DrawText(Math.Round(part.dst,3).ToString(), this.font, new Vector2(winWidth/2 + part.centerPartPos.X,winHeight/2+part.centerPartPos.Y), Color.Red); */
                }
            }
            watch.Stop();
            Informer.instance.AddInfo("debug quadtree draw", $" quadtree draw phase: {watch.ElapsedMilliseconds}");

            Informer.instance.AddInfo("count", DPPList.Count.ToString());
            Informer.instance.ShowInfo(new Vector2(10,10), this.font, Color.Red);
            base.Draw(gameTime);
        }

        public bool RayMarch(Vector3 position, Vector3 dir, out float length, out Color color)
        {
            color = Color.Pink;
            length = 2000;

            dir.Normalize();

            Vector3 testPos = position;
            const int maxSteps = 100;
            for (int iter = 0; iter < maxSteps; iter++)
            {
                bool sObj = true;

                Vector3 cords = testPos - map.mapOrigin;
                float dst = map.distanceMap[(int)(cords.X/map.distanceMapDetail),
                                            (int)(cords.Y/map.distanceMapDetail),
                                            (int)(cords.Z/map.distanceMapDetail)];

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
                    float bestDst = 9999;
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
                    length = (position - testPos).Length();

                    Vector3 startPos;
                    float lightIntensity = 0;
                    foreach(Light light in map.lightList)
                    {
                        startPos = testPos+bestObj.SDF_normal(testPos)*2;

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

            float length = 1f;
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
