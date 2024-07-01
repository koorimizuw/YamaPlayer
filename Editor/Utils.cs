
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Script
{
    public class Utils
    {
        public static T[] FindComponentsInHierarthy<T>() where T : UnityEngine.Object
        {
            T[] res = Resources.FindObjectsOfTypeAll<T>();
            return res.Where(i => AssetDatabase.GetAssetOrScenePath(i).Contains(".unity")).ToArray();
        }

        public static UnityEngine.Object[] FindComponentsInHierarthy(Type t)
        {
            UnityEngine.Object[] res = Resources.FindObjectsOfTypeAll(t);
            return res.Where(i => AssetDatabase.GetAssetOrScenePath(i).Contains(".unity")).ToArray();
        }

        public static Type FindType(string typeName, bool useFullName = false, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            StringComparison e = (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (var assemb in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var t in assemb.GetTypes())
                {
                    if (string.Equals(t.FullName, typeName, e)) return t;
                    if (!useFullName && string.Equals(t.Name, typeName, e)) return t;
                }
            return null;
        }
    }
}
