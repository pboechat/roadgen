using UnityEngine;
using RoadGen;

public class DummyAllotmentBuilder : MonoBehaviour, IAllotmentBuilder
{
    public float height = 12;
    public Material material;

    public GameObject Build(Allotment allotment, IHeightmap heightmap)
    {
        GameObject allotmentGO = new GameObject("Allotment");
        float z = heightmap.GetHeight(allotment.Center.x, allotment.Center.y);
        allotmentGO.AddComponent<MeshFilter>().mesh = StandardGeometry.CreateCubeMesh(allotment.Corners, height, z);
        allotmentGO.AddComponent<MeshRenderer>().material = material;
        return allotmentGO;
    }

}