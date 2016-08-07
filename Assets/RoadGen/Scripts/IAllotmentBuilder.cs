using UnityEngine;

namespace RoadGen
{
    public interface IAllotmentBuilder
    {
        GameObject Build(Allotment allotment, IHeightmap heightmap);

    }

}