using UnityEngine;
using System;
using RoadGen;

public class MapMesh : MonoBehaviour
{
    public float samplingScale = 0.01f;
    public float z = 0;
    public bool invertY = false;
    public bool invertX = false;
    public GameObject mapGameObject;
    // NOTE: heat map
    public Color[] colors = new Color[] {
        new Color(0, 0, 1, 0),     // Blue.
        new Color(0, 1, 1, 0),     // Cyan.
        new Color(0, 1, 0, 0),     // Green.
        new Color(1, 1, 0, 0),     // Yellow.
        new Color(1, 0, 0, 0)      // Red.
        };

    void Start()
    {
        IMap map = null;
        if (mapGameObject != null)
            map = UnityEngineHelper.GetInterface<IMap>(mapGameObject);

        if (map == null)
        {
            Debug.LogError("MapMesh needs a reference to a game object containing at least one component that implements IMap");
            return;
        }

        if (!map.Finished())
        {
            Debug.LogError("MapMesh script can only execute after the component that implements IMap (Configure execution order that in Edit > Project Settings > Script Execution Order)");
            return;
        }

        int width = Mathf.CeilToInt(map.GetWidth() * samplingScale),
            height = Mathf.CeilToInt(map.GetHeight() * samplingScale);
        Texture2D mapTexture = new Texture2D(width, height, TextureFormat.RGB24, true);
        Color[] pixels = new Color[width * height];
        ColorGradient gradient = new ColorGradient(colors);
        int y = (invertY) ? height - 1 : 0;
        Func<int, bool> yCompare, xCompare;
        Func<int, int> yMove, xMove;
        if (invertY)
        {
            yCompare = (a) => a >= 0;
            yMove = (a) => a - 1;
        }
        else
        {
            yCompare = (a) => a < height;
            yMove = (a) => a + 1;
        }
        if (invertX)
        {
            xCompare = (a) => a >= 0;
            xMove = (a) => a - 1;
        }
        else
        {
            xCompare = (a) => a < width;
            xMove = (a) => a + 1;
        }
        for (int p = 0; yCompare(y); y = yMove(y))
        {
            float wY = y / samplingScale + map.GetMinY();
            int x = (invertX) ? width - 1 : 0;
            for (; xCompare(x); x = xMove(x), p++)
            {
                float wX = x / samplingScale + map.GetMinX();
                Color pixel = new Color();
                gradient.GetColorAtValue(map.GetNormalizedValue(wX, wY), ref pixel);
                pixels[p] = pixel;
            }
        }
        mapTexture.SetPixels(pixels);
        mapTexture.Apply(true);
        GameObject mapGO = new GameObject("Map");
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(map.GetMinX(), map.GetMaxY(), z),
            new Vector3(map.GetMaxX(), map.GetMaxY(), z),
            new Vector3(map.GetMaxX(), map.GetMinY(), z),
            new Vector3(map.GetMinX(), map.GetMinY(), z)
        };
        mesh.triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0)
        };
        mesh.normals = new Vector3[]
        {
            Vector3.back,
            Vector3.back,
            Vector3.back,
            Vector3.back
        };
        mapGO.AddComponent<MeshFilter>().mesh = mesh;
        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.SetTexture("_MainTex", mapTexture);
        mapGO.AddComponent<MeshRenderer>().material = material;
        mapGO.transform.parent = transform;
        mapGO.transform.localRotation = Quaternion.identity;
    }

}
