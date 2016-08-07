using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace RoadGen
{
    public static class RoadNetworkCollisionMap
    {
        public class Stroke
        {
            public class Pixel
            {
                public int x, y;
                public float distanceToCenter;
                public float sqrDistanceToCenter;
                internal Stroke source;

                public float GetNormalizedDistanceToCenter()
                {
                    return distanceToCenter / source.halfSize;
                }

                public int GetMapX()
                {
                    return source.x + x;
                }

                public int GetMapY()
                {
                    return source.y + y;
                }

            }

            public int size;
            public int halfSize;
            public int halfSize2;
            public int x, y;
            public Segment segment;
            public Vector2 segmentCoords;
            public Vector2 segmentDirection;
            public float segmentStep;
            private Pixel currentPixel;

            public Stroke()
            {
                currentPixel = new Pixel();
                currentPixel.source = this;
            }

            public Pixel CurrentPixel
            {
                get
                {
                    return currentPixel;
                }
            }

        }

        public delegate bool WorldToMapCoords(float wX, float wY, out int mX, out int mY);
        public delegate void BrushFunction<T>(Stroke stroke, int mapSize, T[,] map);
        public delegate T DrawFunction<T>(Stroke stroke, T currentValue);

        private static void FindStrokePixelBoundaries(Stroke stroke, int mapSize, out int minY, out int maxY, out int minX, out int maxX)
        {
            if (stroke.y < stroke.halfSize)
                minY = -stroke.y;
            else
                minY = -stroke.halfSize;
            if (stroke.y + stroke.halfSize > mapSize - 1)
                maxY = mapSize - 1 - stroke.y;
            else
                maxY = stroke.halfSize;
            if (stroke.x < stroke.halfSize)
                minX = -stroke.x;
            else
                minX = -stroke.halfSize;
            if (stroke.x + stroke.halfSize > mapSize - 1)
                maxX = mapSize - 1 - stroke.x;
            else
                maxX = stroke.halfSize;
        }

        public static void CircleBrush<T>(Stroke stroke, int mapSize, T[,] map, DrawFunction<T> drawFunction)
        {
            int minSPY, maxSPY, minSPX, maxSPX;
            FindStrokePixelBoundaries(stroke, mapSize, out minSPY, out maxSPY, out minSPX, out maxSPX);
            for (stroke.CurrentPixel.y = minSPY; stroke.CurrentPixel.y <= maxSPY; stroke.CurrentPixel.y++)
            {
                int sY2 = stroke.CurrentPixel.y * stroke.CurrentPixel.y;
                for (stroke.CurrentPixel.x = minSPX; stroke.CurrentPixel.x <= maxSPX; stroke.CurrentPixel.x++)
                {
                    stroke.CurrentPixel.distanceToCenter = Mathf.Sqrt(stroke.CurrentPixel.x * stroke.CurrentPixel.x + sY2);
                    if (stroke.CurrentPixel.distanceToCenter <= stroke.halfSize)
                    {
                        int mX = stroke.CurrentPixel.GetMapX(),
                            mY = stroke.CurrentPixel.GetMapY();
                        map[mX, mY] = drawFunction(stroke, map[mX, mY]);
                    }
                }
            }
        }

        public static void SquaredCircleBrush<T>(Stroke stroke, int mapSize, T[,] map, DrawFunction<T> drawFunction)
        {
            int minSPY, maxSPY, minSPX, maxSPX;
            FindStrokePixelBoundaries(stroke, mapSize, out minSPY, out maxSPY, out minSPX, out maxSPX);
            for (stroke.CurrentPixel.y = minSPY; stroke.CurrentPixel.y <= maxSPY; stroke.CurrentPixel.y++)
            {
                int sY2 = stroke.CurrentPixel.y * stroke.CurrentPixel.y;
                for (stroke.CurrentPixel.x = minSPX; stroke.CurrentPixel.x <= maxSPX; stroke.CurrentPixel.x++)
                {
                    stroke.CurrentPixel.sqrDistanceToCenter = stroke.CurrentPixel.x * stroke.CurrentPixel.x + sY2;
                    if (stroke.CurrentPixel.sqrDistanceToCenter <= stroke.halfSize2)
                    {
                        int mX = stroke.CurrentPixel.GetMapX(),
                            mY = stroke.CurrentPixel.GetMapY();
                        map[mX, mY] = drawFunction(stroke, map[mX, mY]);
                    }
                }
            }
        }

        public static void SolidCircleBrush(Stroke stroke, int mapSize, float[,] map)
        {
            int minSPY, maxSPY, minSPX, maxSPX;
            FindStrokePixelBoundaries(stroke, mapSize, out minSPY, out maxSPY, out minSPX, out maxSPX);
            for (int sPY = minSPY; sPY <= maxSPY; sPY++)
            {
                int sY2 = sPY * sPY;
                for (int sPX = minSPX; sPX <= maxSPX; sPX++)
                    if (sPX * sPX + sY2 <= stroke.halfSize2)
                        map[stroke.x + sPX, stroke.y + sPY] = 1;
            }
        }

        public static void SmoothCircleBrush(Stroke stroke0, int mapSize, float[,] map)
        {
            CircleBrush(stroke0, mapSize, map, (stroke1, currentValue) => currentValue + Mathf.Lerp(1, 0, stroke1.CurrentPixel.GetNormalizedDistanceToCenter()));
        }

        public static void CubicEaseOutCircleBrush(Stroke stroke0, int mapSize, float[,] map)
        {
            CircleBrush(stroke0, mapSize, map, (stroke1, currentValue) => currentValue + EasingHelper.EaseOutCubic(1, 0, stroke1.CurrentPixel.GetNormalizedDistanceToCenter(), 1));
        }

        public static void CubicEaseInCircleBrush(Stroke stroke0, int mapSize, float[,] map)
        {
            CircleBrush(stroke0, mapSize, map, (stroke1, currentValue) => currentValue + EasingHelper.EaseInCubic(1, 0, stroke1.CurrentPixel.GetNormalizedDistanceToCenter(), 1));
        }

        public static void SquaredSmoothCircleBrush(Stroke stroke0, int mapSize, float[,] map)
        {
            SquaredCircleBrush(stroke0, mapSize, map, (stroke1, currentValue) => currentValue + Mathf.Lerp(1, 0, stroke1.CurrentPixel.GetNormalizedDistanceToCenter()));
        }

        public static void Build<T>(List<Segment> segments, int mask, float samplingStep, int strokeSize, BrushFunction<T> brushFunction, int mapSize, WorldToMapCoords worldToMapCoords, T[,] map)
        {
            Stroke stroke = new Stroke();
            stroke.halfSize = Mathf.CeilToInt(strokeSize * 0.5f);
            stroke.halfSize2 = stroke.halfSize * stroke.halfSize;
            HashSet<Segment> visited = new HashSet<Segment>();
            foreach (var segment in segments)
            {
                RoadNetworkTraversal.PreOrder(segment, (s0) =>
                {
                    stroke.segment = s0;
                    stroke.segmentDirection = (s0.End - s0.Start) / s0.Length;
                    int numSteps = Mathf.CeilToInt(s0.Length / samplingStep);
                    var segmentStep = 1.0f / numSteps;
                    var positionIncrement = s0.Length / numSteps * stroke.segmentDirection;
                    stroke.segmentStep = 0;
                    stroke.segmentCoords = s0.Start;
                    int x, y;
                    for (int step = 0; step < numSteps; step++, stroke.segmentCoords += positionIncrement, stroke.segmentStep += segmentStep)
                    {
                        if (!worldToMapCoords(stroke.segmentCoords.x, stroke.segmentCoords.y, out x, out y))
                            break;
                        stroke.x = x;
                        stroke.y = y;
                        brushFunction(stroke, mapSize, map);
                    }
                    return true;
                }, mask, ref visited);
            }
        }

        public static void Normalize(int mapSize, float[,] map)
        {
            float maxValue = -float.MaxValue;
            for (int y = 0; y < mapSize; y++)
                for (int x = 0; x < mapSize; x++)
                {
                    float value = map[x, y];
                    if (value > maxValue)
                        maxValue = value;
                }
            for (int y = 0; y < mapSize; y++)
                for (int x = 0; x < mapSize; x++)
                    map[x, y] /= maxValue;
        }

        public static void SaveMapToFile(string fileName, float[,] map, bool invert = true, bool normalize = false)
        {
            int width = map.GetLength(0), height = map.GetLength(1);
            float maxValue = 0;
            if (normalize)
            {
                maxValue = -float.MaxValue;
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        float value = map[x, y];
                        if (value > maxValue)
                            maxValue = value;
                    }
            }
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            Color[] pixels = new Color[width * height];
            for (int y = 0, i = 0; y < height; y++)
                for (int x = 0; x < width; x++, i++)
                {
                    float value = map[x, y];
                    if (normalize)
                        value = value / maxValue;
                    if (invert)
                        value = 1 - value;
                    pixels[i] = new Color(value, value, value, 1.0f);
                }
            texture.SetPixels(pixels);
            texture.Apply();
            var bytes = texture.EncodeToPNG();
            var fileStream = File.Open(Application.dataPath + "/" + fileName, FileMode.Create);
            var binary = new BinaryWriter(fileStream);
            binary.Write(bytes);
            fileStream.Close();
        }

    }

}