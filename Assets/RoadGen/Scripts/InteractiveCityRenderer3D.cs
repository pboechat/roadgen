using UnityEngine;
using System.Collections.Generic;
using RoadGen;

public class InteractiveCityRenderer3D : MonoBehaviour
{
    public float scale = 0.001f;
    public float z = 1;
    public float lengthStep = 1;
    public bool displayBuildings = true;
    public bool displayHighways = true;
    public bool displayStreets = true;
    public Material roadSegmentsMaterial;
    public Material roadCrossingsMaterial;
    public float iconSize = 0.333f;
    public Texture2D intersectionIcon;
    public Texture2D snapIcon;
    public Texture2D intersectionRadiusIcon;
    RoadNetworkGenerator.InteractiveGenerationContext context;
    List<GameObject> iconGOs;
    GameObject roadGO;
    int action;
    bool step;
    bool end;
    int speed;

    void CreateRoadMesh(IRoadNetworkGeometry roadGeometry)
    {
        Destroy(roadGO);
        roadGO = new GameObject("Road");
        List<Vector3> vertices = new List<Vector3>();
        roadGeometry.GetSegmentPositions().ForEach((p) =>
        {
            vertices.Add(new Vector3(p.x, p.y, z));
        });
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = roadGeometry.GetSegmentIndices().ToArray();
        mesh.uv = roadGeometry.GetSegmentUvs().ToArray();
        mesh.RecalculateNormals();
        GameObject segmentsGO = new GameObject("Segments");
        segmentsGO.AddComponent<MeshFilter>().mesh = mesh;
        segmentsGO.AddComponent<MeshRenderer>().material = roadSegmentsMaterial;
        segmentsGO.transform.parent = roadGO.transform;
        vertices = new List<Vector3>();
        roadGeometry.GetCrossingPositions().ForEach((p) =>
        {
            vertices.Add(new Vector3(p.x, p.y, z));
        });
        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = roadGeometry.GetCrossingIndices().ToArray();
        mesh.uv = roadGeometry.GetCrossingUvs().ToArray();
        mesh.RecalculateNormals();
        GameObject crossingsGO = new GameObject("Crossings");
        crossingsGO.AddComponent<MeshFilter>().mesh = mesh;
        crossingsGO.AddComponent<MeshRenderer>().material = roadCrossingsMaterial;
        crossingsGO.transform.parent = roadGO.transform;
    }
    
    void RemoveIcons()
    {
        foreach (var gameObject in iconGOs)
            Destroy(gameObject);
        iconGOs.Clear();
    }

    static GameObject CreateIcon(Vector2 intersection, float iconSize, float scale, float z, Texture2D icon)
    {
        GameObject gameObject = new GameObject("Icon");
        gameObject.AddComponent<MeshFilter>().mesh = CreateQuadMesh(intersection * scale, iconSize, iconSize, z - 1);
        Material material = new Material(Shader.Find("Unlit/Transparent"));
        material.SetTexture("_MainTex", icon);
        gameObject.AddComponent<MeshRenderer>().material = material;
        return gameObject;
    }

    static Mesh CreateQuadMesh(Vector2 center, float width, float height, float z)
    {
        Mesh mesh = new Mesh();
        float halfWidth = width * 0.5f, halfHeight = height * 0.5f;
        mesh.vertices = new Vector3[]
        {
            new Vector3(center.x - halfWidth, center.y - halfHeight, z),
            new Vector3(center.x - halfWidth, center.y + halfHeight, z),
            new Vector3(center.x + halfWidth, center.y + halfHeight, z),
            new Vector3(center.x + halfWidth, center.y - halfHeight, z)
        };
        mesh.normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
        mesh.uv = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0)
        };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        return mesh;
    }

    void Start()
    {
        context = RoadNetworkGenerator.BeginInteractiveGeneration();
        iconGOs = new List<GameObject>();
        action = 1;
    }

    void Update()
    {
        if (action == 0)
            return;

        if (action == 1)
        {
            if (!end)
                end = Input.GetKeyDown(KeyCode.F5);

            if (!step)
                step = Input.GetKeyDown(KeyCode.F10);

            if (!step && !end)
                return;

            if (end)
            {
                RoadNetworkGenerator.EndInteractiveGeneration(ref context);
                action = 0;
            }
            else
            {
                if (!RoadNetworkGenerator.InteractiveGenerationStep(speed, ref context))
                    action = 0;
            }

            // ---

            var roadGeometry = RoadNetworkGeometryBuilder.Build(
                scale,
                Config.highwaySegmentWidth * scale,
                Config.streetSegmentWidth * scale,
                lengthStep,
                context.segments,
                ((displayHighways) ? RoadNetworkTraversal.HIGHWAYS_MASK : 0) | ((displayStreets) ? RoadNetworkTraversal.STREETS_MASK : 0)
            );
            CreateRoadMesh(roadGeometry);

            // ---

            RemoveIcons();
            if (context.debugData.intersections.Count > 0 ||
                context.debugData.snaps.Count > 0 ||
                context.debugData.intersectionsRadius.Count > 0)
            {
                foreach (Vector2 intersection in context.debugData.intersections)
                    iconGOs.Add(CreateIcon(intersection, iconSize, scale, z, intersectionIcon));
                foreach (Vector2 snap in context.debugData.snaps)
                    iconGOs.Add(CreateIcon(snap, iconSize, scale, z, snapIcon));
                foreach (Vector2 intersectionRadius in context.debugData.intersectionsRadius)
                    iconGOs.Add(CreateIcon(intersectionRadius, iconSize, scale, z, intersectionRadiusIcon));
                context.debugData.intersections.Clear();
                context.debugData.snaps.Clear();
                context.debugData.intersectionsRadius.Clear();
            }

            step = false;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 40, 20), "Speed");
        if (!int.TryParse(GUI.TextField(new Rect(50, 10, 40, 20), speed + ""), out speed))
            speed = 1;
        else
            speed = (int)Mathf.Max(1, speed);
        step = GUI.Button(new Rect(10, 40, 100, 20), "Step");
        end = GUI.Button(new Rect(10, 70, 100, 20), "End");
        GUI.Label(new Rect(10, 100, 200, 20), "Global Derivation Step: " + context.globalDerivationStep);
        if (GUI.Button(new Rect(10, 130, 140, 20), "Remove Icons"))
            RemoveIcons();
    }

}
