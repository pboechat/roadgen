using System.Diagnostics;
using System.Collections.Generic;
using NUnit.Framework;
using RoadGen;

[TestFixture]
public class BitOpsHelperPerformanceTests
{
    [Test]
    public void test_remove_random_bit_vs_hashset_performance()
    {
        HashSet<int> hashSet = new HashSet<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
        21, 22, 23, 24, 25, 26, 27, 28, 29, 30 };
        int bitmask = 2147483647;
        Stopwatch stopWatch = new Stopwatch();

        int[] selection0 = new int[31];
        int[] selection1 = new int[31];


        // ---

        UnityEngine.Random.seed = 0;
        int c = 0;
        stopWatch.Start();
        while (bitmask != 0)
            selection0[c++] = BitOpsHelper.RemoveRandomBit(ref bitmask);
        stopWatch.Stop();
        var t0 = stopWatch.ElapsedTicks;

        UnityEngine.Random.seed = 0;
        c = 0;
        stopWatch.Start();
        while (hashSet.Count > 0)
        {
            int i = UnityEngine.Random.Range(0, hashSet.Count);
            var e = hashSet.GetEnumerator();
            while (i-- >= 0)
                e.MoveNext();
            hashSet.Remove(selection1[c++] = e.Current);
        }
        stopWatch.Stop();
        var t1 = stopWatch.ElapsedTicks;

        // ---

        //UnityEngine.Debug.Log("t0: " + t0);
        //UnityEngine.Debug.Log("t1: " + t1);
        //string str0 = "", str1 = "";
        //for (int i = 0; i < 31; i++)
        //{
        //    str0 += selection0[i] + ", ";
        //    str1 += selection1[i] + ", ";
        //}
        //UnityEngine.Debug.Log("selection0: " + str0);
        //UnityEngine.Debug.Log("selection1: " + str1);

        Assert.AreIdenticalSets(selection0, selection1, (a, b) => a == b);
        Assert.IsTrue(t0 < t1);
    }

}
