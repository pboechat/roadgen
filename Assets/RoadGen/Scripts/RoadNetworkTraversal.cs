using System.Collections.Generic;

namespace RoadGen
{
    public class RoadNetworkTraversal
    {
        public const int HIGHWAYS_MASK = 1;
        public const int STREETS_MASK = 2;

        public delegate bool Visitor0(Segment s0);
        public delegate bool Visitor1(Segment s0, Segment s1);
        public delegate bool Visitor2<T>(Segment s0, ref T context);
        public delegate bool Visitor3<T, U>(Segment s0, ref T context, U inData, out U outData);
        public delegate bool Visitor4<T, U>(Segment s0, Segment s1, ref T context, U inData, out U outData);

        public static void PreOrder(Segment s0, Visitor0 visitor, int mask, ref HashSet<Segment> visited)
        {
            if (s0.Highway && (mask & HIGHWAYS_MASK) == 0 || !s0.Highway && (mask & STREETS_MASK) == 0)
                return;
            if (visited.Contains(s0))
                return;
            visited.Add(s0);
            if (!visitor(s0))
                return;
            foreach (var destination in s0.Destinations)
                PreOrder(destination, visitor, mask, ref visited);
        }

        public static void PreOrder(Segment s0, Visitor1 visitor, int mask, ref HashSet<Segment> visited)
        {
            PreOrder(null, s0, visitor, mask, ref visited);
        }

        public static void PreOrder(Segment s0, Segment s1, Visitor1 visitor, int mask, ref HashSet<Segment> visited)
        {
            if (s1.Highway && (mask & HIGHWAYS_MASK) == 0 || !s1.Highway && (mask & STREETS_MASK) == 0)
                return;
            if (visited.Contains(s1))
                return;
            visited.Add(s1);
            if (s1.Destinations.Count == 0)
            {
                if (!visitor(s1, null))
                    return;
            }
            else
            {
                if (!visitor(s0, s1))
                    return;
                foreach (var s2 in s1.Destinations)
                    PreOrder(s1, s2, visitor, mask, ref visited);
            }
        }

        public static void PreOrder<T>(Segment s0, ref T context, Visitor2<T> visitor, int mask, ref HashSet<Segment> visited)
        {
            if (s0.Highway && (mask & HIGHWAYS_MASK) == 0 || !s0.Highway && (mask & STREETS_MASK) == 0)
                return;
            if (visited.Contains(s0))
                return;
            visited.Add(s0);
            if (!visitor(s0, ref context))
                return;
            foreach (var destination in s0.Destinations)
                PreOrder(destination, ref context, visitor, mask, ref visited);
        }

        public static void PreOrder<T, U>(Segment s0, ref T context, U inData, Visitor3<T, U> visitor, int mask, ref HashSet<Segment> visited)
        {
            if (s0.Highway && (mask & HIGHWAYS_MASK) == 0 || !s0.Highway && (mask & STREETS_MASK) == 0)
                return;
            if (visited.Contains(s0))
                return;
            visited.Add(s0);
            U outData;
            if (!visitor(s0, ref context, inData, out outData))
                return;
            foreach (var destination in s0.Destinations)
                PreOrder(destination, ref context, outData, visitor, mask, ref visited);
        }

        public static void PreOrder<T, U>(Segment s0, ref T context, U inData, Visitor4<T, U> visitor, int mask, ref HashSet<Segment> visited)
        {
            PreOrder<T, U>(null, s0, ref context, inData, visitor, mask, ref visited);
        }

        public static void PreOrder<T, U>(Segment s0, Segment s1, ref T context, U inData, Visitor4<T, U> visitor, int mask, ref HashSet<Segment> visited)
        {
            if (s1.Highway && (mask & HIGHWAYS_MASK) == 0 || !s1.Highway && (mask & STREETS_MASK) == 0)
                return;
            if (visited.Contains(s1))
                return;
            visited.Add(s1);
            U outData;
            if (s1.Destinations.Count == 0)
            {
                if (!visitor(s1, null, ref context, inData, out outData))
                    return;
            }
            else
            {
                if (!visitor(s0, s1, ref context, inData, out outData))
                    return;
                foreach (var s2 in s1.Destinations)
                    PreOrder(s1, s2, ref context, outData, visitor, mask, ref visited);
            }
        }

    }

}