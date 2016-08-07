using UnityEngine;

namespace RoadGen
{
    public static class PopulationDensityMap
    {
        private static float width = -1;
        private static float height = -1;
        private static float offset = -1;
        private static float twoOffset = -1;

        private static float Width
        {
            get
            {
                if (width == -1)
                {
                    // NOTE: downscale of 4
                    width = Config.QuadtreeParams.width * 0.25f;
                }
                return width;
            }
        }

        private static float Height
        {
            get
            {
                if (height == -1)
                {
                    // NOTE: downscale of 4
                    height = Config.QuadtreeParams.height * 0.25f;
                }
                return height;
            }
        }

        private static float Offset
        {
            get
            {
                if (offset == -1)
                    offset = Width * 0.025f;
                return offset;
            }
        }

        private static float TwoOffset
        {
            get
            {
                if (twoOffset == -1)
                    twoOffset = Offset * 2;
                return twoOffset;
            }
        }

        public static void ResetCache()
        {
            width = height = offset = twoOffset = -1;
        }

        public static float DensityOnRoad(Segment segment)
        {
            return (DensityAt(segment.Start.x, segment.Start.y) + DensityAt(segment.End.x, segment.End.y)) / 2;
        }

        public static float DensityAt(float x, float y)
        {
            float value1, value2, value3;
            value1 = (Perlin.Simplex2(x / (Width * 0.5f), y / (Height * 0.5f)) + 1) * 0.5f;
            value2 = (Perlin.Simplex2(x / Width + Offset, y / Height + Offset) + 1) * 0.5f;
            value3 = (Perlin.Simplex2(x / Width + TwoOffset, y / height + TwoOffset) + 1) * 0.5f;
            return Mathf.Pow((value1 * value2 + value3) * 0.5f, 2);
        }

    };
}
