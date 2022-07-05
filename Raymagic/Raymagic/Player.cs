using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Raymagic
{
    public class Player : IPortalable
    {
        //SINGLETON
        public Vector3 position {get; private set;}
        public Vector2 rotation;
        public Vector3 velocity {get; private set;}

        Vector2 size;
        public Object model {get; private set;}

        public Vector3 lookDir {get; private set;}

        const float walkingSpeed = 2.5f;
        float jumpStrength = 11f;
        bool grounded = true;

        public bool GodMode = false;
        Vector3 preGodPositionCache;

        /* const float gravity = 0.05f; */
        public int cursorSize = 10;

        Map map = Map.instance;

        Dictionary<string, Keys> playerControls = new Dictionary<string, Keys>();

        private Player()
        {
            position = map.GetPlayerStart();
            rotation = new Vector2(270,120);
            size = new Vector2(25,75);

            /* model = new PlayerModel(size.Y, size.X); */

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

            this.model = new Capsule(new Vector3(400,400,0),
                                     size.Y/2, 
                                     size.X, 
                                     Color.Orange,
                                     false);
            Capsule topPart = new Capsule(new Vector3(0,0,-size.X),
                                          75/2,
                                          26,
                                          Color.White,
                                          false);

            topPart.AddChildObject(new Plane(new Vector3(0,0,75/4),
                                             new Vector3(0,0,-1),
                                             Color.Black,
                                             booleanOP: BooleanOP.INTERSECT), true);

            this.model.AddChildObject(topPart, true);
        }

        public static readonly Player instance = new Player();

        int lastMouseX = 500;
        int lastMouseY = 500;
        public void Controlls(GameTime gameTime, MouseState mouse)
        {
            Vector2 walkingVelocity = new Vector2();
            if (Keyboard.GetState().IsKeyDown(playerControls["forward_move"])) 
                walkingVelocity += new Vector2((float)Math.Cos(this.rotation.X*Math.PI/180)*2,
                                               (float)Math.Sin(this.rotation.X*Math.PI/180)*2);

            if (Keyboard.GetState().IsKeyDown(playerControls["backward_move"])) 
                walkingVelocity -= new Vector2((float)Math.Cos(this.rotation.X*Math.PI/180)*2,
                                               (float)Math.Sin(this.rotation.X*Math.PI/180)*2);
                                             
            if (Keyboard.GetState().IsKeyDown(playerControls["left_move"]))
                walkingVelocity -= new Vector2((float)Math.Cos((90+this.rotation.X)*Math.PI/180)*2,
                                               (float)Math.Sin((90+this.rotation.X)*Math.PI/180)*2);

            if (Keyboard.GetState().IsKeyDown(playerControls["right_move"])) 
                walkingVelocity += new Vector2((float)Math.Cos((90+this.rotation.X)*Math.PI/180)*2,
                                               (float)Math.Sin((90+this.rotation.X)*Math.PI/180)*2);

            if (walkingVelocity.X != 0 && walkingVelocity.Y != 0)
            {
                walkingVelocity = Vector2.Normalize(walkingVelocity);
            }

            if (grounded)
            {
                walkingVelocity *= walkingSpeed;
                this.velocity = new Vector3(walkingVelocity.X, walkingVelocity.Y, velocity.Z);
            }
            else
            {
                walkingVelocity *= 0.1f;
                this.velocity = new Vector3(velocity.X+walkingVelocity.X, velocity.Y+walkingVelocity.Y, velocity.Z);
            }

            // move model to "feet" position
            if (!GodMode)
            {
                model.TranslateAbsolute(this.position + new Vector3(0,0,-1)*(size.Y/2) + new Vector3(0,0,-1)*(size.X/2));
            }

            if (Keyboard.GetState().IsKeyDown(playerControls["jump"])) 
                this.Jump(gameTime);

            if (Keyboard.GetState().IsKeyDown(playerControls["godMode"])) 
            {
                GodMode = true;
                this.preGodPositionCache = this.position;
            }
            if (Keyboard.GetState().IsKeyDown(playerControls["playerMode"])) 
            {
                GodMode = false;
                this.position = this.preGodPositionCache;
            }


            if(GodMode)
            {
                if(Keyboard.GetState().IsKeyDown(playerControls["god_down"]))
                    this.position += new Vector3(0,0,-2);
                if(Keyboard.GetState().IsKeyDown(playerControls["god_up"]))
                    this.position += new Vector3(0,0,+2);
            }

            if(Keyboard.GetState().IsKeyDown(playerControls["TESTANYTHING_ON"]))
            {
                map.enabledUpdate = true;
            }
            if(Keyboard.GetState().IsKeyDown(playerControls["TESTANYTHING_OFF"]))
            {
                map.enabledUpdate = false;
            }

            this.Rotate(new Vector2(mouse.X - lastMouseX, mouse.Y - lastMouseY));

            Mouse.SetPosition(500, 500);
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

            while (this.rotation.X > 360)
            {
                this.rotation.X -= 360;
            }
            while (this.rotation.X < 0)
            {
                this.rotation.X += 360;
            }

            double R_inclination = rotation.Y*Math.PI/180f;
            double R_azimuth = rotation.X*Math.PI/180f;
            double _x = Math.Cos(R_azimuth)*Math.Sin(R_inclination); 
            double _y = Math.Sin(R_azimuth)*Math.Sin(R_inclination); 
            double _z = Math.Cos(R_inclination);

            lookDir = new Vector3((float)_x,(float)_y,(float)_z);
            lookDir.Normalize();

            Informer.instance.AddInfo("playerRot", "LookDir: " + lookDir.ToString());
        }

        public void RotateAbsolute(Vector3 rot)
        {
            this.lookDir = Vector3.Normalize(rot);

            Vector2 v = Vector2.Normalize(new Vector2(lookDir.X, lookDir.Y));
            var xDegree = Math.Acos(v.X)*180/Math.PI;
            var yDegree = Math.Asin(v.Y)*180/Math.PI;

            float testAzimuth = 0;
            if (yDegree > 0)
            {
                testAzimuth = (float)xDegree;
            }
            else
            {
                testAzimuth = 360 - (float)xDegree;
            }

            var RI = Math.Acos(lookDir.Z);

            this.rotation = new Vector2(testAzimuth, (float)(RI*180/Math.PI));
        }

        public void TranslateAbsolute(Vector3 newPosition)
        {
            this.position = newPosition;
        }

        public void SetVelocity(Vector3 newVelocity)
        {
            this.velocity = newVelocity;
        }

        public void Jump(GameTime gameTime)
        {
            if(GodMode) return; // disable jumping in godmode - GODS FLY
            if(grounded)
            {
                grounded = false;
                this.velocity += new Vector3(0,0,1)*jumpStrength;
            }
        }

        public void Update(GameTime gameTime)
        {
            if(!GodMode)
            {
                FeetCollider(gameTime);
                BodyCollider(gameTime);
            }

            if (this.velocity.Z < -20)
            {
                this.velocity = new Vector3(this.velocity.X, this.velocity.Y, -20);
            }
            if (this.velocity.Z > 20)
            {
                this.velocity = new Vector3(this.velocity.X, this.velocity.Y, 20);
            }

            this.position += this.velocity;

            Informer.instance.AddInfo("playerPos", "Player POS: " + this.position.ToString());
            Informer.instance.AddInfo("playerFeet", "Player feet: " + (this.position + new Vector3(0,0,-1)*size.Y).ToString());
            Informer.instance.AddInfo("playerVelocity", "Player velocity: " + this.velocity);
            Informer.instance.AddInfo("playerGroundedState", "Player grounded: " + this.grounded);
        }

        void BodyCollider(GameTime gameTime)
        {
            float width;
            Vector3 hitPos;
            Object hitObj;
            Ray testRay = new Ray();
            bool collisionFound = false;

            for (int i = 0; i < 4; i++)
            {
                do
                {
                    switch (i)
                    {
                        case 0: testRay = new Ray(this.position, new Vector3());                                  break;
                        case 1: testRay = new Ray(this.position + new Vector3(0,0,-1)*size.Y/4, new Vector3());   break;
                        case 2: testRay = new Ray(this.position + new Vector3(0,0,-1)*size.Y/2, new Vector3());   break;
                        case 3: testRay = new Ray(this.position + new Vector3(0,0,-1)*3*size.Y/4, new Vector3()); break;
                    }

                    collisionFound = false;
                    RayMarchingHelper.PhysicsRayMarch(testRay, 1, size.X/2, out width, out hitPos, out hitObj, caller:this.model); 

                    if (width <= this.size.X/2)
                    {
                        if ((hitObj == map.portalList[0] && map.portalList[0].otherPortal != null) || 
                            (hitObj == map.portalList[1] && map.portalList[1].otherPortal != null))  break;

                        Vector3 normal = hitObj.SDF_normal(hitPos);

                        // walkingSpeed/# of testRays
                        this.position += normal/2;
                        collisionFound = true;
                        /* Console.WriteLine("casd"); */
                    }
                    
                } while (collisionFound);
            }

            // maintain height above ground (stairs/steps) 
            RayMarchingHelper.PhysicsRayMarch(new Ray(this.position, new Vector3(0,0,-1)), 10, -1, out float length, out hitPos, out hitObj, caller:this.model);

            if ((length < size.Y) && 
                !((hitObj == map.portalList[0] || hitObj == map.portalList[1]) && ((Portal)hitObj).otherPortal != null))
            {
                this.position += new Vector3(0,0,1)*(size.Y-length);
            }
        }

        /* void BodyCollider(GameTime gameTime) */
        /* { */
        /*     // body side collider */
        /*     double angle; */
        /*     Vector3 testDir; */
        /*     double R_azimuth = rotation.X*Math.PI/180f; */
        /*     Object obj; */
        /*     for (int i = 0; i < 12; i++) */
        /*     { */
        /*         angle = 2*Math.PI/12 * i; */
        /*         testDir = new Vector3((float)Math.Cos(R_azimuth+angle), (float)Math.Sin(R_azimuth+angle), 0); */
        /*         testDir.Normalize(); */

        /*         Ray testRay = new Ray(this.position + new Vector3(0,0,-1)*size.Y/2, testDir); */

        /*         RayMarchingHelper.PhysicsRayMarch(testRay, 5, 0, out float width, out Vector3 hit, out obj); */ 

        /*         if(width <= this.size.X/2) */
        /*         { */
        /*             // pass through portals */
        /*             if ((obj == map.portalList[0] || obj == map.portalList[1]) && ((Portal)obj).otherPortal != null)return; */

        /*             Vector3 normal = obj.SDF_normal(hit); */

        /*             this.position += normal*3; */
        /*             return; */
        /*         } */
        /*     } */

        /*     // maintain height above ground (stairs/steps) */ 
        /*     RayMarchingHelper.PhysicsRayMarch(new Ray(this.position, new Vector3(0,0,-1)), 10, -1, out float length, out Vector3 _, out obj); */
        /*     if ((length < size.Y) && !((obj == map.portalList[0] || obj == map.portalList[1]) && ((Portal)obj).otherPortal != null)) */
        /*     { */
        /*         this.position += new Vector3(0,0,1)*(size.Y-length); */
        /*     } */

        /* } */

        void FeetCollider(GameTime gameTime)
        {
            Vector3 feetPos = this.position + new Vector3(0,0,-1)*size.Y;
            //check if the bottom part of the player capsule is touching the ground
            RayMarchingHelper.PhysicsRayMarch(new Ray(feetPos + new Vector3(0,0,1)*size.X/2, new Vector3(0,0,-1)), 1, -1, out float capsuleLength, out Vector3 _, out Object capsObj, caller:this.model);

            //check directly under player feet
            RayMarchingHelper.PhysicsRayMarch(new Ray(feetPos + new Vector3(0,0,1), new Vector3(0,0,-1)), 2, -1, out float downLength, out Vector3 hit, out Object obj, caller:this.model);

            if ((capsuleLength > size.X/2 || downLength > size.X/4) ||
               ((capsObj == map.portalList[0] && map.portalList[0].otherPortal != null) || 
                (capsObj == map.portalList[1] && map.portalList[1].otherPortal != null)))  
            {
                grounded = false;
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                this.velocity += new Vector3(0,0,-1) * Map.instance.gravity * dt * dt; // falls at the same speed as physics objects
            }
            else if (this.velocity.Z < 0)
            {
                grounded = true;
                this.velocity = new Vector3(this.velocity.X, this.velocity.Y, 0);
            }
        }

/*         void FeetCollider(GameTime gameTime) */
/*         { */
/*             Vector3 feetPos = this.position + new Vector3(0,0,-1)*size.Y; */
/*             RayMarchingHelper.PhysicsRayMarch(new Ray(feetPos, new Vector3(0,0,-1)), 2, 0, out float length, out Vector3 hit, out Object obj); */

/*             // gravity */
/*             if ((length > 0) || ((obj == map.portalList[0] || obj == map.portalList[1]) && ((Portal)obj).otherPortal != null) || */
/*                 (map.portalList[0] != null && map.portalList[0].PosInPortal(feetPos)) || (map.portalList[1] != null && map.portalList[1].PosInPortal(feetPos))) */
/*             { */
/*                 this.velocity += new Vector3(0,0,-1) * gravity*gameTime.ElapsedGameTime.Milliseconds; */
/*                 grounded = false; */
/*             } */
/*             else if(!grounded) */
/*             { */
/*                 Console.WriteLine("happended"); */
/*                 grounded = true; */
/*                 this.velocity = new Vector3(this.velocity.X, this.velocity.Y, 0); */
/*             } */
/*         } */

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
