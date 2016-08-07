namespace RoadGen
{
    public class AABB
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public object reference;

        public AABB(float x, float y, float width, float height, object reference)
        {
            this.x = x; this.y = y; this.width = width; this.height = height; this.reference = reference;
        }

    }
}
