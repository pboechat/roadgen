using UnityEngine;
using System.Collections.Generic;
using RoadGen;

public class RoadNetwork : MonoBehaviour
{
    // Config
    public Rect quadtreeParams = new Rect(-25000, -25000, 50000, 50000);
    public int quadtreeMaxObjects = 10;
    public int quadtreeMaxLevels = 10;
    public int segmentCountLimit = 400;
    public int derivationStepLimit = 400;
    public float streetSegmentLength = 50;
    public float highwaySegmentLength = 120;
    public float streetSegmentWidth = 30;
    public float highwaySegmentWidth = 30;
    public float streetBranchProbability = 0.1f;
    public float highwayBranchProbability = 0.025f;
    public float streetBranchPopulationThreshold = 0.1f;
    public float highwayBranchPopulationThreshold = 0.15f;
    public int streetBranchTimeDelayFromHighway = 5;
    public float minimumIntersectionDeviation = 30;
    public float snapDistance = 30;
    public float allotmentMinHalfDiagonal = 60;
    public float allotmentMaxHalfDiagonal = 80;
    public float allotmentMinAspect = 1;
    public float allotmentMaxAspect = 1.45f;
    public int allotmentPlacementLoopLimit = 3;
    public int settlementSpawnDelay = 10;
    public int settlementDensity = 40;
    public float settlementRadius = 400.0f;
    public float settlementInCrossingProbability = 0.9f;
    public float settlementInHighwayProbability = 0.1f;
    // Road network
    public bool generateHighways = true;
    public bool generateStreets = true;
    private List<Segment> segments;
    private RoadGen.Quadtree quadtree;
    private int mask = 0;
    private bool finished = false;
    private Rect boundingBox;


    public List<Segment> Segments
    {
        get
        {
            return segments;
        }
    }

    public RoadGen.Quadtree Quadtree
    {
        get
        {
            return quadtree;
        }
    }

    public Rect BoundingBox
    {
        get
        {
            return boundingBox;
        }
    }

    public int Mask
    {
        get
        {
            return mask;
        }
    }

    public bool Finished
    {
        get
        {
            return finished;
        }
    }

    void SetupConfig()
    {
        Config.QuadtreeParams = quadtreeParams;
        Config.quadtreeMaxObjects = quadtreeMaxObjects;
        Config.quadtreeMaxLevels = quadtreeMaxLevels;
        Config.segmentCountLimit = segmentCountLimit;
        Config.derivationStepLimit = derivationStepLimit;
        Config.streetSegmentLength = streetSegmentLength;
        Config.highwaySegmentLength = highwaySegmentLength;
        Config.streetSegmentWidth = streetSegmentWidth;
        Config.highwaySegmentWidth = highwaySegmentWidth;
        Config.streetBranchProbability = streetBranchProbability;
        Config.highwayBranchProbability = highwayBranchProbability;
        Config.streetBranchPopulationThreshold = streetBranchPopulationThreshold;
        Config.streetBranchDelayFromHighway = streetBranchTimeDelayFromHighway;
        Config.minIntersectionDeviation = minimumIntersectionDeviation;
        Config.snapDistance = snapDistance;
        Config.allotmentMinHalfDiagonal = allotmentMinHalfDiagonal;
        Config.allotmentMaxHalfDiagonal = allotmentMaxHalfDiagonal;
        Config.allotmentMinAspect = allotmentMinAspect;
        Config.allotmentMaxAspect = allotmentMaxAspect;
        Config.allotmentPlacementLoopLimit = allotmentPlacementLoopLimit;
        Config.settlementSpawnDelay = settlementSpawnDelay;
        Config.settlementDensity = settlementDensity;
        Config.settlementRadius = settlementRadius;
        Config.settlementInCrossingProbability = settlementInCrossingProbability;
        Config.settlementInHighwayProbability = settlementInHighwayProbability;
    }

    void Start()
    {
        SetupConfig();

        // ---

        RoadNetworkGenerator.DebugData debugData;
        RoadNetworkGenerator.Generate(out segments, out quadtree, out debugData);

        Debug.Log(segments.Count + " segments");

        // ---

        mask = ((generateHighways) ? RoadNetworkTraversal.HIGHWAYS_MASK : 0) | ((generateStreets) ? RoadNetworkTraversal.STREETS_MASK : 0);

        float minX = float.MaxValue,
            maxX = -float.MaxValue,
            minY = float.MaxValue,
            maxY = -float.MaxValue;
        HashSet<Segment> visited = new HashSet<Segment>();
        foreach (var segment in segments)
        {
            RoadNetworkTraversal.PreOrder(segment, (a) =>
            {
                minX = Mathf.Min(Mathf.Min(a.Start.x, a.End.x), minX);
                minY = Mathf.Min(Mathf.Min(a.Start.y, a.End.y), minY);
                maxX = Mathf.Max(Mathf.Max(a.Start.x, a.End.x), maxX);
                maxY = Mathf.Max(Mathf.Max(a.Start.y, a.End.y), maxY);
                return true;
            }, mask, ref visited);
        }
        Vector2 size = new Vector2(maxX - minX, maxY - minY);
        Vector2 center = new Vector2(minX, minY);
        boundingBox = new Rect(center, size);

        finished = true;
    }

}
