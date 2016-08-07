namespace RoadGen
{
    public interface IMap : IOrderedScript
    {
        float GetWidth();
        float GetHeight();
        float GetMinX();
        float GetMaxX();
        float GetMinY();
        float GetMaxY();
        float GetNormalizedValue(float x, float y);

    }

}