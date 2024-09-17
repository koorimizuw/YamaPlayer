using UnityEditor;
using UnityEngine;

public static class YamaPlayerMenu
{
    const string menuPrefix = "GameObject/YamaPlayer/";
    static string _yamaplayerPrefabGuid = "0ca92d0fbf2bf3944bfeef01f4977da5";
    static string _yamaplayerNoScreenPrefabGuid = "84f33e64f5807174288ea98464197fd1";
    static string _subScreenPrefabGuid = "1d1c026d8b023d04ea81f85594f05aec";
    static string _controllerBarPrefabGuid = "ddf7f58d0d20d6843a79711f81f34bf2";
    static string _playlistPanelPrefabGuid = "32aa1985af9229540a44cf22406ee1a2";

    [MenuItem(menuPrefix + "Main (With screen and UI)", priority = 1)]
    public static void CreateYamaPlayer() =>
        CreateGameObject(AssetDatabase.GUIDToAssetPath(_yamaplayerPrefabGuid));

    [MenuItem(menuPrefix + "Main (No screen)", priority = 2)]
    public static void CreateYamaPlayerNoScreen() => 
        CreateGameObject(AssetDatabase.GUIDToAssetPath(_yamaplayerNoScreenPrefabGuid));

    [MenuItem(menuPrefix + "SubScreen", priority = 101)]
    public static void CreateSubScreen() =>
        CreateGameObject(AssetDatabase.GUIDToAssetPath(_subScreenPrefabGuid));

    [MenuItem(menuPrefix + "Controller Bar", priority = 102)]
    public static void CreateControllerBar() =>
        CreateGameObject(AssetDatabase.GUIDToAssetPath(_controllerBarPrefabGuid));

    [MenuItem(menuPrefix + "Playlist Panel", priority = 103)]
    public static void CreatePlaylistPanel() =>
        CreateGameObject(AssetDatabase.GUIDToAssetPath(_playlistPanelPrefabGuid));

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
