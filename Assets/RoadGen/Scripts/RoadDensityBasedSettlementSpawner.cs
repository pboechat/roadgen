using UnityEngine;
using System;
using System.Collections.Generic;
using RoadGen;
using RoadGen.Eppy;

public class RoadDensityBasedSettlementSpawner : MonoBehaviour
{
    [Serializable]
    public struct DensityTier
    {
        public float offset;
        public float multiplier;
        public float threshold;
        public Interpolation interpolation;

    }

    public DensityTier[] densityTiers;
    public float distanceFromRoad = 1.0f;
    public RoadNetwork roadNetwork;
    public RoadDensityMap roadDensityMap;
    public GameObject heightmapGameObject;
    public GameObject allotmentBuilderGameObject;
    public int maxNumAllotments = 1000;
    private IHeightmap heightmap;
    private List<Tuple<float, Allotment>> allotments;
    private float minAllotmentWidth;

    bool TryToAddSideBuilding(Allotment newAllotment, Vector2 position, Vector2 side, float direction, float halfRoadWidth, float density)
    {
        Vector2 center = position + side * (newAllotment.Height * 0.5f + distanceFromRoad + halfRoadWidth);
        newAllotment.UpdateCenterAndDirection(center, direction);
        bool addBuilding = false;
        for (int i = 0; i < Config.allotmentPlacementLoopLimit; i++)
        {
            int c = 0;
            Vector2 offset;
            var colliders = roadNetwork.Quadtree.Retrieve(newAllotment.GetCollider().GetAABB());
            for (int j = 0; j < colliders.Count && (c == 0 || i < Config.allotmentPlacementLoopLimit - 1); j++)
            {
                if (newAllotment.GetCollider().Collide(((ICollidable)colliders[j].reference).GetCollider(), out offset))
                {
                    c++;
                    newAllotment.Center = (newAllotment.Center + offset);
                }
            }
            if (c == 0)
            {
                addBuilding = true;
                break;
            }
        }
        if (!addBuilding)
            return false;
        allotments.Add(Tuple.Create(density, newAllotment));
        roadNetwork.Quadtree.Insert(newAllotment);
        return true;
    }

    float Apply(float value, float edge0, float edge1, Interpolation interpolation)
    {
        float x = (value - edge0) / (edge1 - edge0);
        x = (interpolation == Interpolation.SMOOTHERSTEP) ? x * x * x * (x * (x * 6 - 15) + 10) : (interpolation == Interpolation.SMOOTHSTEP) ? x * x * (3 - 2 * x) : x;
        return x;
    }

    int FindDensityTierIndex(float density)
    {
        int i = 0;
        for (; i < densityTiers.Length - 1; i++)
        {
            if (density >= densityTiers[i].threshold && density < densityTiers[i + 1].threshold)
                return i;
        }
        if (density >= densityTiers[i].threshold)
            return i;
        return -1;
    }

    float GetProbability(float density)
    {
        var i = FindDensityTierIndex(density);
        if (i == -1)
            return 0;
        var currDensityTier = densityTiers[i];
        return Mathf.Clamp01(Apply(density, currDensityTier.threshold, (i == densityTiers.Length - 1) ? 1 : densityTiers[i + 1].threshold, currDensityTier.interpolation)
            * currDensityTier.multiplier + currDensityTier.offset);
    }

    static Allotment CreateAllotment()
    {
        return new Allotment(default(Vector2),
                0,
                UnityEngine.Random.Range(Config.allotmentMinHalfDiagonal, Config.allotmentMaxHalfDiagonal),
                UnityEngine.Random.Range(Config.allotmentMinAspect, Config.allotmentMaxAspect));
    }

    bool SegmentVisitor(Segment s0)
    {
        var dir = (s0.End - s0.Start) / s0.Length;
        Vector2 leftDir = new Vector2(-dir.y, dir.x),
            rightDir = new Vector2(dir.y, -dir.x);
        float lengthLeft = 0, lengthRight = 0;
        Vector2 positionLeft = s0.Start, positionRight = s0.Start;
        float halfRoadWith = s0.Width * 0.5f;
        bool hasLeft = true, hasRight = true;
        do
        {
            var allotment = CreateAllotment();
            var step = allotment.Width;
            if (hasLeft)
            {
                var remainingLength = s0.Length - lengthLeft;
                bool forceFit = remainingLength < step && remainingLength >= minAllotmentWidth;
                if (remainingLength >= step || forceFit)
                {
                    if (forceFit)
                    {
                        var height = remainingLength / allotment.AspectRatio;
                        allotment.UpdateWidthAndHeight(remainingLength, height);
                        step = remainingLength;
                    }
                    var halfStep = step * 0.5f * dir;
                    var newPosition = positionLeft + halfStep;
                    var roadDensity = roadDensityMap.GetNormalizedValue(newPosition.x, newPosition.y);
                    if (UnityEngine.Random.value < GetProbability(roadDensity))
                        TryToAddSideBuilding(allotment, newPosition, leftDir, s0.Direction - 90, halfRoadWith, roadDensity);
                    positionLeft = newPosition + halfStep;
                    lengthLeft += step;
                }
                else
                    hasLeft = false;
            }
            allotment = CreateAllotment();
            step = allotment.Width;
            if (hasRight)
            {
                var remainingLength = s0.Length - lengthRight;
                bool forceFit = remainingLength < step && remainingLength >= minAllotmentWidth;
                if (remainingLength >= step || forceFit)
                {
                    if (forceFit)
                    {
                        var height = remainingLength / allotment.AspectRatio;
                        allotment.UpdateWidthAndHeight(remainingLength, height);
                        step = remainingLength;
                    }
                    var halfStep = step * 0.5f * dir;
                    var newPosition = positionRight + halfStep;
                    var roadDensity = roadDensityMap.GetNormalizedValue(newPosition.x, newPosition.y);
                    if (UnityEngine.Random.value < GetProbability(roadDensity))
                        TryToAddSideBuilding(allotment, newPosition, rightDir, s0.Direction + 90, halfRoadWith, roadDensity);
                    positionRight = newPosition + halfStep;
                    lengthRight += step;
                }
                else
                    hasRight = false;
            }
        } while (hasLeft || hasRight);
        return true;
    }

    void Start()
    {
        if (roadNetwork == null)
        {
            Debug.LogError("RoadDensityBasedSettlementSpawner needs a reference to a RoadNetwork");
            return;
        }

        if (!roadNetwork.Finished)
        {
            Debug.LogError("RoadDensityBasedSettlementSpawner script can only execute after RoadNetwork (Configure execution order that in Edit > Project Settings > Script Execution Order)");
            return;
        }

        if (roadDensityMap == null)
        {
            Debug.LogError("RoadDensityBasedSettlementSpawner needs a reference to a RoadDensityMap");
            return;
        }

        if (!roadDensityMap.Finished())
        {
            Debug.LogError("RoadDensityBasedSettlementSpawner script can only execute after RoadDensityMap (Configure execution order that in Edit > Project Settings > Script Execution Order)");
            return;
        }

        if (heightmapGameObject != null)
            heightmap = UnityEngineHelper.GetInterface<IHeightmap>(heightmapGameObject);

        if (heightmap == null)
        {
            Debug.LogError("RoadDensityBasedSettlementSpawner needs a reference to a game object containing at least one component that implements IHeightmap");
            return;
        }

        if (!heightmap.Finished())
        {
            Debug.LogError("RoadDensityBasedSettlementSpawner script can only execute after the components that implements IHeightmap (Configure execution order that in Edit > Project Settings > Script Execution Order)");
            return;
        }

        List<IAllotmentBuilder> allotmentBuilders = null;
        if (allotmentBuilderGameObject != null)
            allotmentBuilders = UnityEngineHelper.GetInterfaces<IAllotmentBuilder>(allotmentBuilderGameObject);
        if (allotmentBuilders == null)
        {
            Debug.LogError("RoadDensityBasedSettlementSpawner needs a reference to a game object containing at least one component that implements IAllotmentBuilder");
            return;
        }

        {
            float minThreshold = -float.MaxValue;
            foreach (var densityTier in densityTiers)
            {
                if (densityTier.threshold < minThreshold)
                {
                    Debug.LogError("Density tier has a threshold smaller than it's predecessor");
                    return;
                }
                minThreshold = densityTier.threshold;
            }
        }

        minAllotmentWidth = Allotment.GetWidth(Config.allotmentMinHalfDiagonal, Config.allotmentMinAspect);

        allotments = new List<Tuple<float, Allotment>>();
        HashSet<Segment> visited = new HashSet<Segment>();
        foreach (var segment in roadNetwork.Segments)
            RoadNetworkTraversal.PreOrder(segment, SegmentVisitor, roadNetwork.Mask, ref visited);
        if (maxNumAllotments > 0 && allotments.Count > maxNumAllotments)
        {
            allotments.Sort((a, b) => b.Item1.CompareTo(a.Item1));
            allotments = allotments.GetRange(0, maxNumAllotments);
        }
        GameObject allotmentsGO = new GameObject("Allotments");
        allotmentsGO.transform.parent = transform;
        {
            foreach (var allotment in allotments)
            {
                foreach (var allotmentBuilder in allotmentBuilders)
                {
                    GameObject allotmentGO = allotmentBuilder.Build(allotment.Item2, heightmap);
                    allotmentGO.transform.parent = allotmentsGO.transform;
                }
            }
        }
        Debug.Log(allotments.Count + " allotments spawned");
    }

}
