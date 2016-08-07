namespace RoadGen
{
    /// <summary>
    /// source: http://gizma.com/easing/
    /// </summary>
    public static class EasingHelper
    {
        public static float EaseInCubic(float t, float b, float c, float d)
        {
            t /= d;
            return c * t * t * t + b;
        }

        public static float EaseOutCubic(float t, float b, float c, float d)
        {
            t /= d;
            t--;
            return c * (t * t * t + 1) + b;
        }

    }

}