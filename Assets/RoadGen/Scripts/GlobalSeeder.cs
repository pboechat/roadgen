using UnityEngine;
using System;

public class GlobalSeeder : MonoBehaviour
{
    public bool useTimeAsSeed = true;
    public int seed = 0;

    static int GetSecondsSinceEpoch()
    {
        var seconds = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        if (seconds > (double)int.MaxValue)
            throw new Exception();
        return (int)seconds;
    }

    void Start()
    {
        seed = (useTimeAsSeed) ? GetSecondsSinceEpoch() : seed;
        UnityEngine.Random.seed = seed;
        RoadGen.Perlin.Seed(seed);
        Debug.Log("Seed: " + seed);
    }

}
