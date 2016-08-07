using UnityEngine;

namespace RoadGen
{
    public interface ITerrain : IHeightmap
    {
        int GetHeightmapDownscale();
        TerrainData GetData();

    }

}