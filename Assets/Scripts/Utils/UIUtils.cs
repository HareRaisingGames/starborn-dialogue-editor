using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public static class UIUtils
{
    public static bool inFieldFocus(TMP_InputField[] fields)
    {
        foreach(TMP_InputField field in fields)
        {
            if (field.isFocused) return true;
        }
        return false;
    }

    public static bool containsEvents(UnityEvent eventHandler)
    {
        return eventHandler.GetPersistentEventCount() != 0;
    }

    public static List<GameObject> FindInactiveGameObjectsWithTag(string tag)
    {
        List<GameObject> foundObjects = new List<GameObject>();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];

        foreach (GameObject go in allObjects)
        {
            // Check if the object is part of the scene and not a prefab asset
            if (go.hideFlags == HideFlags.None && go.tag == tag && !go.activeInHierarchy)
            {
                foundObjects.Add(go);
            }
        }
        return foundObjects;
    }

    public static GameObject FindInactiveGameObjectWithTag(string tag)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];

        foreach (GameObject go in allObjects)
        {
            // Check if the object is part of the scene and not a prefab asset
            if (go.hideFlags == HideFlags.None && go.tag == tag && !go.activeInHierarchy)
            {
                return go;
            }
        }
        return null;
    }

    public static int GetDropdownValueByName(TMP_Dropdown dropdown, string name)
    {
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (dropdown.options[i].text == name)
            {
                return i;
            }
        }
        return -1;
    }
}
