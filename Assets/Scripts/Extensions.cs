using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extensions
{
    public static string Join(this IEnumerable<string> strings, string separator = "")
    {
        return strings.Aggregate((a, b) => a + separator + b);
    }

    public static void SetPositionX(this Transform t, float x)
    {
        t.localPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);
    }
    public static void SetPositionY(this Transform t, float y)
    {
        t.localPosition = new Vector3(t.localPosition.x, y, t.localPosition.z);
    }
    public static void SetPositionZ(this Transform t, float z)
    {
        t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, z);
    }
    public static T PickRandom<T>(this T[] arr)
    {
        return arr[Random.Range(0, arr.Length)];
    }
}
