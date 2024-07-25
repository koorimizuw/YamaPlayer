
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Script
{
    public static class Utils
    {
        static string _packageInfoGuid = "b4c53d030728ff34098bd0ed5fc21c72";

        [Serializable]
        private struct PackageInfo
        {
            public string version;
        }

        public static string GetYamaPlayerVersion() =>
            JsonUtility.FromJson<PackageInfo>(
                System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath),
                AssetDatabase.GUIDToAssetPath(_packageInfoGuid)))
                ).version ?? string.Empty;

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

        public static void AddScreenProperty(this Controller controller, ScreenType screenType, UnityEngine.Object screen, string textureProperty, string avProProperty)
        {
            if (controller == null) return;
            SerializedObject serializedObject = new SerializedObject(controller);
            SerializedProperty screenTypes = serializedObject.FindProperty("_screenTypes");
            SerializedProperty screens = serializedObject.FindProperty("_screens");
            SerializedProperty textureProperties = serializedObject.FindProperty("_textureProperties");
            SerializedProperty avProProperties = serializedObject.FindProperty("_avProProperties");
            for (int i = 0; i < screens.arraySize; i++)
            {
                if (screens.GetArrayElementAtIndex(i).objectReferenceValue == screen) return;
            }
            screenTypes.arraySize += 1;
            screens.arraySize += 1;
            textureProperties.arraySize += 1;
            avProProperties.arraySize += 1;
            screenTypes.GetArrayElementAtIndex(screenTypes.arraySize - 1).intValue = (int)screenType;
            screens.GetArrayElementAtIndex(screenTypes.arraySize - 1).objectReferenceValue = screen;
            textureProperties.GetArrayElementAtIndex(screenTypes.arraySize - 1).stringValue = textureProperty;
            avProProperties.GetArrayElementAtIndex(screenTypes.arraySize - 1).stringValue = avProProperty;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
