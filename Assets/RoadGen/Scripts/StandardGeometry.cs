using UnityEngine;

namespace RoadGen
{
    public static class StandardGeometry
    {
        public static Mesh CreateCubeMesh(Vector2[] corners, float height = 1, float z = 0)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
            // back
            new Vector3(corners[3].x, z + height, corners[3].y),
            new Vector3(corners[0].x, z + height, corners[0].y),
            new Vector3(corners[0].x, z, corners[0].y),
            new Vector3(corners[3].x, z, corners[3].y),

            // right
            new Vector3(corners[0].x, z + height, corners[0].y),
            new Vector3(corners[1].x, z + height, corners[1].y),
            new Vector3(corners[1].x, z, corners[1].y),
            new Vector3(corners[0].x, z, corners[0].y),

            // forward
            new Vector3(corners[1].x, z + height, corners[1].y),
            new Vector3(corners[2].x, z + height, corners[2].y),
            new Vector3(corners[2].x, z, corners[2].y),
            new Vector3(corners[1].x, z, corners[1].y),

            // left
            new Vector3(corners[2].x, z + height, corners[2].y),
            new Vector3(corners[3].x, z + height, corners[3].y),
            new Vector3(corners[3].x, z, corners[3].y),
            new Vector3(corners[2].x, z, corners[2].y),

            // up
            new Vector3(corners[2].x, z + height, corners[2].y),
            new Vector3(corners[1].x, z + height, corners[1].y),
            new Vector3(corners[0].x, z + height, corners[0].y),
            new Vector3(corners[3].x, z + height, corners[3].y),

            // down
            new Vector3(corners[1].x, z, corners[1].y),
            new Vector3(corners[2].x, z, corners[2].y),
            new Vector3(corners[3].x, z, corners[3].y),
            new Vector3(corners[0].x, z, corners[0].y)

            };
            mesh.triangles = new int[]
            {
            0, 1, 2,
            0, 2, 3,

            4, 5, 6,
            4, 6, 7,

            8, 9, 10,
            8, 10, 11,

            12, 13, 14,
            12, 14, 15,

            16, 17, 18,
            16, 18, 19,

            20, 21, 22,
            20, 22, 23,

            };
            mesh.uv = new Vector2[]
            {
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),

            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),

            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),

            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),

            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),

            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0)

            };

            mesh.normals = new Vector3[]
            {
            Vector3.back,
            Vector3.back,
            Vector3.back,
            Vector3.back,

            Vector3.right,
            Vector3.right,
            Vector3.right,
            Vector3.right,

            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,

            Vector3.left,
            Vector3.left,
            Vector3.left,
            Vector3.left,

            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,

            Vector3.down,
            Vector3.down,
            Vector3.down,
            Vector3.down

            };
            return mesh;
        }

    }

}