using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Screen
    {
        //SINGLETON
        Graphics graphics;
        Player player = Player.instance;

        Point screenDimensions;
        int detailSize;

        Stopwatch watch;

        private Screen() {} 

        public static readonly Screen instance = new Screen();

        public void Init(Graphics graphics, Point screenDimensions, int detailSize)
        {
            this.graphics = graphics;
            this.screenDimensions = screenDimensions;
            this.detailSize = detailSize;

            this.watch = new Stopwatch();
        }

        public void SetDetailSize(int detailSize)
        {
            this.detailSize = detailSize;
        }

        Color[,] colors;
        public void DrawGame(MainGame game)
        {
            RayMarchingPhase(game);
            Informer.instance.AddInfo("debug rays", $" ray phase: {watch.ElapsedMilliseconds}");
            watch.Reset();

            TileDrawPhase();
            watch.Reset();
            Informer.instance.AddInfo("debug draw", $" draw phase: {watch.ElapsedMilliseconds}");

            CursorDrawPhase();

            Informer.instance.AddInfo("details", $"details: {detailSize}");
        }

        private void RayMarchingPhase(MainGame game)
        {
            watch.Start();

            int zoom = 450;

            colors = new Color[screenDimensions.X/detailSize,screenDimensions.Y/detailSize];
            /* float[,] lengths = new float[screenDimensions.X/detailSize,screenDimensions.Y/detailSize]; */

            player.GetViewPlaneVectors(out Vector3 viewPlaneUp, out Vector3 viewPlaneRight);
            
            // Parallel RAYMARCHING!!!
            Parallel.For(0, (screenDimensions.Y/detailSize) * (screenDimensions.X/detailSize),new ParallelOptions{ MaxDegreeOfParallelism = Environment.ProcessorCount}, i =>
            {
                int y = i / (screenDimensions.X/detailSize);
                int x = i % (screenDimensions.X/detailSize);

                // get ray dir from camera through view plane (detailSize) "rectangles"
                float _x = x - (screenDimensions.X/detailSize)/2;
                float _y = y - (screenDimensions.Y/detailSize)/2;

                Vector3 rayDir = (player.position + player.lookDir*zoom + viewPlaneRight*_x*detailSize + viewPlaneUp*_y*detailSize) - player.position;

                /* colors[x,y] = RayMarching.Rendering(new Ray(player.position, rayDir), maxSteps:100); */

                game.RayMarch(player.position, rayDir, out float length, out Color Color);
                colors[x,y] = Color;
            }); 

            watch.Stop();
        }

        private void TileDrawPhase()
        {
            graphics.Begin();

            for(int y = 0; y < (screenDimensions.Y/detailSize); y++)
                for(int x = 0; x < (screenDimensions.X/detailSize); x++)
                {
                    graphics.DrawRectangle(new Point(x*detailSize,y*detailSize), 
                                           detailSize,detailSize, 
                                           colors[x,y]);
                }
        }

        private void CursorDrawPhase()
        {
            graphics.DrawLine(new Point(screenDimensions.X/2,screenDimensions.Y/2-player.cursorSize), 
                              new Point(screenDimensions.X/2,screenDimensions.Y/2+player.cursorSize), 
                              5, 
                              Color.Gold);

            graphics.DrawLine(new Point(screenDimensions.X/2-player.cursorSize,screenDimensions.Y/2), 
                              new Point(screenDimensions.X/2+player.cursorSize,screenDimensions.Y/2), 
                              5, 
                              Color.Gold);

            graphics.End();
        }
    }
}
