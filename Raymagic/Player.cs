using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Raymagic
{
    public class Player 
    {
        //SINGLETON
        public Vector3 position;
        public Vector2 rotation;
        public Vector3 velocity;

        Vector2 size;

        float gravity = 0.05f;

        public int cursorSize = 10;

        public Vector3 lookDir {get; private set;}

        Map map = Map.instance;

        private Player()
        {
            position = map.GetPlayerStart();
            rotation = new Vector2(270,120);
            size = new Vector2(30,75);
        }

        public static readonly Player instance = new Player();

        int lastMouseX = 200;
        int lastMouseY = 200;
        public void Controlls(GameTime gameTime, MouseState mouse)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.W)) 
                this.position += new Vector3((float)Math.Cos(this.rotation.X*Math.PI/180)*2,(float)Math.Sin(this.rotation.X*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.S)) 
                this.position -= new Vector3((float)Math.Cos(this.rotation.X*Math.PI/180)*2,(float)Math.Sin(this.rotation.X*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.A))
                this.position -= new Vector3((float)Math.Cos((90+this.rotation.X)*Math.PI/180)*2,(float)Math.Sin((90+this.rotation.X)*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.D)) 
                this.position += new Vector3((float)Math.Cos((90+this.rotation.X)*Math.PI/180)*2,(float)Math.Sin((90+this.rotation.X)*Math.PI/180)*2,0);

            if (Keyboard.GetState().IsKeyDown(Keys.Space)) 
                this.Jump(gameTime);

            if(lastMouseX != -1)
                this.Rotate(new Vector2(mouse.X - lastMouseX, mouse.Y - lastMouseY));

            Mouse.SetPosition(200,200);
            lastMouseX = 200;
            lastMouseY = 200;
        }

        public void Rotate(Vector2 rot)
        {
            rot /= 3;
            this.rotation += rot;
            if(this.rotation.Y < 40)
                this.rotation.Y = 40;
            if(this.rotation.Y > 140)
                this.rotation.Y = 140;

            double R_inclination = rotation.Y*Math.PI/180f;
            double R_azimuth = rotation.X*Math.PI/180f;
            double _x = Math.Cos(R_azimuth)*Math.Sin(R_inclination); 
            double _y = Math.Sin(R_azimuth)*Math.Sin(R_inclination); 
            double _z = Math.Cos(R_inclination);

            lookDir = new Vector3((float)_x,(float)_y,(float)_z);
            lookDir.Normalize();

            Informer.instance.AddInfo("playerRot", lookDir.ToString());
        }

        public void Jump(GameTime gameTime)
        {
            if(this.velocity.Z == 0)
                this.velocity.Z += 0.5f*gameTime.ElapsedGameTime.Milliseconds;
        }

        public void Update(MainGame game, GameTime gameTime)
        {
            FeetCollider(game, gameTime);
            BodyCollider(game, gameTime);

            this.position += this.velocity;
            Informer.instance.AddInfo("playerPos", this.position.ToString());
            Informer.instance.AddInfo("playerFeet", (this.position + new Vector3(0,0,-1)*size.Y).ToString());
        }

        public bool DynamicObjectOcclusionCulling(Object dObj)
        {
            Vector3 dir = dObj.Position - this.position;
            dir.Normalize();

            return ((this.lookDir - dir).Length() < 1.5f);
        }

        void BodyCollider(MainGame game, GameTime gameTime)
        {
            // body side collider
            double angle;
            Vector3 testDir;
            double R_azimuth = rotation.X*Math.PI/180f;
            for (int i = 0; i < 12; i++)
            {
                angle = 2*Math.PI/12 * i;
                testDir = new Vector3((float)Math.Cos(R_azimuth+angle), (float)Math.Sin(R_azimuth+angle), 0);
                testDir.Normalize();

                game.PhysicsRayMarch(this.position + new Vector3(0,0,-1)*size.Y/2, testDir, 5, 0, out float width, out Vector3 hit, out Object obj); 

                if(width <= size.X/2)
                {
                    Vector3 normal = obj.SDF_normal(hit);

                    this.position += normal*3;
                    return;
                }
            }

            // maintain height above ground (stairs/steps) 
            game.PhysicsRayMarch(this.position, new Vector3(0,0,-1), 10, -1, out float length, out Vector3 _, out Object _);
            if(length < size.Y)
            {
                this.position += new Vector3(0,0,1)*(size.Y-length);
            }

        }

        void FeetCollider(MainGame game, GameTime gameTime)
        {
            // fall
            Vector3 feetPos = this.position + new Vector3(0,0,-1)*size.Y;
            game.PhysicsRayMarch(feetPos, new Vector3(0,0,-1), 5, 0, out float length, out Vector3 _, out Object _);

            if(length > 0)
            {
                this.velocity += new Vector3(0,0,-1) * gravity*gameTime.ElapsedGameTime.Milliseconds;
            }
            else if(velocity.Z < 0)
                this.velocity.Z = 0;
        }
    }
}
