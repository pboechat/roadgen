roadgen
=======

Road network generation tool based on [Müller et al.](https://github.com/pboechat/roadgen/blob/master/%282001%29%20Procedural%20Modeling%20of%20Cities.pdf). The basic idea is to extend L-System to support intersection and snapping (self-sensitivity) and be guided by image maps (population density).

It features:

 - road network generation/manipulation
 - basic road network geometry construction
 - basic building allotment distribution


----------
#### Getting Started

 1. Add RoadNetwork to a game object in your scene an set generation parameters (ie.: street segment length)
 2. Add MockTerrain (or your own ITerrain implementation)
 3. [Optional] Add DummyAllotmentBuilder and RoadDensityMap. RoadDensityMap requires that you reference RoadNetwork.
 5. Add RoadDensityBasedSettlementSpawner (or your own settlement spawner implementation) and reference DummyAllotmentBuilder (or your own IAllotmentBuilder implementation) and RoadDensityMap
 6. Add RoadNetworkMesh and set materials (road segments and crossing),  mesh granularity (length step) and reference to RoadNetwork and terrain

![Example](http://www.pedroboechat.com/images/roadgen.png)

##### Generation Parameters

![Generation Parameters](http://www.pedroboechat.com/images/roadgen-RoadNetwork.png)

**TODO**

##### Script Execution Order

![Script Execution Order](http://www.pedroboechat.com/images/roadgen-ScriptExecutionOrder.png)

##### (Pseudo-)Randomness

Add GlobalSeeder to a game object in your scene to control pseudo-random number generation.

![Global Seeder](http://www.pedroboechat.com/images/roadgen-GlobalSeeder.png)


----------
#### Advanced Usage

##### Road Network Generation

		using System.Collections.Generic;
		using RoadGen;

		(...)

		List<Segment> segments;
		RoadGen.Quadtree quadtree;
		RoadNetworkGenerator.DebugData debugData;

		RoadNetworkGenerator.Generate(out segments, out quadtree, out debugData);

##### Road Network Traversal

###### Standard segment visitor

		using System.Collections.Generic;
		using RoadGen;

		(...)

		HashSet<Segment> visited = new HashSet<Segment>();
		foreach (var segment in segments)
			RoadNetworkTraversal.PreOrder(segment, (a) =>
			{
				// my logic
				return true;
			}, 
			mask, 
			ref visited);
			
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
			// my logic
			return true;
		}
		
		(...)
		
		HashSet<Segment> visited = new HashSet<Segment>();
		MyContext myContext;
        foreach (var segment in roadNetwork.Segments)
            RoadNetworkTraversal.PreOrder(segment, 
				ref myContext, 
				MyVisitor, 
				RoadNetworkTraversal.HIGHWAYS_MASK | RoadNetworkTraversal.STREETS_MASK, 
				ref visited);

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
			// my logic
			return true;
		}
		
		(...)
		
		HashSet<Segment> visited = new HashSet<Segment>();
		MyContext myContext;
		MyUserData myUserData;
        foreach (var segment in roadNetwork.Segments)
            RoadNetworkTraversal.PreOrder(segment, 
				ref myContext, 
				myUserData,
				MyVisitor, 
				RoadNetworkTraversal.HIGHWAYS_MASK | RoadNetworkTraversal.STREETS_MASK, 
				ref visited);

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
