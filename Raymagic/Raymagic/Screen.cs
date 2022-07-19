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

        public bool DrawPhase {get; private set;}

        Stopwatch watch;

        private Screen() {} 

        public static readonly Screen instance = new Screen();

        public void Init(Graphics graphics, Point screenDimensions, int detailSize)
        {
            this.graphics = graphics;
            this.screenDimensions = screenDimensions;
            this.detailSize = detailSize;

            this.colors = new Color[screenDimensions.X/detailSize,screenDimensions.Y/detailSize];
            /* float[,] lengths = new float[screenDimensions.X/detailSize,screenDimensions.Y/detailSize]; */

            this.watch = new Stopwatch();
        }

        public void SetDetailSize(int detailSize)
        {
            if(detailSize != this.detailSize)
            {
                this.colors = new Color[screenDimensions.X/detailSize,screenDimensions.Y/detailSize];
                /* float[,] lengths = new float[screenDimensions.X/detailSize,screenDimensions.Y/detailSize]; */

                GC.Collect();
            }

            this.detailSize = detailSize;

        }

        Color[,] colors;

        public void DrawGame()
        {
            this.DrawPhase = true;
            RayMarchingPhase();
            Informer.instance.AddInfo("debug raysPhase", $" ray phase: {watch.ElapsedMilliseconds}");

            TileDrawPhase();
            CursorDrawPhase();
            Informer.instance.AddInfo("debug drawPhase", $" draw phase: {watch.ElapsedMilliseconds}");

            Informer.instance.AddInfo("details", $"details: {detailSize}");
            this.DrawPhase = false;
        }

        private void RayMarchingPhase()
        {
            watch.Restart();
            int zoom = 450;

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

                RayMarchingHelper.RayMarch(new Ray(player.position, rayDir), out float length, out Color Color);
                colors[x,y] = Color;
            }); 

            watch.Stop();
        }

        private void TileDrawPhase()
        {
            watch.Restart();

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

            watch.Stop();
        }
    }
}
