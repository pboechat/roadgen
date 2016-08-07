using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RoadGen.Mischel.Collections;
using RoadGen.Eppy;

namespace RoadGen
{
    public static class RoadNetworkGenerator
    {
        public class DebugData
        {
            public List<Vector2> intersections = new List<Vector2>();
            public List<Vector2> snaps = new List<Vector2>();
            public List<Vector2> intersectionsRadius = new List<Vector2>();

        }

        public static List<T> Splice<T>(this List<T> source, int index, int count)
        {
            var items = source.GetRange(index, count);
            source.RemoveRange(index, count);
            return items;
        }

        static bool LocalConstraints(Segment segment, List<Segment> segments, Quadtree quadtree, DebugData debugData)
        {
            int priority = 0;
            Func<bool> action = null;
            float t0 = -1;

            var matches = quadtree.Retrieve(segment);
            for (int i = 0, j = 0, k = matches.Count - 1; j <= k; i = j += 1)
            {
                Segment other = (Segment)matches[i].reference;

                if (segment == other)
                    continue;

                // intersection check
                if (priority <= 4)
                {
                    Vector2 intersection;
                    float t1;
                    if (segment.Intersect(other, out intersection, out t1))
                    {
                        if (t0 == -1 || t1 < t0)
                        {
                            t0 = t1;
                            priority = 4;
                            action = () =>
                            {
                                float dirDiff = Mathf.Abs(other.Direction - segment.Direction) % 180.0f;
                                float minDirectionDiff = Mathf.Min(dirDiff, Mathf.Abs(dirDiff - 180.0f));
                                if (minDirectionDiff < Config.minIntersectionDeviation)
                                    return false;
                                IntersectSegments(intersection, other, segment, segments, quadtree);
                                if (debugData != null)
                                    debugData.intersections.Add(intersection);
                                return true;
                            };
                        }
                    }
                }
                // snap to crossing within radius check
                if (priority <= 3)
                {
                    if ((segment.End - other.End).magnitude <= Config.snapDistance)
                    {
                        priority = 3;
                        action = () =>
                        {
                            List<Segment> links;
                            segment.End = other.End;
                            segment.Severed = true;
                            foreach (var destination in segment.Destinations)
                            {
                                int index = destination.Sources.IndexOf(other);
                                if (index != -1)
                                    continue;
                                destination.Sources.Add(other);
                            }
                            links = other.StartIsBackwards() ? other.Forwards : other.Branches;
                            if (links.Any((Segment link) =>
                            {
                                return ((link.Start == segment.End) &&
                                        (link.End == segment.Start)) ||
                                        ((link.Start == segment.Start) && (link.End == segment.End));
                            }))
                                return false;
                            links.ForEach((Segment link) =>
                            {
                                link.LinksForEndContaining(other).Add(segment);
                                segment.Forwards.Add(link);
                            });
                            links.Add(segment);
                            segment.Forwards.Add(other);
                            if (debugData != null)
                                debugData.snaps.Add(other.End);
                            return true;
                        };
                    }
                }
                // intersection within radius check
                if (priority <= 2)
                {
                    var e0 = (segment.End - other.Start);
                    var e1 = (other.End - other.Start);
                    Vector2 proj = Vector3.Project(e0, e1);
                    Vector2 pointOnLine = (other.Start + proj);
                    float distance2 = Vector2.SqrMagnitude(segment.End - pointOnLine);
                    float lineProj2 = Mathf.Sign(Vector3.Dot(e0, e1)) * Vector2.SqrMagnitude(proj);
                    float length2 = Vector2.SqrMagnitude(e1);
                    if (distance2 < Config.snapDistance * Config.snapDistance && lineProj2 >= 0 && lineProj2 <= length2)
                    {
                        Vector2 point;
                        point = pointOnLine;
                        priority = 2;
                        action = () =>
                        {
                            float dirDiff = Math.Abs(other.Direction - segment.Direction) % 180.0f;
                            float minDirDiff = Math.Min(dirDiff, Math.Abs(dirDiff - 180.0f));
                            if (minDirDiff < Config.minIntersectionDeviation)
                                return false;
                            IntersectSegments(point, other, segment, segments, quadtree);
                            if (debugData != null)
                                debugData.intersectionsRadius.Add(point);
                            return true;
                        };
                    }
                }
            }

            if (action != null)
                return action();

            return true;
        }

        static List<Tuple<Segment, int>> GlobalGoals(Segment segment)
        {
            List<Tuple<Segment, int>> newSegments = new List<Tuple<Segment, int>>();
            if (!segment.Severed)
            {
                Segment straight = new Segment(segment.End,
                    segment.Direction,
                    segment.Length,
                    segment.Highway,
                    segment.Severed);
                float straightDensity = PopulationDensityMap.DensityOnRoad(straight);
                if (segment.Highway)
                {
                    Segment randomStraight = new Segment(segment.End,
                        segment.Direction + Config.RandomStraightAngle(),
                        segment.Length,
                        segment.Highway,
                        segment.Severed);
                    var randomDensity = PopulationDensityMap.DensityOnRoad(randomStraight);
                    float density;
                    if (randomDensity > straightDensity)
                    {
                        newSegments.Add(Tuple.Create(randomStraight, 0));
                        density = randomDensity;
                    }
                    else
                    {
                        newSegments.Add(Tuple.Create(straight, 0));
                        density = straightDensity;
                    }
                    if (density > Config.highwayBranchPopulationThreshold)
                    {
                        if (UnityEngine.Random.value < Config.highwayBranchProbability)
                        {
                            Segment leftHighwayBranch = new Segment(segment.End,
                                segment.Direction - 90.0f + Config.RandomBranchAngle(),
                                segment.Length,
                                segment.Highway,
                                segment.Severed);
                            newSegments.Add(Tuple.Create(leftHighwayBranch, 0));
                        }
                        else if (UnityEngine.Random.value < Config.highwayBranchProbability)
                        {
                            Segment rightHighwayBranch = new Segment(segment.End,
                                segment.Direction + 90.0f + Config.RandomBranchAngle(),
                                segment.Length,
                                segment.Highway,
                                segment.Severed);
                            newSegments.Add(Tuple.Create(rightHighwayBranch, 0));
                        }
                    }
                }
                else if (straightDensity > Config.streetBranchPopulationThreshold)
                {
                    newSegments.Add(Tuple.Create(straight, 0));
                }
                if (straightDensity > Config.streetBranchPopulationThreshold)
                {
                    if (UnityEngine.Random.value < Config.streetBranchProbability)
                    {
                        Segment leftBranch = new Segment(segment.End,
                            segment.Direction - 90.0f + Config.RandomBranchAngle(),
                            Config.streetSegmentLength,
                            false,
                            false);
                        newSegments.Add(Tuple.Create(leftBranch, segment.Highway ? Config.streetBranchDelayFromHighway : 0));
                    }
                    else if (UnityEngine.Random.value < Config.streetBranchProbability)
                    {
                        Segment rightBranch = new Segment(segment.End,
                            segment.Direction + 90.0f + Config.RandomBranchAngle(),
                            Config.streetSegmentLength,
                            false,
                            false);
                        newSegments.Add(Tuple.Create(rightBranch, segment.Highway ? Config.streetBranchDelayFromHighway : 0));
                    }
                }
            }
            for (int i = 0; i < newSegments.Count; i++)
            {
                Segment newSegment = newSegments[i].Item1;
                newSegment.SetupBranchLinksCallback = () =>
                {
                    foreach (var link in segment.Forwards)
                    {
                        newSegment.Branches.Add(link);
                        link.LinksForEndContaining(segment).Add(newSegment);
                    }
                    segment.Forwards.Add(newSegment);
                    newSegment.Branches.Add(segment);
                    segment.Destinations.Add(newSegment);
                    newSegment.Sources.Add(segment);
                };
            }
            return newSegments;
        }

        static void IntersectSegments(Vector2 intersection, Segment segment0, Segment segment1, List<Segment> segments, Quadtree quadtree)
        {
            bool startIsBackwards = segment0.StartIsBackwards();
            Segment splitPart = new Segment(segment0);
            splitPart.Destinations.Add(segment0);
            AddSegment(splitPart, segments, quadtree);
            splitPart.End = intersection;
            segment0.Start = intersection;
            foreach (var source0 in segment0.Sources)
            {
                int index = source0.Destinations.IndexOf(segment0);
                if (index == -1)
                    continue;
                source0.Destinations[index] = splitPart;
            }
            splitPart.Sources = new List<Segment>(segment0.Sources);
            segment0.Sources.Clear();
            segment0.Sources.Add(splitPart);
            segment0.Sources.Add(segment1);
            splitPart.Branches = new List<Segment>(segment0.Branches);
            splitPart.Forwards = new List<Segment>(segment0.Forwards);
            Segment firstSplit, secondSplit;
            List<Segment> linksToFix;
            if (startIsBackwards)
            {
                firstSplit = splitPart;
                secondSplit = segment0;
                linksToFix = splitPart.Branches;
            }
            else
            {
                firstSplit = segment0;
                secondSplit = splitPart;
                linksToFix = splitPart.Forwards;
            }
            foreach (var link in linksToFix)
            {
                int index = link.Branches.IndexOf(segment0);
                if (index != -1)
                    link.Branches[index] = splitPart;
                else
                {
                    index = link.Forwards.IndexOf(segment0);
                    link.Forwards[index] = splitPart;
                }
            }
            firstSplit.Forwards = new List<Segment>();
            firstSplit.Forwards.Add(segment1);
            firstSplit.Forwards.Add(secondSplit);
            secondSplit.Branches = new List<Segment>();
            secondSplit.Branches.Add(segment1);
            secondSplit.Branches.Add(firstSplit);
            segment1.Forwards.Add(firstSplit);
            segment1.Forwards.Add(secondSplit);
            segment1.End = intersection;
            segment1.Destinations.Clear();
            segment1.Severed = true;
        }

        static void AddSegment(Segment segment, List<Segment> segments, Quadtree quadtree)
        {
            segment.Index = segments.Count;
            segments.Add(segment);
            quadtree.Insert(segment);
        }

        public struct InteractiveGenerationContext
        {
            public List<Segment> segments;
            public Quadtree quadtree;
            public DebugData debugData;
            internal PriorityQueue<Segment, int> priorityQueue;
            internal int globalDerivationStep;
            internal bool ended;

            internal InteractiveGenerationContext(List<Segment> segments, Quadtree quadtree, DebugData debugData, PriorityQueue<Segment, int> priorityQueue)
            {
                this.segments = segments;
                this.quadtree = quadtree;
                this.debugData = debugData;
                this.priorityQueue = priorityQueue;
                globalDerivationStep = 0;
                ended = false;
            }

        }

        public static InteractiveGenerationContext BeginInteractiveGeneration()
        {
            List<Segment> segments;
            Quadtree quadtree;
            DebugData debugData;

            debugData = new DebugData();

            Segment root1 = new Segment(new Vector2(0, 0), new Vector2(Config.highwaySegmentLength, 0), true, false);
            //Segment root2 = new Segment(new Vector2(0, 0), new Vector2(-Config.HIGHWAY_SEGMENT_LENGTH, 0), true, false);
            //root2.Branches.Add(root1);
            //root1.Branches.Add(root2);

            segments = new List<Segment>();
            quadtree = new Quadtree(Config.QuadtreeParams, Config.quadtreeMaxObjects, Config.quadtreeMaxLevels);

            PriorityQueue<Segment, int> priorityQueue = new PriorityQueue<Segment, int>(new Comparison<int>((i1, i2) => i2.CompareTo(i1)));

            priorityQueue.Enqueue(root1, 0);
            //priorityQueue.Enqueue(root2, 0);

            return new InteractiveGenerationContext(segments, quadtree, debugData, priorityQueue);
        }

        public static bool InteractiveGenerationStep(int speed, ref InteractiveGenerationContext context)
        {
            if (context.priorityQueue.Count == 0 || context.segments.Count >= Config.segmentCountLimit || context.globalDerivationStep >= Config.derivationStepLimit)
            {
                if (!context.ended)
                    context.ended = true;
                return false;
            }

            int steps = 0;
            while (steps++ < speed &&
                context.priorityQueue.Count > 0 &&
                context.segments.Count < Config.segmentCountLimit &&
                context.globalDerivationStep++ < Config.derivationStepLimit)
            {
                var item = context.priorityQueue.Dequeue();
                var segment = item.Value;
                if (LocalConstraints(segment, context.segments, context.quadtree, context.debugData))
                {
                    segment.SetupBranchLinks();
                    AddSegment(segment, context.segments, context.quadtree);
                    var queueItems = GlobalGoals(segment);
                    int basePriority = item.Priority + 1;
                    foreach (var queueItem in queueItems)
                        context.priorityQueue.Enqueue(queueItem.Item1, basePriority + queueItem.Item2);
                }
            }
            return true;
        }

        public static void EndInteractiveGeneration(ref InteractiveGenerationContext context)
        {
            while (context.priorityQueue.Count > 0 &&
                context.segments.Count < Config.segmentCountLimit &&
                context.globalDerivationStep++ < Config.derivationStepLimit)
            {
                var item = context.priorityQueue.Dequeue();
                var segment = item.Value;
                if (LocalConstraints(segment, context.segments, context.quadtree, context.debugData))
                {
                    segment.SetupBranchLinks();
                    AddSegment(segment, context.segments, context.quadtree);
                    var queueItems = GlobalGoals(segment);
                    int basePriority = item.Priority + 1;
                    foreach (var queueItem in queueItems)
                        context.priorityQueue.Enqueue(queueItem.Item1, basePriority + queueItem.Item2);
                }
            }

            if (context.ended)
                return;

            context.ended = true;
        }

        public static void Generate(out List<Segment> segments, out Quadtree quadtree, out DebugData debugData)
        {

            Segment root1 = new Segment(new Vector2(0, 0), new Vector2(Config.highwaySegmentLength, 0), true, false);
            Segment root2 = new Segment(new Vector2(0, 0), new Vector2(-Config.highwaySegmentLength, 0), true, false);
            root2.Branches.Add(root1);
            root1.Branches.Add(root2);

            segments = new List<Segment>();
            quadtree = new Quadtree(Config.QuadtreeParams, Config.quadtreeMaxObjects, Config.quadtreeMaxLevels);

            PriorityQueue<Segment, int> priorityQueue = new PriorityQueue<Segment, int>(new Comparison<int>((i1, i2) => i2.CompareTo(i1)));

            priorityQueue.Enqueue(root1, 0);
            priorityQueue.Enqueue(root2, 0);

            debugData = new DebugData();
            int derivationStep = 0;
            while (priorityQueue.Count > 0 && segments.Count < Config.segmentCountLimit && derivationStep++ < Config.derivationStepLimit)
            {
                var item = priorityQueue.Dequeue();
                var segment = item.Value;
                if (LocalConstraints(segment, segments, quadtree, debugData))
                {
                    segment.SetupBranchLinks();
                    AddSegment(segment, segments, quadtree);
                    var queueItems = GlobalGoals(segment);
                    int basePriority = item.Priority + 1;
                    foreach (var queueItem in queueItems)
                        priorityQueue.Enqueue(queueItem.Item1, basePriority + queueItem.Item2);
                }
            }

            // DEBUG:
            if (derivationStep == Config.derivationStepLimit)
                Debug.Log("generation was interrupted for security reasons");
        }

    }

}