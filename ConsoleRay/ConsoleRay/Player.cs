using System;
using Microsoft.Xna.Framework;

namespace ConsoleRay
{
    public class Player 
    {
        //SINGLETON
        public Vector3 position;
        public Vector2 rotation;

        public Vector3 lookDir {get; private set;}

        private Player()
        {
            position = new Vector3();
            rotation = new Vector2(0, 90);

            double R_inclination = rotation.Y*Math.PI/180f;
            double R_azimuth = rotation.X*Math.PI/180f;
            double _x = Math.Cos(R_azimuth)*Math.Sin(R_inclination); 
            double _y = Math.Sin(R_azimuth)*Math.Sin(R_inclination); 
            double _z = Math.Cos(R_inclination);

            lookDir = new Vector3((float)_x,(float)_y,(float)_z);
            lookDir.Normalize();

        }

        public static readonly Player instance = new Player();

    }
}
