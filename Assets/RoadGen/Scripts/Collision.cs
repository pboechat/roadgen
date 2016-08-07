using UnityEngine;
using System.Collections.Generic;

namespace RoadGen
{
    public static class Collision
    {
        public static readonly float EPSILON = 0.001f;

        public static void ArmPoints(Vector2 a, Vector2 b, Vector2 c, float width, out Vector2 elbow, out Vector2 joint)
        {
            var a0 = (a - b).normalized;
            var a1 = (c - b).normalized;
            float r = 0.5f * width;
            float dot = Vector2.Dot(a0, a1);
            if (Mathf.Abs(dot) < (1.0f - RoadGen.Collision.EPSILON))
            {
                Vector2 i;
                LineLineIntersection(new Vector3(-a0.y, a0.x, r), new Vector3(-a1.y, a1.x, -r), out i);
                elbow = (b + i); // left
                LineLineIntersection(new Vector3(-a0.y, a0.x, -r), new Vector3(-a1.y, a1.x, r), out i);
                joint = (b + i); // right
            }
            else
            {
                Vector2 n;
                if (dot > RoadGen.Collision.EPSILON)
                {
                    n = (new Vector2(-a0.y - a1.y, a0.x + a1.x)).normalized;
                    elbow = (b + (-r * n)); // left
                    joint = (b + (r * n)); // right
                }
                else
                {
                    n = (new Vector2(-a0.y + a1.y, a0.x - a1.x)).normalized;
                    elbow = (b + (-r * n)); // left
                    joint = (b + (r * n)); // right
                }
            }
        }

        public static bool RectangleCircleIntersection(Vector2[] corners, Vector2 center, float radius)
        {
            float r2 = radius * radius;
            for (int i = 0; i < corners.Length; i++)
            {
                if (Vector2.SqrMagnitude(center - corners[i]) <= (r2))
                    return true;
            }
            Vector2 e0, e1, p0;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 start = corners[i];
                Vector2 end = corners[(i + 1) % corners.Length];
                e0 = (center - start);
                e1 = (end - start);
                p0 = Vector3.Project(e0, e1);
                float d2 = Vector2.SqrMagnitude(center - (start + p0));
                float pl0 = Mathf.Sign(Vector3.Dot(e0, e1)) * Vector2.SqrMagnitude(p0);
                float l2 = Vector2.SqrMagnitude(e1);
                if (pl0 > 0 && pl0 < l2 && d2 <= r2)
                    return true;
            }
            Vector2 a0 = corners[3] - corners[0],
                    a1 = corners[3] - corners[2];
            e0 = center - corners[0];
            e1 = center - corners[2];
            p0 = Vector3.Project(e0, a0);
            Vector2 p1 = Vector3.Project(e1, a1);
            if (Vector2.Dot(e0, a0) < 0 ||
                Vector2.SqrMagnitude(p0) > Vector2.SqrMagnitude(a0) ||
                Vector2.Dot(e1, a1) < 0 ||
                Vector2.SqrMagnitude(p1) > Vector2.SqrMagnitude(a1))
                return false;
            return true;
        }

        public static Vector2[] GetCorners(Vector2 start, Vector2 end, float width)
        {
            Vector2 direction = (end - start);
            Vector2 perpDir = new Vector2(
                -direction.y,
                direction.x
            );
            Vector2 halfWidthPerpDir = perpDir * (0.5f * width / perpDir.magnitude);
            return new Vector2[]
            {
                (start + halfWidthPerpDir),
                (start - halfWidthPerpDir),
                (end - halfWidthPerpDir),
                (end + halfWidthPerpDir)
            };
        }

        public static void SetCorners(Vector2[] corners, Vector2 center, float direction /* in radians */, float halfDiagonal, float aspectAngle = 0.785398163f /* in radians */)
        {
            float a0 = (aspectAngle + direction),
                  a1 = (direction - aspectAngle);
            corners[0].x = center.x + halfDiagonal * Mathf.Sin(a0);
            corners[0].y = center.y + halfDiagonal * Mathf.Cos(a0);
            corners[1].x = center.x + halfDiagonal * Mathf.Sin(a1);
            corners[1].y = center.y + halfDiagonal * Mathf.Cos(a1);
            corners[2].x = center.x + halfDiagonal * Mathf.Sin(Mathf.PI + a0);
            corners[2].y = center.y + halfDiagonal * Mathf.Cos(Mathf.PI + a0);
            corners[3].x = center.x + halfDiagonal * Mathf.Sin(Mathf.PI + a1);
            corners[3].y = center.y + halfDiagonal * Mathf.Cos(Mathf.PI + a1);
        }


        public static bool RectangleRectangleIntersection(Vector2[] corners0, Vector2[] corners1, out Vector2 offset)
        {
            Vector2[] axes = new Vector2[] { (corners0[3] - corners0[0]),
                (corners0[3] - corners0[2]),
                (corners1[0] - corners1[1]),
                (corners1[0] - corners1[3]) };
            List<Vector2> axisOverlaps = new List<Vector2>(4);
            offset = default(Vector2);
            int i;
            for (i = 0; i < 4; i++)
            {
                Vector2 axis = axes[i];
                List<Vector2> projectedVectorsA = new List<Vector2>(4);
                List<Vector2> projectedVectorsB = new List<Vector2>(4);
                foreach (var corner in corners0)
                    projectedVectorsA.Add(Vector3.Project(corner, axis));
                foreach (var corner in corners1)
                    projectedVectorsB.Add(Vector3.Project(corner, axis));
                List<float> positionsOnAxisA = new List<float>();
                List<float> positionsOnAxisB = new List<float>();
                foreach (var v in projectedVectorsA)
                    positionsOnAxisA.Add(Vector2.Dot(v, axis));
                foreach (var v in projectedVectorsB)
                    positionsOnAxisB.Add(Vector2.Dot(v, axis));
                float maxA = -float.MaxValue, minA = float.MaxValue, maxB = -float.MaxValue, minB = float.MaxValue;
                int maxA_j = -1, minA_j = -1, maxB_j = -1, minB_j = -1;
                for (int j = 0; j < positionsOnAxisA.Count; j++)
                {
                    var a = positionsOnAxisA[j];
                    if (a > maxA)
                    {
                        maxA = a;
                        maxA_j = j;
                    }
                    if (a < minA)
                    {
                        minA = a;
                        minA_j = j;
                    }
                }
                for (int j = 0; j < positionsOnAxisB.Count; j++)
                {
                    var b = positionsOnAxisB[j];
                    if (b > maxB)
                    {
                        maxB = b;
                        maxB_j = j;
                    }
                    if (b < minB)
                    {
                        minB = b;
                        minB_j = j;
                    }
                }
                if (maxA < minB || maxB < minA)
                {
                    return false;
                }
                else
                {
                    Vector2 diff1 = (projectedVectorsA[maxA_j] - projectedVectorsB[minB_j]);
                    Vector2 diff2 = (projectedVectorsB[maxB_j] - projectedVectorsA[minA_j]);
                    if (diff1.SqrMagnitude() < diff2.SqrMagnitude())
                        axisOverlaps.Add(diff1);
                    else
                        axisOverlaps.Add((diff2 * -1));
                }
            }

            if (axisOverlaps.Count == 0)
                return false;

            float minLength2 = float.MaxValue;
            i = -1;
            for (int j = 0; j < axisOverlaps.Count; j++)
            {
                var length2 = axisOverlaps[j].SqrMagnitude();
                if (length2 < minLength2)
                {
                    minLength2 = length2;
                    i = j;
                }
            }
            offset = (axisOverlaps[i] * -1.0f);
            return true;
        }

        public static bool LineLineIntersection(Vector3 l1, Vector3 l2, out Vector2 intersection)
        {
            float det = l2.x * l1.y - l1.x * l2.y;
            if (Mathf.Abs(det) != 0.0)
            {
                intersection = 1.0f / det * new Vector2(l2.y * l1.z - l1.y * l2.z, l1.x * l2.z - l2.x * l1.z);
                return true;
            }
            intersection = default(Vector2);
            return false;
        }

        public static bool LineSegmentLineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2 intersection, out float t)
        {
            var r = (p2 - p1);
            var s = (q2 - q1);
            var num = Vector3.Cross((q1 - p1), r).z;
            var den = Vector3.Cross(r, s).z;
            if (num == 0 && den == 0)
            {
                // lines are collinear, so do they overlap?
                // return ((q1.x - p1.x < 0) != (q1.x - p2.x < 0) != (q2.x - p1.x < 0) != (q2.x - p2.x < 0)) ||
                //   ((q1.y - p1.y < 0) != (q1.y - p2.y < 0) != (q2.y - p1.y < 0) != (q2.y - p2.y < 0));
                intersection = default(Vector2);
                t = -1;
                return false;
            }
            if (den == 0)
            {
                // lines are parallel
                intersection = default(Vector2);
                t = -1;
                return false;
            }
            var u = num / den;
            t = Vector3.Cross((q1 - p1), s).z / den;
            bool a = (t > EPSILON) && (t < 1.0f - EPSILON) && (u > EPSILON) && (u < 1.0f - EPSILON);
            if (a)
            {
                intersection = new Vector2(p1.x + t * r.x, p1.y + t * r.y);
                return true;
            }
            intersection = default(Vector2);
            return a;
        }

    }

}
