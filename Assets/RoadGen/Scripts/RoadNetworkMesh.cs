using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using RoadGen;

public class RoadNetworkMesh : MonoBehaviour
{
    public float zOffset = 1;
    public float lengthStep = 10;
    public Material roadSegmentsMaterial;
    public Material roadCrossingsMaterial;
    public RoadNetwork roadNetwork;
    public GameObject heightmapGameObject;

    void Start()
    {
        if (roadNetwork == null)
        {
            Debug.LogError("RoadNetworkMesh needs a reference to a RoadNetwork");
            return;
        }

        if (!roadNetwork.Finished)
        {
            Debug.LogError("RoadNetworkMesh script can only execute after RoadNetwork (Configure execution order that in Edit > Project Settings > Script Execution Order)");
            return;
        }

        IHeightmap heightmap = null;
        if (heightmapGameObject != null)
            heightmap = UnityEngineHelper.GetInterface<IHeightmap>(heightmapGameObject);

        if (heightmap == null)
        {
            Debug.LogError("RoadNetworkMesh needs a reference to a game object containing at least one component that implements IHeightmap");
            return;
        }

        if (!heightmap.Finished())
        {
            Debug.LogError("RoadNetworkMesh script can only execute after the components that implements IHeightmap (Configure execution order that in Edit > Project Settings > Script Execution Order)");
            return;
        }

        var geometry = RoadNetworkGeometryBuilder.Build(
            1.0f,
            Config.highwaySegmentWidth,
            Config.streetSegmentWidth,
            lengthStep,
            roadNetwork.Segments,
            roadNetwork.Mask
        );

        GameObject roadGO = new GameObject("Road");
        List<Vector3> vertices = new List<Vector3>();
        geometry.GetSegmentPositions().ForEach((p) =>
        {
            vertices.Add(new Vector3(p.x, heightmap.GetHeight(p.x, p.y) + zOffset, p.y));
        });
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = geometry.GetSegmentIndices().ToArray();
        mesh.uv = geometry.GetSegmentUvs().ToArray();
        mesh.RecalculateNormals();
        GameObject segmentsGO = new GameObject("Segments");
        segmentsGO.AddComponent<MeshFilter>().mesh = mesh;
        var meshRenderer = segmentsGO.AddComponent<MeshRenderer>();
        meshRenderer.material = roadSegmentsMaterial;
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        segmentsGO.transform.parent = roadGO.transform;
        vertices = new List<Vector3>();
        geometry.GetCrossingPositions().ForEach((p) =>
        {
            vertices.Add(new Vector3(p.x, heightmap.GetHeight(p.x, p.y) + zOffset, p.y));
        });
        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = geometry.GetCrossingIndices().ToArray();
        mesh.uv = geometry.GetCrossingUvs().ToArray();
        mesh.RecalculateNormals();
        GameObject crossingsGO = new GameObject("Crossings");
        crossingsGO.AddComponent<MeshFilter>().mesh = mesh;
        meshRenderer = crossingsGO.AddComponent<MeshRenderer>();
        meshRenderer.material = roadCrossingsMaterial;
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        crossingsGO.transform.parent = roadGO.transform;
        roadGO.transform.parent = transform;
    }

}
