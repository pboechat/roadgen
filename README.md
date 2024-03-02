roadgen
=======

This is a road network generation tool based on [Müller et al.](https://github.com/pboechat/roadgen/blob/master/%282001%29%20Procedural%20Modeling%20of%20Cities.pdf). The basic idea is to extend L-Systems to support intersection and snapping (self-sensitivity) and be guided by image maps (population density).

It features:

 - road network generation/manipulation
 - basic road network geometry construction
 - basic building allotment distribution


----------
#### Getting Started

 1. Add RoadNetwork to a game object in your scene an set generation parameters (e.g., street segment length)
 2. Add MockTerrain (or your own ITerrain implementation)
 3. [Optional] Add DummyAllotmentBuilder and RoadDensityMap. RoadDensityMap requires that you reference RoadNetwork.
 5. Add RoadDensityBasedSettlementSpawner (or your own settlement spawner implementation) and reference DummyAllotmentBuilder (or your own IAllotmentBuilder implementation) and RoadDensityMap
 6. Add RoadNetworkMesh and set materials (road segments and crossing),  mesh detail (e.g., length step) and reference to RoadNetwork and terrain

![Example](http://pedroboechat.com/images/roadgen.png)

##### Generation Parameters

![Generation Parameters](http://pedroboechat.com/images/roadgen-RoadNetwork.png)

- _Quadtree Params_: The min. coordinates and size of the quatree (in world units) in which the road segments are put to optimize collision detections.

- _Quadtree Max Objects/Levels_: Some other control parameters for the quadtree (max. number of items per quadrant and max. number of quadtree levels).

- _Segment Count Limit_: Each road segment expands forward or spawns child road segments (branch) at each derivation step. This parameter controls the maximum amount of expansions a road segment can have.

- _Derivation Step Limit_: Each derivation step is an opportunity for an existing road segments to expand or branch. This parameter controls the number of derivation steps.

- _Street/Highway Segment Length/Width_: Road segments can be streets or highways. This parameter controls their width/length (in world units).

- _Street/Highway Branch Probability_: This parameter is the normalized (0-1) probability of a road segment spawning another road segment.

- _Street/Highway Branch Population_: **TODO**

- _Street Branch Time Delay_: Number of derivation steps until the probability for streets to branch is evaluated.

- _Minimum Intersection_ = **TODO**

- _Snap Distance_: Road segments "snap" to each other in order to form network-like structures. This is the maximum distance (in world units) that a road segment can have from another until it's snapped.

- _Allotment Min/Max Half Diagonal_: Building allotments are placed alongside road segments at a certain derivation interval. This is the min./max. diagonal size (in world units) of an allotment.

- _Allotment Min/Max Aspect_: Once the diagonal size is sorted, the aspect length is sorted too. With the diagonal and aspect, we can define width and height. This controls the aspect (width / height) of the allotments.

- _Allotment Placement Loop_: **TODO**

- _Settlement Spawn Delay_: A settlement is a region where building allotments are placed. This parameter controls how many derivation steps needs to pass until a settlement can be randomly sorted.

- _Settlement Radius_: This parameter controls the size of settlement from the point they started (in world units).

- _Settlement Crossing/Highway Probabilities_: This parameter is the normalized (0-1) probability of a road segment that is either a highway or is in a crossing (i.e., has 2 orthogonal road segments connected to it) spawning a settlement.

- _Generate Highways/Streets_: Enable/disable the generation of highways/streets.


##### Script Execution Order

![Script Execution Order](http://pedroboechat.com/images/roadgen-ScriptExecutionOrder.png)

##### (Pseudo-)Randomness

Add GlobalSeeder to a game object in your scene to control pseudo-random number generation.

![Global Seeder](http://pedroboechat.com/images/roadgen-GlobalSeeder.png)


----------
#### Advanced Usage

##### Road Network Generation

	using System.Collections.Generic;
	using RoadGen;

	(...)

	List<Segment> segments;
	Quadtree quadtree;
	RoadNetworkGenerator.DebugData debugData;

	RoadNetworkGenerator.Generate(out segments, out quadtree, out debugData);

##### Road Network Traversal

###### Standard segment visitor

	using System.Collections.Generic;
	using RoadGen;

	(...)

	HashSet<Segment> visited = new HashSet<Segment>();
	foreach (var segment in segments)
	{
		RoadNetworkTraversal.PreOrder(segment, (a) =>
			{
				// my visitation logic
				return true;
			}, 
			mask, 
			ref visited);
	}
			
###### Segment visitor w/ per-traversal parameter (Context)

	using System.Collections.Generic;
	using RoadGen;

	(...)

	struct MyContext
	{
		// my data
	}

	(...)

	bool MyVisitor(Segment segment, ref MyContext myContext)
	{
		// my visitation logic
		return true;
	}

	(...)

	HashSet<Segment> visited = new HashSet<Segment>();
	MyContext myContext;
	foreach (var segment in roadNetwork.Segments)
	{
		RoadNetworkTraversal.PreOrder(segment, 
			ref myContext, 
			MyVisitor, 
			RoadNetworkTraversal.HIGHWAYS_MASK | RoadNetworkTraversal.STREETS_MASK, 
			ref visited);
	}

###### Segment visitor w/ per-segment parameter (User Data)

	using System.Collections.Generic;
	using RoadGen;

	(...)

	struct MyContext
	{
		// my context data
	}

	struct MyUserData
	{
		// my user data
	}

	(...)

	bool MyVisitor(Segment segment, ref MyContext myContext, MyUserData i_myUserData, out MyUserData o_myUserData)
	{
		o_myUserData = new MyUserData();
		// my visitation logic
		return true;
	}

	(...)

	HashSet<Segment> visited = new HashSet<Segment>();
	MyContext myContext;
	MyUserData myUserData;
	foreach (var segment in roadNetwork.Segments)
	{
		RoadNetworkTraversal.PreOrder(segment, 
			ref myContext, 
			myUserData,
			MyVisitor, 
			RoadNetworkTraversal.HIGHWAYS_MASK | RoadNetworkTraversal.STREETS_MASK, 
			ref visited);
	}

##### Road Network Geometry Construction

	using UnityEngine;
	using System.Collections.Generic;
	using System.Linq;
	using RoadGen;

	(...)

	var geometry = RoadNetworkGeometryBuilder.Build(
		scale,
		Config.highwaySegmentWidth,
		Config.streetSegmentWidth,
		lengthStep,
		segments,
		RoadNetworkTraversal.HIGHWAYS_MASK | RoadNetworkTraversal.STREETS_MASK
	);
		
	List<Vector3> vertices = new List<Vector3>();
	geometry.GetSegmentPositions().ForEach((p) =>
	{
		vertices.Add(new Vector3(p.x, heightmap.GetHeight(p.x, p.y), p.y));
	});

	geometry.GetCrossingPositions().ForEach((p) =>
	{
		vertices.Add(new Vector3(p.x, heightmap.GetHeight(p.x, p.y), p.y));
	});
		
	Mesh mesh = new Mesh();
	mesh.vertices = vertices.ToArray();
	mesh.triangles = geometry.GetCrossingIndices().ToArray();
	mesh.uv = geometry.GetCrossingUvs().ToArray();
	mesh.RecalculateNormals();
		
##### Custom Heightmap

Extend RoadGen.IHeightmap and implement:

	float GetHeight(float x, float y);
		
	bool Finished();

##### Custom Allotment Builder

Extend RoadGen.IAllotmentBuilder and implement:

	GameObject Build(Allotment allotment, IHeightmap heightmap);
