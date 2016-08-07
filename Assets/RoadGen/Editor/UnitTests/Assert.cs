using UnityEngine;
using System;
using System.Collections.Generic;
using NUnit.Framework;

public class Assert : NUnit.Framework.Assert
{
    public static void AreApproximatelyEqual(float expected, float actual)
    {
        if (!Mathf.Approximately(expected, actual))
            Assert.Fail("Expected: " + expected + "\nBut was: " + actual);
    }

    public static void AreApproximatelyEqual(Vector2 expected, Vector2 actual)
    {
        if (!Mathf.Approximately(expected.x, actual.x) || !Mathf.Approximately(expected.y, actual.y))
            Assert.Fail("Expected: " + expected + "\nBut was: " + actual);
    }

    private static bool FloatCompare(float a, float b, int precision = -1)
    {
        if (precision < 0)
            return Mathf.Approximately(a, b);
        else
        {
            float multiplier = Mathf.Pow(10, precision);
            return Mathf.RoundToInt(a * multiplier) == Mathf.RoundToInt(b * multiplier);
        }
    }

    public static void AreIdenticalSets<T, U>(IEnumerable<T> A, IEnumerable<U> B, Func<T, U, bool> comparison)
    {
        IEnumerator<T> e0 = A.GetEnumerator();
        IEnumerator<U> e1 = B.GetEnumerator();
        int i = 0;
        do
        {
            bool c1 = e0.MoveNext(), c2 = e1.MoveNext();
            if (c1 ^ c2)
                Assert.Fail("sets don't have the same size");
            if (!c1)
                break;
            if (!comparison(e0.Current, e1.Current))
                Assert.Fail("B[" + i + "]\n\tExpected: " + e0.Current + "\n\tBut was: " + e1.Current);
        }
        while (++i > 0);
    }

    public static void AreIdenticalSets<T, U>(IEnumerable<T[]> A, IEnumerable<U[]> B, Func<T, U, bool> comparison)
    {
        IEnumerator<T[]> e0 = A.GetEnumerator();
        IEnumerator<U[]> e1 = B.GetEnumerator();
        int i = 0;
        do
        {
            bool c1 = e0.MoveNext(), c2 = e1.MoveNext();
            if (c1 ^ c2)
                Assert.Fail("sets don't have the same size");
            if (!c1)
                break;
            var a = e0.Current;
            var b = e1.Current;
            if (a.Length != b.Length)
                Assert.Fail("sets don't have the same size");
            for (int j = 0; j < a.Length; j++)
                if (!comparison(a[j], b[j]))
                    Assert.Fail("B[" + i + "][" + j + "]\n\tExpected: " + a[j] + "\n\tBut was: " + b[j]);
        }
        while (++i > 0);
    }

    public static void AreIdenticalFloatSets(IEnumerable<float> A, IEnumerable<float> B, int precision = -1)
    {
        AreIdenticalSets<float, float>(A, B, (a, b) => FloatCompare(a, b, precision));
    }

    public static void AreIdenticalFloatSets(IEnumerable<float[]> A, IEnumerable<float[]> B, int precision = -1)
    {
        AreIdenticalSets<float, float>(A, B, (a, b) => FloatCompare(a, b, precision));
    }

}
