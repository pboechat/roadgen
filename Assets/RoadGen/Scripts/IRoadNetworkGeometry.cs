using UnityEngine;
using System.Collections.Generic;

namespace RoadGen
{
    public interface IRoadNetworkGeometry
    {
        List<Vector2> GetCrossingPositions();
        List<Vector2> GetCrossingUvs();
        List<int> GetCrossingIndices();
        List<Vector2> GetSegmentPositions();
        List<Vector2> GetSegmentUvs();
        List<int> GetSegmentIndices();

    }

}