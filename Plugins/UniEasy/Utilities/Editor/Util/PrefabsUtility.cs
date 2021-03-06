﻿using CustomMenu = UnityEngine.ContextMenu;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace UniEasy.Editor
{
    public static class PrefabsUtility
    {
        private static int SelectionThresholdForProgressBar = 20;

        [MenuItem("GameObject/Apply Changes To Selected Prefabs", false, 101)]
        [CustomMenu("GameObject/UniEasy/Apply Changes To Selected Prefabs", false, 101)]
        private static void ApplyPrefabs()
        {
            SearchPrefabConnections(ApplyToSelectedPrefabs);
        }

        [MenuItem("GameObject/Revert Changes Of Selected Prefabs", false, 102)]
        [CustomMenu("GameObject/UniEasy/Revert Changes Of Selected Prefabs", false, 102)]
        private static void ResetPrefabs()
        {
            SearchPrefabConnections(RevertToSelectedPrefabs);
        }

        [MenuItem("GameObject/Apply Changes To Selected Prefabs", true)]
        [MenuItem("GameObject/Revert Changes Of Selected Prefabs", true)]
        [CustomMenu("GameObject/UniEasy/Apply Changes To Selected Prefabs", true, 101)]
        [CustomMenu("GameObject/UniEasy/Revert Changes Of Selected Prefabs", true, 102)]
        private static bool IsSceneObjectSelected()
        {
            return Selection.activeTransform != null;
        }

        //Look for connections
        private static void SearchPrefabConnections(Action<GameObject> changePrefabAction)
        {
            GameObject[] selectedTransforms = Selection.gameObjects;
            int numberOfTransforms = selectedTransforms.Length;
            bool showProgressBar = numberOfTransforms >= SelectionThresholdForProgressBar;
            int changedObjectsCount = 0;
            //Iterate through all the selected gameobjects
            try
            {
                for (int i = 0; i < numberOfTransforms; i++)
                {
                    if (showProgressBar)
                    {
                        EditorUtility.DisplayProgressBar("Update prefabs", "Updating prefabs (" + i + "/" + numberOfTransforms + ")",
                            (float)i / (float)numberOfTransforms);
                    }

                    var go = selectedTransforms[i];
#if UNITY_2018_3_OR_NEWER
                    var status = PrefabUtility.GetPrefabInstanceStatus(go);
                    //Is the selected gameobject a prefab?
                    if (PrefabUtility.IsPartOfPrefabInstance(go) || status == PrefabInstanceStatus.Disconnected)
                    {
                        var prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
#else
                    var prefabType = PrefabUtility.GetPrefabType(go);
                    //Is the selected gameobject a prefab?
                    if (prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance)
                    {
                        var prefabRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);
#endif
                        if (prefabRoot == null)
                        {
                            continue;
                        }

                        changePrefabAction(prefabRoot);
                        changedObjectsCount++;
                    }
                }
            }
            finally
            {
                if (showProgressBar)
                {
                    EditorUtility.ClearProgressBar();
                }
                Debug.LogFormat("{0} Prefab(s) updated", changedObjectsCount);
            }
        }

        private static void ApplyToSelectedPrefabs(GameObject go)
        {
            var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (prefabAsset == null)
            {
                return;
            }
#if UNITY_2018_3_OR_NEWER
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, AssetDatabase.GetAssetPath(prefabAsset), InteractionMode.AutomatedAction);
#else
            PrefabUtility.ReplacePrefab(go, prefabAsset, ReplacePrefabOptions.ConnectToPrefab);
#endif
        }

        private static void RevertToSelectedPrefabs(GameObject go)
        {
#if UNITY_2018_3_OR_NEWER
            var prefabAsset = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, AssetDatabase.GetAssetPath(prefabAsset), InteractionMode.AutomatedAction);
            PrefabUtility.RevertPrefabInstance(go, InteractionMode.AutomatedAction);
#else
            PrefabUtility.ReconnectToLastPrefab(go);
            PrefabUtility.RevertPrefabInstance(go);
#endif
        }

        public static void ReplacingGameObjectsWithPrefab(GameObject prefab, params GameObject[] goes)
        {
            var selectedObjects = new List<GameObject>();

            foreach (var go in goes)
            {
#if UNITY_2018_3_OR_NEWER
                if (PrefabUtility.IsPartOfPrefabAsset(go))
#else
                if (PrefabUtility.GetPrefabType(go) == PrefabType.Prefab)
#endif
                {
                    selectedObjects.Add(go);
                    continue;
                }

                var name = go.name;
                var pos = go.transform.position;
                var rot = go.transform.rotation;
                var sca = go.transform.localScale;

#if UNITY_2018_3_OR_NEWER
                var newGo = PrefabUtility.SaveAsPrefabAssetAndConnect(go, AssetDatabase.GetAssetPath(prefab), InteractionMode.AutomatedAction);
#else
                var newGo = PrefabUtility.ConnectGameObjectToPrefab(go, prefab as GameObject);
#endif

                newGo.name = name;
                newGo.transform.position = pos;
                newGo.transform.rotation = rot;
                newGo.transform.localScale = sca;

                selectedObjects.Add(newGo);
            }

            Selection.objects = selectedObjects.ToArray();
        }
    }
}
