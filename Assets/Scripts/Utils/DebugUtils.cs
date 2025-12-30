using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public static class DebugUtils
{
    public static void DebugList<T>(List<T> list)
    {
        string array = "{";
        for(int i = 0; i < list.Count; i++)
        {
            array += $"{list[i]}";
            if (i < list.Count - 1)
                array += ", ";
        }
        array += "}";

        Debug.Log(array);
    }
}
