using UnityEngine;
using RoadGen;

public class MockTerrain : MonoBehaviour, IHeightmap
{
    public float z;

    public float GetHeight(float x, float y)
    {
        return z;
    }

    public bool Finished()
    {
        return true;
    }

}
