using System;

namespace Raymagic
{
    [Serializable]
    class SaveContainer
    {
        public SDFout[,,] distanceMap;

        public SaveContainer(SDFout[,,] dm)
        {
            this.distanceMap = dm;
        }
    }
}
