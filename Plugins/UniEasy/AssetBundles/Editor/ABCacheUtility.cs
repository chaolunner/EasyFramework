﻿using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class ABCacheUtility
    {
        [MenuItem("Tools/AssetBundles/Clear Cache")]
        public static void ClearCache()
        {
            Caching.ClearCache();
            Caching.defaultCache.ClearCache();
            Debug.Log("Removes all cached AssetBundles content has Completed!");
        }
    }
}
