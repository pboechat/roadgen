using UnityEngine;
using System;

namespace RoadGen
{
    public class Config
    {
        private readonly static float BRANCH_ANGLE_DEVIATION = 3;
        private readonly static float FORWARD_ANGLE_DEVIATION = 15;

        private static float RandomAngle(float limit)
        {
            float nonUniformNorm, value;
            nonUniformNorm = Mathf.Pow(Math.Abs(limit), 3);
            value = 0;
            while (value == 0 || UnityEngine.Random.value < Math.Pow(Math.Abs(value), 3) / nonUniformNorm)
                value = UnityEngine.Random.Range(-limit, +limit);
            return value;
        }

        private static Rect quadtreeParams = new Rect(-20000, -20000, 40000, 40000);

        public static Rect QuadtreeParams
        {
            get
            {
                return quadtreeParams;
            }
            set
            {
                quadtreeParams = value;
                PopulationDensityMap.ResetCache();
            }
        }

        public static int quadtreeMaxObjects = 10;
        public static int quadtreeMaxLevels = 10;
        public static float streetSegmentLength = 300;
        public static float highwaySegmentLength = 400;
        public static float streetSegmentWidth = 6;
        public static float highwaySegmentWidth = 16;
        public static int derivationStepLimit = 10000;
        public static float streetBranchProbability = 0.4f;
        public static float highwayBranchProbability = 0.05f;
        public static float streetBranchPopulationThreshold = 0.1f;
        public static float highwayBranchPopulationThreshold = 0.1f;
        public static int highwayDelay = 1;
        public static int streetBranchDelayFromHighway = 5;
        public static float minIntersectionDeviation = 30;
        public static int segmentCountLimit = 500;
        public static float snapDistance = 50;
        public static int settlementSpawnDelay = 10;
        public static int settlementDensity = 10;
        public static float allotmentMinHalfDiagonal = 40.0f;
        public static float allotmentMaxHalfDiagonal = 60.0f;
        public static float allotmentMinAspect = 1.15f;
        public static float allotmentMaxAspect = 1.5f;
        public static int allotmentPlacementLoopLimit = 3;
        public static float settlementRadius = 400.0f;
        public static float settlementInCrossingProbability = 0.9f;
        public static float settlementInHighwayProbability = 0.1f;

        public static float RandomBranchAngle()
        {
            return RandomAngle(BRANCH_ANGLE_DEVIATION);
        }

        public static float RandomStraightAngle()
        {
            return RandomAngle(FORWARD_ANGLE_DEVIATION);
        }

    }

}
