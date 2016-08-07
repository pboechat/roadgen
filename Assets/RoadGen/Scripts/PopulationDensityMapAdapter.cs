using UnityEngine;
using RoadGen;

public class PopulationDensityMapAdapter : MonoBehaviour, IMap
{
    public float GetWidth()
    {
        return RoadGen.Config.QuadtreeParams.width;
    }

    public float GetHeight()
    {
        return RoadGen.Config.QuadtreeParams.height;
    }

    public float GetMinX()
    {
        return RoadGen.Config.QuadtreeParams.xMin;
    }

    public float GetMaxX()
    {
        return RoadGen.Config.QuadtreeParams.xMax;
    }

    public float GetMinY()
    {
        return RoadGen.Config.QuadtreeParams.yMin;
    }

    public float GetMaxY()
    {
        return RoadGen.Config.QuadtreeParams.yMax;
    }

    public float GetNormalizedValue(float x, float y)
    {
        return RoadGen.PopulationDensityMap.DensityAt(x, y);
    }

    public bool Finished()
    {
        return true;
    }

}
