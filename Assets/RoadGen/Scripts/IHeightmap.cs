namespace RoadGen
{
    public interface IHeightmap : IOrderedScript
    {
        float GetHeight(float x, float y);

    }

}