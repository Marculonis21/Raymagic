using System;
using System.Threading;
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
        const float airControll = 0.05f;
        const float jumpStrength = 11f;
        const float grabDistance = 90;
        public bool grounded = true;
        public PhysicsObject grabbing {get; private set;}

        public bool GodMode = false;
        Vector3 preGodPositionCache;

        bool interactButtonDown = false;
        bool grabButtonDown = false;

        bool activeModelButtonDown = false;

        bool testONButtonDown = false;
        bool testOFFButtonDown = false;

        bool pauseButtonDown = false;

        public bool playerPause = false;

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
            playerControls.Add("interact",      Keys.E);
            playerControls.Add("grab",          Keys.F);
            playerControls.Add("pause",         Keys.Escape);
            playerControls.Add("playerMode",    Keys.F1);
            playerControls.Add("godMode",       Keys.F2);
            playerControls.Add("activeModel",   Keys.F12);
            playerControls.Add("god_up",        Keys.Space);
            playerControls.Add("god_down",      Keys.LeftControl);

            playerControls.Add("TESTANYTHING_ON",  Keys.O);
            playerControls.Add("TESTANYTHING_OFF", Keys.P);

            this.model = new Capsule(new Vector3(400,400,0),
                                     size.Y/2, 
                                     size.X, 
                                     Color.Orange);
            Capsule topPart = new Capsule(new Vector3(0,0,-size.X),
                                          75/2,
                                          26,
                                          Color.White);

            topPart.AddChildObject(new Plane(new Vector3(0,0,75/4),
                                             new Vector3(0,0,-1),
                                             Color.Black,
                                             booleanOP: BooleanOP.INTERSECT), true);

            this.model.AddChildObject(topPart, true);

            map.portalableObjectList.Add(this);
        }

        public static readonly Player instance = new Player();

        int lastMouseX = 500;
        int lastMouseY = 500;
        public void Controlls(Game game, GameTime gameTime, MouseState mouse)
        {
            if (game.IsActive)
            {
                if(Keyboard.GetState().IsKeyDown(playerControls["pause"]))
                {
                    this.pauseButtonDown = true;
                }
                if(!Keyboard.GetState().IsKeyDown(playerControls["pause"]) && this.pauseButtonDown)
                {
                    this.pauseButtonDown = false;
                    this.playerPause = !this.playerPause;
                    game.IsMouseVisible = this.playerPause;
                }
            }

            if (!game.IsActive || playerPause) return;

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
                walkingVelocity *= airControll;
                this.velocity = new Vector3(velocity.X+walkingVelocity.X, velocity.Y+walkingVelocity.Y, velocity.Z);
            }

            this.Rotate(new Vector2(mouse.X - lastMouseX, mouse.Y - lastMouseY));

            // move model to "feet" position
            if (!GodMode)
            {
                model.TranslateAbsolute(this.position + new Vector3(0,0,-1)*(size.Y/2) + new Vector3(0,0,-1)*(size.X/2));
            }

            // KEYBOARD INTERACTS
            if (Keyboard.GetState().IsKeyDown(playerControls["jump"])) 
                this.Jump(gameTime);

            if (Keyboard.GetState().IsKeyDown(playerControls["interact"]) && !this.interactButtonDown)
            {
                this.interactButtonDown = true;
                Interactable.PlayerInteract(this.position);
            }
            if (Keyboard.GetState().IsKeyUp(playerControls["interact"]))
            {
                this.interactButtonDown = false;
            }

            if (Keyboard.GetState().IsKeyDown(playerControls["grab"]) && !this.grabButtonDown)
            {
                this.grabButtonDown = true;
                this.Grab();
            }
            if (Keyboard.GetState().IsKeyUp(playerControls["grab"]))
            {
                this.grabButtonDown = false;
            }

            if (Keyboard.GetState().IsKeyDown(playerControls["godMode"])) 
            {
                GodMode = true;
                this.preGodPositionCache = this.position;
                this.velocity = new Vector3(0,0,0);
            }
            if (Keyboard.GetState().IsKeyDown(playerControls["playerMode"])) 
            {
                GodMode = false;
                this.position = this.preGodPositionCache;
                this.velocity = new Vector3(0,0,0);
            }

            if (Keyboard.GetState().IsKeyDown(playerControls["activeModel"]) && !activeModelButtonDown)
            {
                activeModelButtonDown = true;

                map.activeMapReload = !map.activeMapReload;
                if (map.activeMapReload)
                {
                    map.MapReloadingAsync(500);
                }
            }
            if (!Keyboard.GetState().IsKeyDown(playerControls["activeModel"]))
            {
                activeModelButtonDown = false;
            }

            if(GodMode)
            {
                if(Keyboard.GetState().IsKeyDown(playerControls["god_down"]))
                    this.position += new Vector3(0,0,-2);
                if(Keyboard.GetState().IsKeyDown(playerControls["god_up"]))
                    this.position += new Vector3(0,0,+2);
            }

            if(Keyboard.GetState().IsKeyDown(playerControls["TESTANYTHING_ON"]) && !this.testONButtonDown)
            {
                this.testONButtonDown = true;
                Screen.instance.processingTest = true;

                // long running task? thread feels better - completely separated
                /* new Thread(() => map.PreLoadMap("loadTest2")).Start(); */
            }
            if(Keyboard.GetState().IsKeyDown(playerControls["TESTANYTHING_OFF"]) && !this.testOFFButtonDown)
            {
                this.testOFFButtonDown = true;
                Screen.instance.processingTest = false;
            }

            if (Keyboard.GetState().IsKeyUp(playerControls["TESTANYTHING_ON"]))
            {
                this.testONButtonDown = false;
            }
            if (Keyboard.GetState().IsKeyUp(playerControls["TESTANYTHING_OFF"]))
            {
                this.testOFFButtonDown = false;
            }


            PortalPlacement(mouse);

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

            if (grabbing != null)
            {
                var right = Vector3.Cross(lookDir, new Vector3(0,0,1));
                var up = new Vector3(0,0,1);
                grabbing.Rotate(-rot.X, up, this.position);

                if (grabbing.GetType() == typeof(MirrorBall))
                {
                    // DONT ASK ME WHY - Rotations are hard, forcing rotations is impossible
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

                    Vector3 gRot = grabbing.Rotation;
                    grabbing.Rotate(testAzimuth - gRot.Z, "z", grabbing.Position);
                    if (yDegree > 0)
                    {
                        grabbing.Rotate(gRot.Y, "x", grabbing.Position);
                    }
                    else
                    {
                        grabbing.Rotate(-gRot.Y, "x", grabbing.Position);
                    }
                    if ((float)Math.Abs(yDegree) > 45)
                    {
                        if (yDegree > 0)
                        {
                            grabbing.Rotate(gRot.X, "y", grabbing.Position);
                        }
                        else
                        {
                            grabbing.Rotate(-gRot.X, "y", grabbing.Position);
                        }
                    }
                    var mb = grabbing as MirrorBall;

                    mb.outDir = Vector3.Normalize(new Vector3(lookDir.X, lookDir.Y, 0));
                }
                else
                {
                    grabbing.Rotate(rot.Y, right, this.position);
                }
            }

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

        public void Grab()
        {
            if (grabbing == null)
            {
                foreach (var pObj in Map.instance.physicsObjectsList)
                {
                    if (pObj.isTrigger) continue;

                    if (Vector3.Distance(this.position, pObj.Position) <= grabDistance &&
                        Vector3.Dot(this.lookDir, Vector3.Normalize(pObj.position - this.position)) > 0.7f)
                    {
                        pObj.physicsEnabled = false;

                        grabbing = pObj;
                        break;
                    }
                }
            }
            else
            {
                Vector3 before = grabbing.Position;
                GrabbingUpdate();
                Vector3 after = grabbing.Position;
                grabbing.ClearVelocity();
                grabbing.ClearForces();
                grabbing.SetVelocity(this.velocity/4);
                grabbing.SetVelocity((after-before)/30);

                grabbing.physicsEnabled = true;
                grabbing = null;
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

            if (grabbing != null && !GodMode)
            {
                GrabbingUpdate();
            }

            Informer.instance.AddInfo("playerPos", "Player POS: " + this.position.ToString());
            Informer.instance.AddInfo("playerFeet", "Player feet: " + (this.position + new Vector3(0,0,-1)*size.Y).ToString());
            Informer.instance.AddInfo("playerVelocity", "Player velocity: " + this.velocity);
            Informer.instance.AddInfo("playerGroundedState", "Player grounded: " + this.grounded);
        }

        void GrabbingUpdate()
        {
            float startOffset = 45;
            Ray testRay = new Ray(this.position + this.lookDir * startOffset, this.lookDir);
            RayMarchingHelper.PhysicsRayMarch(testRay, 3, 10, out float dirLength, out Vector3 dirHit, out Object hitObj, caller:grabbing);

            Vector3 grabPosition;
            if (dirLength + startOffset <= grabDistance && Portal.HitObjectIsActivePortal(hitObj)) 
            {
                Ray moreRay = Portal.TransferRay((Portal)hitObj,testRay, dirHit);
                RayMarchingHelper.PhysicsRayMarch(moreRay, 3, 10, out float dirLengthPortal, out Vector3 dirHitPortal, out Object hitObjPortal, caller:grabbing);

                var grabDistanceLeft = grabDistance - (dirLength + startOffset);
                
                if (dirLengthPortal <= grabDistanceLeft)
                {
                    grabPosition = dirHitPortal;
                }
                else
                {
                    grabPosition = moreRay.origin + moreRay.direction * grabDistanceLeft;
                }
            }
            else if(dirLength + startOffset <= grabDistance)
            {
                grabPosition = this.position + this.lookDir * (dirLength + startOffset);
            }
            else
            {
                grabPosition = this.position + this.lookDir * grabDistance;
            }

            grabbing.TranslateAbsolute(grabPosition);
            grabbing.ClearVelocity();
        }

        void BodyCollider(GameTime gameTime)
        {
            float width;
            Vector3 hitPos;
            Object hitObj;
            Ray testRay = new Ray();

            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0: testRay = new Ray(this.position, new Vector3());                                  break;
                    case 1: testRay = new Ray(this.position + new Vector3(0,0,-1)*size.Y/4, new Vector3());   break;
                    case 2: testRay = new Ray(this.position + new Vector3(0,0,-1)*size.Y/2, new Vector3());   break;
                    case 3: testRay = new Ray(this.position + new Vector3(0,0,-1)*3*size.Y/4, new Vector3()); break;
                }

                RayMarchingHelper.PhysicsRayMarch(testRay, 1, size.X/2, out width, out hitPos, out hitObj, caller:this.model); 

                if (width <= this.size.X/2)
                {
                    if (Portal.HitObjectIsActivePortal(hitObj))  break;

                    Vector3 normal = hitObj.SDF_normal(hitPos);

                    this.position += normal*(this.size.X/2 - width);
                }
            }

            // maintain height above ground (stairs/steps) 
            RayMarchingHelper.PhysicsRayMarch(new Ray(this.position, new Vector3(0,0,-1)), 10, -1, out float length, out hitPos, out hitObj, caller:this.model);

            if ((length < size.Y) && 
                !(Portal.HitObjectIsActivePortal(hitObj)))
            {
                this.position += new Vector3(0,0,1)*(size.Y-length);
            }
        }

        public Vector3 feetPos;
        public Vector3 capsuleBotPos;
        void FeetCollider(GameTime gameTime)
        {
            Vector3 headPos = this.position;
            this.feetPos = headPos + new Vector3(0,0,-1)*size.Y;
            this.capsuleBotPos = feetPos + new Vector3(0,0,1)*size.X/2;

            // check from head down - size of the body, if collides with portal let him fall
            RayMarchingHelper.PhysicsRayMarch(new Ray(headPos, new Vector3(0,0,-1)), 5, size.X/2, out float fromHeadLen, out Vector3 _, out Object hObj, caller:this.model);

            //check directly under player feet
            RayMarchingHelper.PhysicsRayMarch(new Ray(feetPos, new Vector3(0,0,-1)), 1, -1, out float fromFeetLen, out Vector3 _, out Object fObj, caller:this.model);

            RayMarchingHelper.PhysicsRayMarch(new Ray(capsuleBotPos, new Vector3(0,0,-1)), 1, -1, out float fromCapsBot, out Vector3 _, out Object cObj, caller:this.model);

            if ((fromFeetLen > 0 && fromCapsBot > size.X/2) ||
                (fromFeetLen > size.X/4 && fromCapsBot < size.X/2) ||
                (fromFeetLen < 0 && Portal.HitObjectIsActivePortal(hObj)))
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

        bool lPressed;
        bool rPressed;
        void PortalPlacement(MouseState mouse)
        {
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                lPressed = true;
            }
            if (mouse.RightButton == ButtonState.Pressed)
            {
                rPressed = true;
            }

            if ((mouse.LeftButton == ButtonState.Released && lPressed) || (mouse.RightButton == ButtonState.Released && rPressed))
            {
                RayMarchingHelper.PhysicsRayMarch(new Ray(this.position, this.lookDir), 300, 0, out float length, out Vector3 hit, out Object hitObj, caller:this.model);
                if (hitObj.IsSelectable || (hitObj == map.portalList[0] && lPressed) || (hitObj == map.portalList[1] && rPressed))
                {
                    if (hitObj == map.portalList[0] || hitObj == map.portalList[1])
                    {
                        hit += hitObj.SDF_normal(hit)*-2;
                    }
                    int type = lPressed ? 0 : 1;
                    map.portalList[type] = (new Portal(hit, hitObj.SDF_normal(hit), type));
                }
                lPressed = rPressed = false;
            }
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
