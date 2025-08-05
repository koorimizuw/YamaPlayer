using System;
using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Yamadev.YamaStream.Editor
{
    public static class UdonUtility
    {
        public static UdonSharpBehaviour AddUdonSharpComponent(this GameObject gameObject, Type type, Networking.SyncType syncType)
        {
            var proxyBehaviour = gameObject.AddUdonSharpComponent(type);

            var backingBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(proxyBehaviour);
            if (backingBehaviour != null)
            {
                backingBehaviour.SyncMethod = syncType;
            }

            if (Application.isPlaying)
            {
                UdonManager.Instance.RegisterUdonBehaviour(backingBehaviour);
            }

            return proxyBehaviour;
        }

        public static T AddUdonSharpComponent<T>(this GameObject gameObject, Networking.SyncType syncType) where T : UdonSharpBehaviour
        {
            return gameObject.AddUdonSharpComponent(typeof(T), syncType) as T;
        }

        public static Type[] FindAllUdonSharpTypes()
        {
            var results = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsInterface && !type.IsAbstract && type.IsSubclassOf(typeof(UdonSharpBehaviour)))
                    {
                        results.Add(type);
                    }
                }
            }
            return results.ToArray();
        }

        public static Networking.SyncType GetSyncType(this Type type)
        {
            var attributes = type.GetCustomAttributes(typeof(UdonBehaviourSyncModeAttribute), false);

            if (attributes.Length == 0)
            {
                return Networking.SyncType.None;
            }

            switch (((UdonBehaviourSyncModeAttribute)attributes.FirstOrDefault()).behaviourSyncMode)
            {
                case BehaviourSyncMode.None:
                    return Networking.SyncType.None;
                case BehaviourSyncMode.Manual:
                    return Networking.SyncType.Manual;
                case BehaviourSyncMode.Continuous:
                    return Networking.SyncType.Continuous;
                default:
                    return Networking.SyncType.None;
            }
        }
    }
}