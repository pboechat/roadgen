using UnityEngine;
using System;
using System.Collections.Generic;

namespace RoadGen
{
    public class Segment : ICollidable
    {
        private int index;
        private int roadRevision;
        private int directionRevision;
        private int lengthRevision;
        private LineCollider collider;
        private float cachedDirection;
        private float cachedLength;
        private bool highway;
        private bool severed;
        private List<Segment> sources;
        private List<Segment> destinations;
        private List<Segment> branches;
        private List<Segment> forwards;
        private Action setupBranchLinksCallback;

        public Segment(Vector2 start, Vector2 end, bool highway = false, bool severed = false)
        {
            index = -1;
            roadRevision = 0;
            directionRevision = -1;
            lengthRevision = -1;
            cachedDirection = -1;
            cachedLength = -1;
            float width = highway ? Config.highwaySegmentWidth : Config.streetSegmentWidth;
            collider = new LineCollider(this, start, end, width);
            sources = new List<Segment>();
            destinations = new List<Segment>();
            branches = new List<Segment>();
            forwards = new List<Segment>();
            setupBranchLinksCallback = null;
            this.highway = highway;
            this.severed = severed;
        }

        private static Vector2 ComputeEnd(Vector2 start, float direction, float length)
        {
            return new Vector2(
                start.x + length * Mathf.Sin(direction * Mathf.Deg2Rad),
                start.y + length * Mathf.Cos(direction * Mathf.Deg2Rad)
            );
        }

        public Segment(Segment other) : this(other.Start, other.End, other.highway, other.severed)
        {
        }

        public Segment(Vector2 start, float direction, float length, bool highway, bool severed) : this(start, ComputeEnd(start, direction, length), highway, severed)
        {
        }

        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }

        public Vector2 Start
        {
            get
            {
                return collider.Start;
            }
            set
            {
                collider.Start = value;
                roadRevision++;
            }
        }

        public Vector2 End
        {
            get
            {
                return collider.End;
            }
            set
            {
                collider.End = value;
                roadRevision++;
            }
        }

        public float Width
        {
            get
            {
                return collider.Width;
            }
            set
            {
                collider.Width = value;
                roadRevision++;
            }
        }

        public Action SetupBranchLinksCallback
        {
            set
            {
                setupBranchLinksCallback = value;
            }
        }

        public List<Segment> Sources
        {
            get
            {
                return sources;
            }
            set
            {
                sources = value;
            }
        }

        public List<Segment> Destinations
        {
            get
            {
                return destinations;
            }
            set
            {
                destinations = value;
            }
        }

        public List<Segment> Branches
        {
            get
            {
                return branches;
            }
            set
            {
                branches = value;
            }
        }

        public List<Segment> Forwards
        {
            get
            {
                return forwards;
            }
            set
            {
                forwards = value;
            }
        }

        public bool Highway
        {
            get
            {
                return highway;
            }
        }

        public bool Severed
        {
            get
            {
                return severed;
            }
            set
            {
                severed = value;
            }
        }

        public float Direction
        {
            get
            {
                Vector2 direction;
                if (directionRevision != roadRevision)
                {
                    directionRevision = roadRevision;
                    direction = (End - Start);
                    if (Length == 0)
                        cachedDirection = 0;
                    else
                        cachedDirection = Mathf.Sign(direction.x) * Mathf.Acos(direction.y / Length) * Mathf.Rad2Deg;
                }
                return cachedDirection;
            }
        }

        public float Length
        {
            get
            {
                if (lengthRevision != roadRevision)
                {
                    lengthRevision = roadRevision;
                    cachedLength = (End - Start).magnitude;
                }
                return cachedLength;
            }
        }

        public void SetupBranchLinks()
        {
            if (setupBranchLinksCallback != null)
                setupBranchLinksCallback();
        }

        public List<Segment> LinksForEndContaining(Segment segment)
        {
            if (branches.IndexOf(segment) != -1)
                return branches;
            else if (forwards.IndexOf(segment) != -1)
                return forwards;
            else
                return null;
        }

        public bool StartIsBackwards()
        {
            if (branches.Count > 0)
                return (branches[0].Start == Start) || (branches[0].Start == End);
            else if (forwards.Count > 0)
                return (forwards[0].Start == End) || (forwards[0].End == End);
            else
                return false;
        }

        public bool Intersect(Segment other, out Vector2 intersection, out float t)
        {
            return Collision.LineSegmentLineSegmentIntersection(Start, End, other.Start, other.End, out intersection, out t);
        }

        public Collider GetCollider()
        {
            return collider;
        }

        public override int GetHashCode()
        {
            return index.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is Segment)
                return index == ((Segment)obj).index;
            return false;
        }

    }

}
