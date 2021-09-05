using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raymagic
{
    public class Player 
    {
        //SINGLETON
        public Vector3 position;
        public Vector2 rotation;

        public int cursorSize = 8;

        Map map = Map.instance;

        private Player()
        {
            position = map.GetPlayerStart();
            rotation = new Vector2(75,90);
        }

        public static readonly Player instance = new Player();

        public void Rotate(Vector2 rot)
        {
            rot /= 3;
            this.rotation += rot;
            if(this.rotation.Y < 40)
                this.rotation.Y = 40;
            if(this.rotation.Y > 140)
                this.rotation.Y = 140;
        }
    }
}
