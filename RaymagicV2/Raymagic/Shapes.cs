using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class Shapes : IDisposable
    {
        MainGame game;

        //rects drawing
        Point origin;

        BasicEffect effect;
        bool isDisposed;

        VertexPositionColor[] vertices;
        int[] indeces;

        int shapeCount;
        int vertexCount;
        int indexCount;

        bool isStarted;

        //text drawing
        SpriteBatch spriteBatch;

        public Shapes(MainGame game, Point originTranslate, SpriteBatch spriteBatch)
        {
            this.game = game;
            this.origin = originTranslate;
            this.spriteBatch = spriteBatch;

            this.isDisposed = false;

            this.effect = new BasicEffect(this.game.GraphicsDevice);
            this.effect.TextureEnabled = false;
            this.effect.FogEnabled = false;
            this.effect.LightingEnabled = false;
            this.effect.VertexColorEnabled = true;
            this.effect.World = Matrix.Identity;
            this.effect.View = Matrix.Identity;
            this.effect.Projection = Matrix.Identity;

            const int MaxVertexCount = 2048;
            const int MaxIndexCount = MaxVertexCount * 3;

            this.vertices = new VertexPositionColor[MaxVertexCount];
            this.indeces = new int[MaxIndexCount];

            this.shapeCount = 0;
            this.vertexCount = 0; this.indexCount = 0;

            this.isStarted = false;
        }

        public void Dispose()
        {
            if(this.isDisposed)
            {
                return;
            }

            this.effect.Dispose();
            this.isDisposed = true;
        }

        public void Begin()
        {
            this.isStarted = true;

            //coordinate system
            Viewport view = this.game.GraphicsDevice.Viewport;
            this.effect.Projection = Matrix.CreateOrthographicOffCenter(0, view.Width, 0, view.Height, 0f, 1f);
        }
        public void End()
        {
            this.Flush();
            this.isStarted = false;
        }
        public void Flush()
        {
            if (this.shapeCount == 0)
            {
                return;
            }

            this.TestStarted();

            foreach(EffectPass pass in this.effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                //send that shit to gpu
                this.game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                        PrimitiveType.TriangleList,
                        this.vertices,
                        0,
                        this.vertexCount,
                        this.indeces,
                        0,
                        this.indexCount/3);
            }

            this.shapeCount = 0;
            this.vertexCount = 0;
            this.indexCount = 0;

        }

        public void TestStarted()
        {
            if(!this.isStarted) throw new Exception("batch was not started");
        }

        public void TestSpace(int shapeVertexCount, int shapeIndexCount)
        {
            if(shapeVertexCount > this.vertices.Length)
            {
                throw new Exception("maximum shape vertex count is "+ this.vertices.Length);
            }
            if(shapeIndexCount > this.indeces.Length)
            {
                throw new Exception("maximum shape index count is "+ this.indeces.Length);
            }

            if((this.vertexCount + shapeVertexCount >= this.vertices.Length) || 
               (this.indexCount + shapeIndexCount >= this.indeces.Length))
            {
                this.Flush();
            }

        }

        public void DrawRectangle(Point position, int width, int height, Color color)
        {
            this.TestStarted();
            const int shapeVertexCount = 4;
            const int shapeIndexCount = 6;
            this.TestSpace(shapeVertexCount, shapeIndexCount);

            int x = position.X;
            int y = position.Y;

            y = (int)origin.Y - y;

            int left = x;
            int right = x + width;
            int bottom = y-height;
            int top = y;

            Point a = new Point(left,top);
            Point b = new Point(right,top);
            Point c = new Point(right,bottom);
            Point d = new Point(left,bottom);

            this.indeces[this.indexCount++] = 0 + this.vertexCount;
            this.indeces[this.indexCount++] = 1 + this.vertexCount;
            this.indeces[this.indexCount++] = 2 + this.vertexCount;
            
            this.indeces[this.indexCount++] = 0 + this.vertexCount;
            this.indeces[this.indexCount++] = 2 + this.vertexCount;
            this.indeces[this.indexCount++] = 3 + this.vertexCount;

            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(a.ToVector2(), 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(b.ToVector2(), 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(c.ToVector2(), 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(d.ToVector2(), 0f), color);
            this.shapeCount++;
        }

        public void DrawBorder(Point position, int width, int height, int borderWidth, Color borderColor)
        {
            Point a = position;
            Point b = new Point(position.X, position.Y + height);
            Point c = new Point(position.X + width, position.Y + height);
            Point d = new Point(position.X + width, position.Y);

            DrawLine(a, b, borderWidth, borderColor);
            DrawLine(b, c, borderWidth, borderColor);
            DrawLine(c, d, borderWidth, borderColor);
            DrawLine(d, a, borderWidth, borderColor);
        }

        public void DrawLine(Point from, Point to, float thickness, Color color)
        {
            this.TestStarted();
            const int shapeVertexCount = 4;
            const int shapeIndexCount = 6;
            this.TestSpace(shapeVertexCount, shapeIndexCount);

            Vector2 a = from.ToVector2();
            Vector2 b = to.ToVector2();

            float halfThickness = thickness/2;

            a.Y = origin.Y - a.Y;
            b.Y = origin.Y - b.Y;

            Vector2 e1 = b-a;
            e1.Normalize();
            e1 *= halfThickness;

            Vector2 e2 = -e1;
            Vector2 n1 = new Vector2(-e1.Y, e1.X);
            Vector2 n2 = -n1;

            Vector2 q1 = a + n1 + e2;
            Vector2 q2 = b + n1 + e1;
            Vector2 q3 = b + n2 + e1;
            Vector2 q4 = a + n2 + e2;

            this.indeces[this.indexCount++] = 0 + this.vertexCount;
            this.indeces[this.indexCount++] = 1 + this.vertexCount;
            this.indeces[this.indexCount++] = 2 + this.vertexCount;
            
            this.indeces[this.indexCount++] = 0 + this.vertexCount;
            this.indeces[this.indexCount++] = 2 + this.vertexCount;
            this.indeces[this.indexCount++] = 3 + this.vertexCount;

            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(q1, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(q2, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(q3, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(q4, 0f), color);
            this.shapeCount++;
        }

        public void DrawText(string text, SpriteFont font, Vector2 position, Color color, int anchorX = 1, int anchorY = 1)
        {
            //anchor = 0 left, 1 center, 2 right
            
            Vector2 stringSize = font.MeasureString(text); // center alligned texts
            Vector2 allignedPosition = new Vector2();

            if(anchorX == 0)
                allignedPosition.X = position.X;
            else if(anchorX == 1)
                allignedPosition.X = position.X - stringSize.X/2;
            else
                allignedPosition.X = position.X - stringSize.X;

            if(anchorY == 0)
                allignedPosition.Y = position.Y;
            else if(anchorY == 1)
                allignedPosition.Y = position.Y - stringSize.Y/2;
            else
                allignedPosition.Y = position.Y - stringSize.Y;
            

            spriteBatch.Begin();
            spriteBatch.DrawString(font, text, allignedPosition, color);
            spriteBatch.End();
        }
    }
}
