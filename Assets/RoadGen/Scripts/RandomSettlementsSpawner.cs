using UnityEngine;
using System.Collections.Generic;
using RoadGen;

public class RandomSettlementsSpawner : MonoBehaviour
{
    public RoadNetwork roadNetwork;
    public GameObject heightmapGameObject;
    public GameObject allotmentBuilderGameObject;
    private IHeightmap heightmap;

    public class Context
    {
        public List<Allotment> allotments;
        public Quadtree quadtree;

        public Context(Quadtree quadtree)
        {
            allotments = new List<Allotment>();
            this.quadtree = quadtree;
        }

    }

    static bool SegmentVisitor(Segment s0, ref Context context, int counter, out int o_counter)
    {
        if (counter >= 0 && counter < Config.settlementSpawnDelay)
        {
            o_counter = counter + 1;
            return true;
        }
        float settlementProbability = (s0.Destinations.Count > 1) ? Config.settlementInCrossingProbability : Config.settlementInHighwayProbability;
        if (UnityEngine.Random.value >= settlementProbability)
        {
            o_counter = counter + 1;
            return true;
        }
        List<Allotment> newBuildings = new List<Allotment>();
        SettlementSpawner.Spawn(s0, Config.settlementDensity, Config.settlementRadius, context.quadtree, ref newBuildings);
        foreach (var newBuilding in newBuildings)
            context.quadtree.Insert(newBuilding);
        context.allotments.AddRange(newBuildings);
        o_counter = 0;
        return true;
    }

    void Start()
    {
        if (roadNetwork == null)
        {
            Debug.LogError("RandomSettlementsSpawner needs a reference to a RoadNetwork");
            return;
        }

        if (!roadNetwork.Finished)
        {
            Debug.LogError("RandomSettlementsSpawner script can only execute after RoadNetwork (Configure execution order that in Edit > Project Settings > Script Execution Order)");
            return;
        }

        if (heightmapGameObject != null)
            heightmap = UnityEngineHelper.GetInterface<IHeightmap>(heightmapGameObject);

        if (heightmap == null)
        {
            Debug.LogError("RandomSettlementsSpawner needs a reference to a game object containing at least one component that implements IHeightmap");
            return;
        }

        if (!heightmap.Finished())
        {
            Debug.LogError("RandomSettlementsSpawner script can only execute after the components that implements IHeightmap (Configure execution order that in Edit > Project Settings > Script Execution Order)");
            return;
        }

        List<IAllotmentBuilder> allotmentBuilders = null;
        if (allotmentBuilderGameObject != null)
            allotmentBuilders = UnityEngineHelper.GetInterfaces<IAllotmentBuilder>(allotmentBuilderGameObject);
        if (allotmentBuilders == null)
        {
            Debug.LogError("RoadDensitySettlementSpawner needs a reference to a game object containing at least one component that implements IAllotmentBuilder");
            return;
        }

        Context context = new Context(roadNetwork.Quadtree);
        HashSet<Segment> visited = new HashSet<Segment>();
        foreach (var segment in roadNetwork.Segments)
            RoadNetworkTraversal.PreOrder(segment, ref context, -1, SegmentVisitor, roadNetwork.Mask, ref visited);
        GameObject allotmentsGO = new GameObject("Allotments");
        allotmentsGO.transform.parent = transform;
        foreach (var allotment in context.allotments)
        {
            foreach (var allotmentBuilder in allotmentBuilders)
            {
                GameObject allotmentGO = allotmentBuilder.Build(allotment, heightmap);
                allotmentGO.transform.parent = allotmentsGO.transform;
            }
        }
        Debug.Log(context.allotments.Count + " allotments spawned");
    }

}
