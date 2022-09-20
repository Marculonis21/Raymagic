using Microsoft.Xna.Framework;

namespace Raymagic
{
    public class Light
    {
        public Vector3 position {get; set;}
        public float intensity {get; private set;}
        public Color color {get; private set;}
        Vector3 lightZoneTopCorner;
        Vector3 lightZoneBotCorner;

        public Light(Vector3 position, Color color, float intensity, Vector3 lightZoneBotCorner, Vector3 lightZoneTopCorner)
        {
            this.position = position;
            this.intensity = intensity;
            this.color = color;
            this.lightZoneBotCorner = lightZoneBotCorner;
            this.lightZoneTopCorner = lightZoneTopCorner;
        }

        public float DistanceFrom(Vector3 testPos)
        {
            return SDFs.Point(testPos, position);
        }

        public bool IsPosInZone(Vector3 position)
        {
            return (lightZoneBotCorner.X <= position.X &&
                    lightZoneBotCorner.Y <= position.Y &&
                    lightZoneBotCorner.Z <= position.Z &&
                    lightZoneTopCorner.X >= position.X &&
                    lightZoneTopCorner.Y >= position.Y &&
                    lightZoneTopCorner.Z >= position.Z);
        }
    }
}
