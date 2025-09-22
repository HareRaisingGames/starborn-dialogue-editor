using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StringUtils
{
    public static bool CompareLists<T>(List<T> a, List<T> b)
    {
        if (a.Count != b.Count)
            return false;

        return a.SequenceEqual(b);
    }

    public static List<T> CopyList<T>(List<T> copy)
    {
        List<T> paste = new List<T>();
        foreach(T c in copy)
        {
            paste.Add(c);
        }
        return paste;
    }
}
