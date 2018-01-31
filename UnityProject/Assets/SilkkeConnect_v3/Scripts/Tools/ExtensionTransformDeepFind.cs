// Author: Christopher George Steel

using UnityEngine;
using System.Collections;

// Depth-first search
public static class ExtensionTransformDeepFind
{
    public static Transform DeepFind(this Transform parent, string name)
    {
        Transform res = parent.Find(name);

        if (res != null)
        {
            return res;
        }
        foreach (Transform child in parent)
        {
            res = child.DeepFind(name);
            if (res != null)
            {
                return res;
            }
        }
        return null;
    }
}