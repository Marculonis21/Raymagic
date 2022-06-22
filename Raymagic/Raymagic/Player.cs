using System;
using System.Collections.Generic;
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

        public bool GodMode = false;

        Map map = Map.instance;

        Dictionary<string, Keys> playerControls = new Dictionary<string, Keys>();

        private Player()
        {
            position = map.GetPlayerStart();
            rotation = new Vector2(270,120);
            size = new Vector2(25,75);

            playerControls.Add("forward_move",  Keys.W);
            playerControls.Add("backward_move", Keys.S);
            playerControls.Add("left_move",     Keys.A);
            playerControls.Add("right_move",    Keys.D);
            playerControls.Add("jump",          Keys.Space);
            playerControls.Add("playerMode",    Keys.F1);
            playerControls.Add("godMode",       Keys.F2);
            playerControls.Add("god_up",        Keys.Space);
            playerControls.Add("god_down",      Keys.LeftControl);

            playerControls.Add("TESTANYTHING_ON",  Keys.O);
            playerControls.Add("TESTANYTHING_OFF", Keys.P);
        }

        public static readonly Player instance = new Player();

        int lastMouseX = 100;
        int lastMouseY = 100;
        public void Controlls(GameTime gameTime, MouseState mouse)
        {
            if (Keyboard.GetState().IsKeyDown(playerControls["forward_move"])) 
                this.position += new Vector3((float)Math.Cos(this.rotation.X*Math.PI/180)*2,
                                             (float)Math.Sin(this.rotation.X*Math.PI/180)*2,
                                             0);
            if (Keyboard.GetState().IsKeyDown(playerControls["backward_move"])) 
                this.position -= new Vector3((float)Math.Cos(this.rotation.X*Math.PI/180)*2,
                                             (float)Math.Sin(this.rotation.X*Math.PI/180)*2,
                                             0);
            if (Keyboard.GetState().IsKeyDown(playerControls["left_move"]))
                this.position -= new Vector3((float)Math.Cos((90+this.rotation.X)*Math.PI/180)*2,
                                             (float)Math.Sin((90+this.rotation.X)*Math.PI/180)*2,
                                             0);
            if (Keyboard.GetState().IsKeyDown(playerControls["right_move"])) 
                this.position += new Vector3((float)Math.Cos((90+this.rotation.X)*Math.PI/180)*2,
                                             (float)Math.Sin((90+this.rotation.X)*Math.PI/180)*2,
                                             0);

            if (Keyboard.GetState().IsKeyDown(playerControls["jump"])) 
                this.Jump(gameTime);

            if (Keyboard.GetState().IsKeyDown(playerControls["godMode"])) 
                GodMode = true;
            if (Keyboard.GetState().IsKeyDown(playerControls["playerMode"])) 
                GodMode = false;


            if(GodMode)
            {
                if(Keyboard.GetState().IsKeyDown(playerControls["god_down"]))
                    this.position.Z -= 2f;
                if(Keyboard.GetState().IsKeyDown(playerControls["god_up"]))
                    this.position.Z += 2f;
            }

            /* if(Keyboard.GetState().IsKeyDown(playerControls["TESTANYTHING_ON"])) */
            /*     RayMarchingHelper.GammaCorrectionEnabled = true; */
            /* if(Keyboard.GetState().IsKeyDown(playerControls["TESTANYTHING_OFF"])) */
            /*     RayMarchingHelper.GammaCorrectionEnabled = false; */

            this.Rotate(new Vector2(mouse.X - lastMouseX, mouse.Y - lastMouseY));

            Mouse.SetPosition(100, 100);
        }

        public void Rotate(Vector2 rot)
        {
            // changes rotation speed
            rot /= 3;

            // 0.01 & 179.9 -> portals places straight down/up need to have set
            // up vector - from look dir
            this.rotation += rot;
            if(this.rotation.Y < 0.01)
                this.rotation.Y = 0.01f;
            if(this.rotation.Y > 179.9)
                this.rotation.Y = 179.9f;

            double R_inclination = rotation.Y*Math.PI/180f;
            double R_azimuth = rotation.X*Math.PI/180f;
            double _x = Math.Cos(R_azimuth)*Math.Sin(R_inclination); 
            double _y = Math.Sin(R_azimuth)*Math.Sin(R_inclination); 
            double _z = Math.Cos(R_inclination);

            lookDir = new Vector3((float)_x,(float)_y,(float)_z);
            lookDir.Normalize();

            Informer.instance.AddInfo("playerRot", "LookDir: " + lookDir.ToString());
        }

        public void Jump(GameTime gameTime)
        {
            if(GodMode) return; // disable jumping in godmode - GODS FLY
            if(this.velocity.Z == 0)
                this.velocity.Z += 0.5f*gameTime.ElapsedGameTime.Milliseconds;
        }

        public void Update(GameTime gameTime)
        {
            if(!GodMode)
            {
                FeetCollider(gameTime);
                BodyCollider(gameTime);
            }

            this.position += this.velocity;
            Informer.instance.AddInfo("playerPos", "Player POS: " + this.position.ToString());
            Informer.instance.AddInfo("playerFeet", "Player feet: " + (this.position + new Vector3(0,0,-1)*size.Y).ToString());
        }

        void BodyCollider(GameTime gameTime)
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

                Ray testRay = new Ray(this.position + new Vector3(0,0,-1)*size.Y/2, testDir);

                RayMarchingHelper.PhysicsRayMarch(testRay, 5, 0, out float width, out Vector3 hit, out Object obj); 

                if(width <= this.size.X/2)
                {
                    // pass through portals
                    if (obj == map.portalList[0] || obj == map.portalList[1]) return;

                    Vector3 normal = obj.SDF_normal(hit);

                    this.position += normal*3;
                    return;
                }
            }

            // maintain height above ground (stairs/steps) 
            RayMarchingHelper.PhysicsRayMarch(new Ray(this.position, new Vector3(0,0,-1)), 10, -1, out float length, out Vector3 _, out Object _);
            if(length < size.Y)
            {
                this.position += new Vector3(0,0,1)*(size.Y-length);
            }

        }

        void FeetCollider(GameTime gameTime)
        {
            // gravity
            Vector3 feetPos = this.position + new Vector3(0,0,-1)*size.Y;
            RayMarchingHelper.PhysicsRayMarch(new Ray(feetPos, new Vector3(0,0,-1)), 5, 0, out float length, out Vector3 _, out Object _);

            if(length > 0)
            {
                this.velocity += new Vector3(0,0,-1) * gravity*gameTime.ElapsedGameTime.Milliseconds;
            }
            else if(velocity.Z < 0)
                this.velocity.Z = 0;
        }

        public void GetViewPlaneVectors(out Vector3 viewPlaneUp, out Vector3 viewPlaneRight)
        {
            // Get player look dir vector
            Vector3 playerLookDir = lookDir;

            // Get player look dir perpendicular vectors (UP)
            double R_inclinPerpen = (rotation.Y+90)*Math.PI/180f;
            double R_azimPerpen = (rotation.X)*Math.PI/180f;
            double _x = Math.Cos(R_azimPerpen)*Math.Sin(R_inclinPerpen); 
            double _y = Math.Sin(R_azimPerpen)*Math.Sin(R_inclinPerpen); 
            double _z = Math.Cos(R_inclinPerpen);
            Vector3 playerLookPerpenUP = new Vector3((float)_x,(float)_y,(float)_z);

            // Cross product look dir with perpen to get normal of a plane ->
            // side perpen to the view dir
            Vector3 playerLookPerpenSIDE = Vector3.Cross(playerLookDir, playerLookPerpenUP);

            viewPlaneUp = Vector3.Normalize(playerLookPerpenUP);
            viewPlaneRight = Vector3.Normalize(playerLookPerpenSIDE);
        }
    }
}
