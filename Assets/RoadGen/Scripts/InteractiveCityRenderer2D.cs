using UnityEngine;
using System.Collections.Generic;
using RoadGen;

public class InteractiveCityRenderer2D : MonoBehaviour
{
    public Color highwayStartColor = Color.red;
    public Color highwayEndColor = Color.red;
    public Color streetStartColor = Color.gray;
    public Color streetEndColor = Color.gray;
    public Texture2D intersectionIcon;
    public Texture2D snapIcon;
    public Texture2D intersectionRadiusIcon;
    public float iconSize = 0.333f;
    public float highwayWidth = 0.05f;
    public float streetWidth = 0.025f;
    public float downscaleFactor = 1000.0f;
    public float z = 1;
    public bool displayBuildings = true;
    public bool displayHighways = true;
    public bool displayStreets = true;
    RoadNetworkGenerator.InteractiveGenerationContext context;
    HashSet<Segment> visited;
    List<GameObject> segmentsGOs;
    List<GameObject> iconGOs;
    int mask;
    int action;
    bool step;
    bool end;
    int speed;

    bool Visitor(Segment segment)
    {
        GameObject segmentGO = new GameObject("Segment " + segment.Index);
        segmentGO.AddComponent<MeshFilter>().mesh = CreateLineMesh(
            segment.Start / downscaleFactor,
            segment.End / downscaleFactor,
            z,
            segment.Highway ? highwayStartColor : streetStartColor,
            segment.Highway ? highwayEndColor : streetEndColor,
            segment.Highway ? highwayWidth : streetWidth);
        segmentGO.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/VertexColor"));
        segmentsGOs.Add(segmentGO);
        return true;
    }

    void Start()
    {
        context = RoadNetworkGenerator.BeginInteractiveGeneration();
        visited = new HashSet<Segment>();
        mask = ((displayHighways) ? RoadNetworkTraversal.HIGHWAYS_MASK : 0) | ((displayStreets) ? RoadNetworkTraversal.STREETS_MASK : 0);
        segmentsGOs = new List<GameObject>();
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

            foreach (Segment segment in context.segments)
                RoadNetworkTraversal.PreOrder(segment, Visitor, mask, ref visited);

            RemoveIcons();
            if (context.debugData.intersections.Count > 0 ||
                context.debugData.snaps.Count > 0 ||
                context.debugData.intersectionsRadius.Count > 0)
            {
                foreach (Vector2 intersection in context.debugData.intersections)
                    iconGOs.Add(CreateIcon(intersection, iconSize, downscaleFactor, z, intersectionIcon));
                foreach (Vector2 snap in context.debugData.snaps)
                    iconGOs.Add(CreateIcon(snap, iconSize, downscaleFactor, z, snapIcon));
                foreach (Vector2 intersectionRadius in context.debugData.intersectionsRadius)
                    iconGOs.Add(CreateIcon(intersectionRadius, iconSize, downscaleFactor, z, intersectionRadiusIcon));
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

    static Mesh CreateLineMesh(Vector2 start, Vector2 end, float z, Color startColor, Color endColor, float width)
    {
        float halfWidth = width * 0.5f;
        Vector2 direction = end - start;
        Vector2 side = new Vector2(direction.y, -direction.x);
        side.Normalize();
        side *= halfWidth;
        Vector2[] positions = new Vector2[4];
        positions[0] = start - side;
        positions[1] = end - side;
        positions[2] = end + side;
        positions[3] = start + side;
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        for (int i = 0; i < 4; i++)
            vertices[i] = new Vector3(positions[i].x, positions[i].y, z);
        mesh.vertices = vertices;
        mesh.normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
        mesh.colors = new Color[] { startColor, endColor, endColor, startColor };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        return mesh;
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

    void RemoveIcons()
    {
        foreach (var gameObject in iconGOs)
            Destroy(gameObject);
        iconGOs.Clear();
    }

    static GameObject CreateIcon(Vector2 intersection, float iconSize, float downscaleFactor, float z, Texture2D icon)
    {
        GameObject gameObject = new GameObject("Icon");
        gameObject.AddComponent<MeshFilter>().mesh = CreateQuadMesh(intersection / downscaleFactor, iconSize, iconSize, z - 1);
        Material material = new Material(Shader.Find("Unlit/Transparent"));
        material.SetTexture("_MainTex", icon);
        gameObject.AddComponent<MeshRenderer>().material = material;
        return gameObject;
    }

}
