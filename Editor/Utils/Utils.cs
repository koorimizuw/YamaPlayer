
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
    internal static class Utils
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

        public static void AddScreenProperty(this Controller controller, ScreenType screenType, UnityEngine.Object screen, string textureProperty = "_MainTex")
        {
            if (controller == null) return;
            SerializedObject serializedObject = new SerializedObject(controller);
            SerializedProperty screenTypes = serializedObject.FindProperty("_screenTypes");
            SerializedProperty screens = serializedObject.FindProperty("_screens");
            SerializedProperty textureProperties = serializedObject.FindProperty("_textureProperties");
            for (int i = 0; i < screens.arraySize; i++)
            {
                if (screens.GetArrayElementAtIndex(i).objectReferenceValue == screen) return;
            }
            screenTypes.arraySize += 1;
            screens.arraySize += 1;
            textureProperties.arraySize += 1;
            screenTypes.GetArrayElementAtIndex(screenTypes.arraySize - 1).intValue = (int)screenType;
            screens.GetArrayElementAtIndex(screenTypes.arraySize - 1).objectReferenceValue = screen;
            textureProperties.GetArrayElementAtIndex(screenTypes.arraySize - 1).stringValue = textureProperty;
            serializedObject.ApplyModifiedProperties();
        }

        public static void RemoveScreenProperty(this Controller controller, UnityEngine.Object screen)
        {
            if (controller == null) return;
            SerializedObject serializedObject = new SerializedObject(controller);
            SerializedProperty screenTypes = serializedObject.FindProperty("_screenTypes");
            SerializedProperty screens = serializedObject.FindProperty("_screens");
            SerializedProperty textureProperties = serializedObject.FindProperty("_textureProperties");
            for (int i = 0; i < screens.arraySize; i++)
            {
                if (screens.GetArrayElementAtIndex(i).objectReferenceValue != screen) continue;
                screenTypes.DeleteArrayElementAtIndex(i);
                screens.DeleteArrayElementAtIndex(i);
                textureProperties.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                break;
            }
        }
    }
}
