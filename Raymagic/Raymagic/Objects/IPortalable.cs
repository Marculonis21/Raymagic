using Microsoft.Xna.Framework;

namespace Raymagic
{
    public interface IPortalable 
    {
        public Vector3 lookDir {get;}
        public Vector3 position {get;}

        public void RotateAbsolute(Vector3 newLookDir);
        public void TranslateAbsolute(Vector3 newPosition);
    }
}