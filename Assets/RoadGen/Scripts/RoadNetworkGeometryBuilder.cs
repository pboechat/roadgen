using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using RoadGen.Eppy;

namespace RoadGen
{
    public class RoadNetworkGeometryBuilder
    {
        public class UserData
        {
            public Vector2 origin;
            public int i0;
            public int i1;
            public float v;

        }

        static float OrientedAngle(Vector2 a, Vector2 b)
        {
            float l = a.magnitude * b.magnitude;
            if (l == 0)
                return 0;
            float angle = Mathf.Acos(Vector2.Dot(a, b) / l);
            if ((a.x * b.y - a.y * b.x) >= 0)
                return angle;
            else
                return 2 * Mathf.PI - angle;
        }

        static float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        static Vector2 GetCentroid(List<Vector2> polygon)
        {
            int i = 0;
            float doubleArea = 0;
            float centroidX = 0;
            float centroidY = 0;
            while (i < polygon.Count)
            {
                int j = i - 1;
                j = (j < 0) ? polygon.Count + j : j;
                Vector2 p0 = polygon[j];
                Vector2 p1 = polygon[i];
                float cross = Cross(p0, p1);
                centroidX += (p0.x + p1.x) * cross;
                centroidY += (p0.y + p1.y) * cross;
                doubleArea += cross;
                i += 1;
            }
            float m = 3.0f * doubleArea;
            if (m != 0)
                return new Vector2(centroidX / m, centroidY / m);
            else
                return default(Vector2);
        }

        public static void CreateSegmentCrossingPoints(
            Vector2 origin,
            List<Tuple<int, int>> nX01,
            List<Vector2> destinations,
            List<float> widths,
            ref List<Vector2> positions,
            ref List<Vector2> uvs,
            ref List<int> indices,
            ref Dictionary<Tuple<int, int>, Tuple<int, int>> edgeToIndicesMap
        )
        {
            if (destinations.Count < 2)
                return;

            List<Tuple<int, Vector2>> adjacencies = new List<Tuple<int, Vector2>>();
            adjacencies.Add(Tuple.Create(-1, destinations[0]));
            int i;
            for (i = 1; i < destinations.Count; i++)
                adjacencies.Add(Tuple.Create(i, destinations[i]));

            adjacencies.Sort(new Comparison<Tuple<int, Vector2>>((a, b) =>
            {
                if (a.Item1 == b.Item1)
                    return 0;
                if (a.Item1 == -1)
                    return -int.MaxValue;
                if (b.Item1 == -1)
                    return int.MaxValue;
                float aA = OrientedAngle(destinations[0], a.Item2),
                    aB = OrientedAngle(destinations[0], b.Item2);
                return aB.CompareTo(aA);
            }));

            int k = positions.Count;
            List<Vector2> polygon = new List<Vector2>();
            for (i = 0; i < adjacencies.Count; i++)
            {
                Vector2 elbow, joint;
                RoadGen.Collision.ArmPoints(adjacencies[i].Item2, new Vector2(0, 0), adjacencies[(i + 1) % adjacencies.Count].Item2, widths[i], out elbow, out joint);
                positions.Add(origin + elbow);
                polygon.Add(elbow);
            }

            Vector2 centroid = GetCentroid(polygon);
            for (i = 0; i < polygon.Count; i++)
                uvs.Add((polygon[i] - centroid).normalized * 0.5f);

            for (i = 0; i < adjacencies.Count - 2; i++)
            {
                indices.Add(k);
                indices.Add(k + i + 1);
                indices.Add(k + i + 2);
            }

            var n01 = nX01[0];
            edgeToIndicesMap[n01] = Tuple.Create(k, k + adjacencies.Count - 1);
            for (i = 1; i < adjacencies.Count; i++)
            {
                n01 = nX01[adjacencies[i].Item1];
                edgeToIndicesMap[n01] = Tuple.Create(k + i - 1, k + i);
            }
        }

        static void InterpolateSegmentPoints(
            Vector2 c0,
            Vector2 c1,
            Vector2 p0,
            Vector2 p1,
            Vector2 p2,
            Vector2 p3,
            float lengthStep,
            float u,
            float v,
            ref List<Vector2> positions,
            ref List<Vector2> uvs,
            ref List<int> indices,
            out int o_i0,
            out int o_i1,
            out float o_v
            )
        {
            Vector2 dir = (c1 - c0);
            float length = dir.magnitude;
            dir /= length;
            int N;
            if (lengthStep > 0)
                N = Mathf.CeilToInt(length / lengthStep);
            else
                N = 1;
            float actualLengthStep = length / (float)N;
            float lengthStep0 = ((p2 - p0).magnitude / (float)N),
                lengthStep1 = ((p3 - p1).magnitude / (float)N);
            Vector2 step0 = dir * lengthStep0;
            Vector2 step1 = dir * lengthStep1;
            Vector2 c_p0 = p0,
                c_p1 = p1,
                c_p2,
                c_p3;
            float c_v = v,
                n_v;
            // FIXME:
            o_i0 = 0;
            for (int i = 0; i < N; i++)
            {
                c_p2 = c_p0 + step0;
                c_p3 = c_p1 + step1;
                o_i0 = positions.Count;
                positions.Add(c_p0);
                positions.Add(c_p1);
                positions.Add(c_p2);
                positions.Add(c_p3);
                n_v = c_v + actualLengthStep;
                uvs.Add(new Vector2(0, c_v));
                uvs.Add(new Vector2(u, c_v));
                uvs.Add(new Vector2(0, n_v));
                uvs.Add(new Vector2(u, n_v));
                c_p0 = c_p2;
                c_p1 = c_p3;
                c_v = n_v;
                indices.Add(o_i0 + 2);
                indices.Add(o_i0 + 3);
                indices.Add(o_i0 + 1);
                indices.Add(o_i0 + 2);
                indices.Add(o_i0 + 1);
                indices.Add(o_i0);
            }
            o_i1 = o_i0 + 1;
            o_v = c_v;
        }

        public static void CreateStartSegmentPoints(
            Vector2 start,
            Vector2 end,
            float width,
            float lengthStep,
            float u,
            int n0,
            int n1,
            ref List<Vector2> positions1,
            ref List<Vector2> positions2,
            ref List<Vector2> uvs,
            ref List<int> indices,
            Dictionary<Tuple<int, int>, Tuple<int, int>> edgeToIndicesMap,
            out int o_i0,
            out int o_i1,
            out float o_v
        )
        {
            Tuple<int, int> i01;
            Vector2 p0, p1, p2, p3;
            RoadGen.Collision.ArmPoints(end, start, end, width, out p1, out p0);
            o_i0 = positions1.Count;
            o_i1 = o_i0 + 1;
            o_v = 0;
            positions1.Add(p0);
            positions1.Add(p1);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(u, 0));
            if (edgeToIndicesMap.TryGetValue(Tuple.Create(n1, n0), out i01))
            {
                p2 = positions2[i01.Item1];
                p3 = positions2[i01.Item2];
                InterpolateSegmentPoints(
                    start,
                    end,
                    p0,
                    p1,
                    p2,
                    p3,
                    lengthStep,
                    u,
                    o_v,
                    ref positions1,
                    ref uvs,
                    ref indices,
                    out o_i0,
                    out o_i1,
                    out o_v
                    );
                // NOTE: segment should never be continued!!!
                o_i0 = -1;
                o_i1 = -1;
                o_v = -1;
            }
        }

        public static void CreateMidSegmentPoints(
            Vector2 origin,
            Vector2 start,
            Vector2 end,
            float width,
            float lengthStep,
            float u,
            float v,
            int n0,
            int n1,
            int i0,
            int i1,
            ref List<Vector2> positions1,
            ref List<Vector2> positions2,
            ref List<Vector2> uvs,
            ref List<int> indices,
            Dictionary<Tuple<int, int>, Tuple<int, int>> edgeToIndicesMap,
            out int o_i0,
            out int o_i1,
            out float o_v
        )
        {
            Vector2 p0, p1, p2, p3;
            Tuple<int, int> i01;
            bool finishPreviousSegment;
            if (edgeToIndicesMap.TryGetValue(Tuple.Create(n0, n1), out i01))
            {
                p2 = positions2[i01.Item1];
                p3 = positions2[i01.Item2];
                finishPreviousSegment = false;
            }
            else
            {
                RoadGen.Collision.ArmPoints(origin, start, end, width, out p2, out p3);
                finishPreviousSegment = true;
            }
            if (finishPreviousSegment)
            {
                p0 = positions1[i0];
                p1 = positions1[i1];
                InterpolateSegmentPoints(
                    origin,
                    start,
                    p0,
                    p1,
                    p2,
                    p3,
                    lengthStep,
                    u,
                    v,
                    ref positions1,
                    ref uvs,
                    ref indices,
                    out o_i0,
                    out o_i1,
                    out o_v
                    );
            }
            else
            {
                int j = positions1.Count;
                positions1.Add(p2);
                positions1.Add(p3);
                o_v = v + (start - origin).magnitude;
                uvs.Add(new Vector2(0, o_v));
                uvs.Add(new Vector2(u, o_v));
                o_i0 = j;
                o_i1 = j + 1;
            }
            Tuple<int, int> i10;
            if (edgeToIndicesMap.TryGetValue(Tuple.Create(n1, n0), out i10))
            {
                p0 = p2;
                p1 = p3;
                p2 = positions2[i10.Item1];
                p3 = positions2[i10.Item2];
                InterpolateSegmentPoints(
                    start,
                    end,
                    p0,
                    p1,
                    p2,
                    p3,
                    lengthStep,
                    u,
                    o_v,
                    ref positions1,
                    ref uvs,
                    ref indices,
                    out o_i0,
                    out o_i1,
                    out o_v
                    );
                // NOTE: segment should never be continued!!!
                o_i0 = -1;
                o_i1 = -1;
                o_v = -1;
            }
        }

        public static void CreateEndSegmentPoints(
            Vector2 origin,
            Vector2 start,
            Vector2 end,
            float width,
            float lengthStep,
            float u,
            float v,
            int n0,
            int n1,
            int i0,
            int i1,
            ref List<Vector2> positions1,
            ref List<Vector2> positions2,
            ref List<Vector2> uvs,
            ref List<int> indices,
            Dictionary<Tuple<int, int>, Tuple<int, int>> edgeToIndicesMap
        )
        {
            Tuple<int, int> i01;
            Vector2 p0, p1, p2, p3;
            if (edgeToIndicesMap.TryGetValue(Tuple.Create(n0, n1), out i01))
            {
                p2 = positions2[i01.Item1];
                p3 = positions2[i01.Item2];
            }
            else
                RoadGen.Collision.ArmPoints(end, start, end, width, out p2, out p3);
            int n_i0, n_i1;
            float n_v;
            // NOTE: finishing previous segment
            if (i0 != -1 || i1 != -1)
            {
                p0 = positions1[i0];
                p1 = positions1[i1];
                InterpolateSegmentPoints(
                    origin,
                    start,
                    p0,
                    p1,
                    p2,
                    p3,
                    lengthStep,
                    u,
                    v,
                    ref positions1,
                    ref uvs,
                    ref indices,
                    out n_i0,
                    out n_i1,
                    out n_v
                    );
            }
            else
            {
                n_i0 = positions1.Count;
                n_i1 = n_i0 + 1;
                positions1.Add(p2);
                positions1.Add(p3);
                n_v = v + (start - origin).magnitude;
                uvs.Add(new Vector2(0, n_v));
                uvs.Add(new Vector2(u, n_v));
            }
            p0 = p2;
            p1 = p3;
            RoadGen.Collision.ArmPoints(start, end, start, width, out p2, out p3);
            InterpolateSegmentPoints(
                    start,
                    end,
                    p0,
                    p1,
                    p2,
                    p3,
                    lengthStep,
                    u,
                    n_v,
                    ref positions1,
                    ref uvs,
                    ref indices,
                    out n_i0,
                    out n_i1,
                    out n_v
                    );
        }

        private class Context : IRoadNetworkGeometry
        {
            public float scale;
            public float highwayWidth;
            public float streetWidth;
            public float lengthStep;
            public float highwayU;
            public Dictionary<Tuple<int, int>, Tuple<int, int>> edgeToIndicesMap;
            public List<Vector2> crossingPositions;
            public List<Vector2> crossingUVs;
            public List<int> crossingIndices;
            public List<Vector2> segmentPositions;
            public List<Vector2> segmentUVs;
            public List<int> segmentIndices;

            public Context(
                float scale,
                float highwayWidth,
                float streetWidth,
                float lengthStep
                )
            {
                this.scale = scale;
                this.highwayWidth = highwayWidth;
                this.streetWidth = streetWidth;
                this.lengthStep = lengthStep;
                highwayU = highwayWidth / streetWidth;
                crossingPositions = new List<Vector2>();
                crossingUVs = new List<Vector2>();
                crossingIndices = new List<int>();
                segmentPositions = new List<Vector2>();
                segmentUVs = new List<Vector2>();
                segmentIndices = new List<int>();
                edgeToIndicesMap = new Dictionary<Tuple<int, int>, Tuple<int, int>>();
            }

            public List<Vector2> GetCrossingPositions()
            {
                return crossingPositions;
            }

            public List<Vector2> GetCrossingUvs()
            {
                return crossingUVs;
            }

            public List<int> GetCrossingIndices()
            {
                return crossingIndices;
            }

            public List<Vector2> GetSegmentPositions()
            {
                return segmentPositions;
            }

            public List<Vector2> GetSegmentUvs()
            {
                return segmentUVs;
            }

            public List<int> GetSegmentIndices()
            {
                return segmentIndices;
            }

        }

        static bool SegmentCrossingVisitor(Segment s0, ref Context context)
        {
            List<Tuple<int, int>> nX01 = new List<Tuple<int, int>>();
            List<Vector2> destinations = new List<Vector2>();
            List<float> widths = new List<float>();
            destinations.Add((s0.Start - s0.End) * context.scale);
            int n0 = s0.Index * 2;
            nX01.Add(Tuple.Create(n0 + 1, n0));
            widths.Add((s0.Highway) ? context.highwayWidth : context.streetWidth);
            Vector2 origin = s0.End * context.scale;
            foreach (var s1 in s0.Destinations)
            {
                Assert.AreEqual(s1.Start, s0.End);
                destinations.Add(s1.End * context.scale - origin);
                n0 = s1.Index * 2;
                nX01.Add(Tuple.Create(n0, n0 + 1));
                widths.Add((s1.Highway) ? context.highwayWidth : context.streetWidth);
            }
            CreateSegmentCrossingPoints(
                origin,
                nX01,
                destinations,
                widths,
                ref context.crossingPositions,
                ref context.crossingUVs,
                ref context.crossingIndices,
                ref context.edgeToIndicesMap);

            return true;
        }

        static bool SegmentVisistor(Segment s0, Segment s1, ref Context context, UserData userData, out UserData o_userData)
        {
            o_userData = new UserData();
            if (s0 == null) // starting segment
            {
                Assert.IsNull(userData);
                int n0 = s1.Index * 2,
                    n1 = n0 + 1;
                float width = (s1.Highway) ? context.highwayWidth : context.streetWidth;
                CreateStartSegmentPoints(
                    s1.Start * context.scale,
                    s1.End * context.scale,
                    width,
                    context.lengthStep,
                    (s1.Highway) ? context.highwayU : 1,
                    n0,
                    n1,
                    ref context.segmentPositions,
                    ref context.crossingPositions,
                    ref context.segmentUVs,
                    ref context.segmentIndices,
                    context.edgeToIndicesMap,
                    out o_userData.i0,
                    out o_userData.i1,
                    out o_userData.v
                );
                o_userData.origin = s1.Start;
            }
            else if (s1 == null) // end segment
            {
                int n0 = s0.Index * 2,
                    n1 = n0 + 1;
                float width = (s0.Highway) ? context.highwayWidth : context.streetWidth;
                CreateEndSegmentPoints(
                    ((userData != null) ? userData.origin : s0.Start) * context.scale,
                    s0.Start * context.scale,
                    s0.End * context.scale,
                    width,
                    context.lengthStep,
                    (s0.Highway) ? context.highwayU : 1,
                    (userData != null) ? userData.v : 0,
                    n0,
                    n1,
                    (userData != null) ? userData.i0 : -1,
                    (userData != null) ? userData.i1 : -1,
                    ref context.segmentPositions,
                    ref context.crossingPositions,
                    ref context.segmentUVs,
                    ref context.segmentIndices,
                    context.edgeToIndicesMap
                );
            }
            else // mid segment
            {
                Assert.IsNotNull(userData);
                Assert.AreEqual(s0.End, s1.Start);
                int n0 = s1.Index * 2,
                    n1 = n0 + 1;
                float width = (s1.Highway) ? context.highwayWidth : context.streetWidth;
                CreateMidSegmentPoints(
                    s0.Start * context.scale,
                    s1.Start * context.scale,
                    s1.End * context.scale,
                    width,
                    context.lengthStep,
                    (s1.Highway) ? context.highwayU : 1,
                    userData.v,
                    n0,
                    n1,
                    userData.i0,
                    userData.i1,
                    ref context.segmentPositions,
                    ref context.crossingPositions,
                    ref context.segmentUVs,
                    ref context.segmentIndices,
                    context.edgeToIndicesMap,
                    out o_userData.i0,
                    out o_userData.i1,
                    out o_userData.v
                );
                o_userData.origin = s1.Start;
            }
            return true;
        }

        public static IRoadNetworkGeometry Build(
            float scale,
            float highwayWidth,
            float streetWidth,
            float lengthStep,
            List<Segment> segments,
            int mask = 0)
        {
            Context context = new Context(scale, highwayWidth, streetWidth, lengthStep);
            HashSet<Segment> visited = new HashSet<Segment>();
            foreach (var segment in segments)
                RoadNetworkTraversal.PreOrder(segment, ref context, SegmentCrossingVisitor, mask, ref visited);
            visited.Clear();
            foreach (var segment in segments)
                RoadNetworkTraversal.PreOrder<Context, UserData>(null, segment, ref context, null, SegmentVisistor, mask, ref visited);
            return context;
        }

    }

}