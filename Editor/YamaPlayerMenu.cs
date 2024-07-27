using UnityEditor;
using UnityEngine;

public static class YamaPlayerMenu
{
    const string menuPrefix = "GameObject/YamaPlayer/";
    static string yamaplayerPrefabGuid = "0ca92d0fbf2bf3944bfeef01f4977da5";
    static string subScreenPrefabGuid = "1d1c026d8b023d04ea81f85594f05aec";
    static string controllerPrefabGuid = "ddf7f58d0d20d6843a79711f81f34bf2";
    static string playlistPanelPrefabGuid = "32aa1985af9229540a44cf22406ee1a2";

    [MenuItem(menuPrefix + "Main")]
    public static void CreateYamaPlayer()
    {
        string path = AssetDatabase.GUIDToAssetPath(yamaplayerPrefabGuid);
        CreateGameObject(path);
    }

    [MenuItem(menuPrefix + "SubScreen")]
    public static void CreateSubScreen()
    {
        string path = AssetDatabase.GUIDToAssetPath(subScreenPrefabGuid);
        CreateGameObject(path);
    }

    [MenuItem(menuPrefix + "Controller Bar")]
    public static void CreateController()
    {
        string path = AssetDatabase.GUIDToAssetPath(controllerPrefabGuid);
        CreateGameObject(path);
    }

    [MenuItem(menuPrefix + "Playlist Panel")]
    public static void CreatePlaylistPanel()
    {
        string path = AssetDatabase.GUIDToAssetPath(playlistPanelPrefabGuid);
        CreateGameObject(path);
    }

    static void CreateGameObject(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        Transform parent = Selection.activeTransform;
        GameObject obj = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
        if (obj == null) return;

        obj.name = GameObjectUtility.GetUniqueNameForSibling(parent, prefab.name);
        Selection.activeGameObject = obj;
    }
}
