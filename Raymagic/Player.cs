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

        public float xFOV;
        public float yFOV;

        float speed;

        Map map = Map.instance;

        private Player()
        {
            position = map.GetPlayerStart();
            rotation = new Vector2(75,90);

            xFOV = 70;
            yFOV = 75;
        }

        public static readonly Player instance = new Player();

        public void Rotate(Vector2 rot)
        {
            rot /= 3;
            this.rotation += rot;
            if(this.rotation.Y < 50)
                this.rotation.Y = 50;
            if(this.rotation.Y > 135)
                this.rotation.Y = 135;
        }
    }
}
