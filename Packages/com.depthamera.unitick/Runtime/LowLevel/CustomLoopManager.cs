using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace UniTick.LowLevel
{
    public static class CustomLoopManager
    {
        public struct FixedTickDispatcher { }


#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ResetLoop();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            PlayerLoopSystem currentLoop = PlayerLoop.GetCurrentPlayerLoop();

            RemoveSystem(ref currentLoop, typeof(FixedTickDispatcher));

            PlayerLoopSystem customSystem = new()
            {
                type = typeof(FixedTickDispatcher),
                updateDelegate = OnCustomUpdate
            };

            if (InsertSystem(ref currentLoop, typeof(Update), customSystem))
            {
                PlayerLoop.SetPlayerLoop(currentLoop);
            }
        }

        private static void OnCustomUpdate()
        {
            if (!Application.isPlaying) return;

        }

        private static void ResetLoop()
        {
            PlayerLoopSystem currentLoop = PlayerLoop.GetCurrentPlayerLoop();
            if (RemoveSystem(ref currentLoop, typeof(FixedTickDispatcher)))
            {
                PlayerLoop.SetPlayerLoop(currentLoop);
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void InitEditorCleanup()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    ResetLoop();
                }
            };
        }
#endif


        public static bool RemoveSystem(ref PlayerLoopSystem root, Type targetMarkerType)
        {
            if (root.subSystemList == null || root.subSystemList.Length == 0)
                return false;

            var list = new List<PlayerLoopSystem>(root.subSystemList);
            bool removed = false;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].type == targetMarkerType)
                {
                    list.RemoveAt(i);
                    removed = true;
                    break;
                }
                else
                {
                    var child = list[i];
                    if (RemoveSystem(ref child, targetMarkerType))
                    {
                        list[i] = child;
                        removed = true;
                    }
                }
            }

            if (removed)
            {
                root.subSystemList = list.ToArray();
            }

            return removed;
        }

        private static bool InsertSystem(ref PlayerLoopSystem root, Type targetType, PlayerLoopSystem systemToInsert)
        {
            if (root.type == targetType)
            {
                List<PlayerLoopSystem> subSystems = root.subSystemList != null
                    ? new List<PlayerLoopSystem>(root.subSystemList)
                    : new List<PlayerLoopSystem>();

                subSystems.Add(systemToInsert);
                root.subSystemList = subSystems.ToArray();
                return true;
            }

            if (root.subSystemList == null) return false;

            for (int i = 0; i < root.subSystemList.Length; i++)
            {
                if (InsertSystem(ref root.subSystemList[i], targetType, systemToInsert))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
