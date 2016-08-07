using UnityEngine;
using RoadGen;

public class RoadDensityMap : MonoBehaviour, IMap
{
    public float samplingStep = 10.0f;
    public int collisionMapDownscale = 5;
    public int collisionStrokeSize = 60;
    public int kernelSize = 9;
    public float gaussianWeight = 16;
    public RoadNetwork roadNetwork;
    public bool saveMapToFile = false;
    private int bbMinWX;
    private int bbMinWY;
    private int bbMaxWX;
    private int bbMaxWY;
    private int bbWidth;
    private int bbHeight;
    private int dScale;
    private int w2GX;
    private int w2GY;
    private float[,] roadDensityMap;
    private bool finished = false;

    bool WorldToMapCoords(float wX, float wY, out int mX, out int mY)
    {
        mX = Mathf.RoundToInt((w2GX + wX) / dScale);
        mY = Mathf.RoundToInt((w2GY + wY) / dScale);
        return true;
    }

    void Start()
    {
        if (roadNetwork == null)
        {
            Debug.LogError("RoadDensityMap needs a reference to a RoadNetwork");
            return;
        }

        if (!roadNetwork.Finished)
        {
            Debug.LogError("RoadDensityMap script can only execute after RoadNetwork (Configure execution order that in Edit > Project Settings > Script Execution Order)");
            return;
        }

        bbMinWX = Mathf.FloorToInt(roadNetwork.BoundingBox.xMin);
        bbMinWY = Mathf.FloorToInt(roadNetwork.BoundingBox.yMin);
        bbMaxWX = Mathf.CeilToInt(roadNetwork.BoundingBox.xMax);
        bbMaxWY = Mathf.CeilToInt(roadNetwork.BoundingBox.yMax);
        bbWidth = bbMaxWX - bbMinWX;
        bbHeight = bbMaxWY - bbMinWY;
        int cWX = bbMinWX + Mathf.CeilToInt(bbWidth * 0.5f);
        int cWY = bbMinWY + Mathf.CeilToInt(bbHeight * 0.5f);

        dScale = (int)Mathf.Pow(2, collisionMapDownscale);
        int gSide = Mathf.NextPowerOfTwo(Mathf.CeilToInt(Mathf.Max(bbWidth, bbHeight))) / dScale + 1;
        int hGSide = gSide / 2 + 1;
        w2GX = hGSide * dScale - cWX;
        w2GY = hGSide * dScale - cWY;

        roadDensityMap = new float[gSide, gSide];
        RoadNetworkCollisionMap.Build(roadNetwork.Segments,
            roadNetwork.Mask,
            samplingStep,
            collisionStrokeSize,
            RoadNetworkCollisionMap.SquaredSmoothCircleBrush,
            gSide,
            WorldToMapCoords,
            roadDensityMap);

        RoadNetworkCollisionMap.Normalize(gSide, roadDensityMap);

        if (kernelSize > 0)
        {
            var gaussian = FilterHelper.CreateGaussianKernel(kernelSize, gaussianWeight);
            var dst = new float[gSide, gSide];
            FilterHelper.Convolute(roadDensityMap, dst, gaussian, gSide, gSide, kernelSize);
            roadDensityMap = dst;
        }

        if (saveMapToFile)
            RoadNetworkCollisionMap.SaveMapToFile("road_density_map.png", roadDensityMap);

        finished = true;
    }

    public float GetNormalizedValue(float x, float y)
    {
        int mX, mY;
        if (!WorldToMapCoords(x, y, out mX, out mY))
            return -1;
        return roadDensityMap[mX, mY];
    }

    public float GetWidth()
    {
        return (float)bbWidth;
    }

    public float GetHeight()
    {
        return (float)bbHeight;
    }

    public float GetMinX()
    {
        return (float)bbMinWX;
    }

    public float GetMaxX()
    {
        return (float)bbMaxWX;
    }

    public float GetMinY()
    {
        return (float)bbMinWY;
    }

    public float GetMaxY()
    {
        return (float)bbMaxWY;
    }
    public bool Finished()
    {
        return finished;
    }

}
