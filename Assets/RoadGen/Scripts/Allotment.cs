using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadGen
{
    public class Allotment : ICollidable
    {
        private Vector2 center;
        private float direction; // in radians
        private float halfDiagonal;
        private float width;
        private float height;
        private float aspectRatio;
        private float aspectAngle; // in radians
        private RectangleCollider collider;

        public Allotment(Vector2 center, float direction /* in degrees */, float halfDiagonal, float aspectRatio = 1)
        {
            this.center = center;
            this.direction = direction * Mathf.Deg2Rad;
            this.halfDiagonal = halfDiagonal;
            aspectAngle = Mathf.Atan(aspectRatio);
            height = GetHeight(halfDiagonal, aspectRatio);
            width = height * aspectRatio;
            this.aspectRatio = aspectRatio;
            collider = new RectangleCollider(this, new Vector2[4]);
            UpdateCorners();
        }

        public static float GetHeight(float halfDiagonal, float aspectRatio)
        {
            return halfDiagonal * 2.0f / Mathf.Sqrt(1.0f + aspectRatio * aspectRatio);
        }

        public static float GetWidth(float halfDiagonal, float aspectRatio)
        {
            return GetHeight(halfDiagonal, aspectRatio) * aspectRatio;
        }

        public float Width
        {
            get
            {
                return width;
            }
        }

        public float Height
        {
            get
            {
                return height;
            }
        }

        public float HalfDiagonal
        {
            get
            {
                return halfDiagonal;
            }
        }

        public float AspectRatio
        {
            get
            {
                return aspectRatio;
            }
        }

        private void UpdateCorners()
        {
            var corners = collider.Corners;
            RoadGen.Collision.SetCorners(corners, center, direction, halfDiagonal, aspectAngle);
            collider.Corners = corners;
        }

        public Collider GetCollider()
        {
            return collider;
        }

        public Vector2 Center
        {
            get
            {
                return center;
            }
            set
            {
                center = value;
                UpdateCorners();
            }
        }

        public void UpdateCenterAndDirection(Vector2 center, float direction /* in degrees */)
        {
            this.center = center;
            this.direction = direction * Mathf.Deg2Rad;
            UpdateCorners();
        }

        public void UpdateWidthAndHeight(float width, float height)
        {
            this.width = width;
            this.height = height;
            halfDiagonal = Mathf.Sqrt(width * width + height * height) * 0.5f;
            UpdateCorners();
        }

        public Vector2[] Corners
        {
            get
            {
                return collider.Corners;
            }
        }

        public float Direction
        {
            get
            {
                return direction;
            }
        }

    }

}
