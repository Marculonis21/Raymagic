using System;

namespace Raymagic
{
    [Serializable]
    class SaveContainer
    {
        public float[,,] distanceMap;

        public SaveContainer(float[,,] dm)
        {
            this.distanceMap = dm;
        }
    }
}
