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

        Map map = Map.instance;

        private Player()
        {
            position = map.GetPlayerStart();
            rotation = new Vector2(75,90);

            xFOV = 90;
            yFOV = 90;
        }

        public static readonly Player instance = new Player();
    }
}
