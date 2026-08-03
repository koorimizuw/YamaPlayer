using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Yamadev.YamaStream.Editor;

#if LTCGI_INCLUDED
using pi.LTCGI;
#endif

namespace Yamadev.YamaStream.Modules.LTCGIAdaptor.Editor
{
  [CustomEditor(typeof(LTCGIAdaptor))]
  public class LTCGIAdaptorEditor : EditorBase
  {
#if LTCGI_INCLUDED
    private List<ScreenInfo> _targetScreens = new List<ScreenInfo>();

    private static readonly Color ActiveColor = new Color(0.4f, 0.8f, 0.4f);
    private static Color InactiveColor => EditorGUIUtility.isProSkin ? new Color(0.55f, 0.55f, 0.55f) : new Color(0.45f, 0.45f, 0.45f);

    private class ScreenInfo
    {
      public YamaPlayerScreen Screen;
      public Renderer Renderer;
      public LTCGI_Screen LtcgiScreen;
      public bool IsEnabled => LtcgiScreen != null && LtcgiScreen.enabled;
    }

    private void OnEnable()
    {
      Title = EditorLocalization.Get("module.ltcgiAdaptor.title");
      ShowHeader = false;
      RefreshScreenList();
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      Title = EditorLocalization.Get("module.ltcgiAdaptor.title");

      DrawScreenListSection();

      EditorGUILayout.Space(SpaceMedium);
      EditorGUILayout.HelpBox(EditorLocalization.Get("module.ltcgiAdaptor.hint"), MessageType.Info);
    }

    private void RefreshScreenList()
    {
      _targetScreens.Clear();

      var adaptor = target as LTCGIAdaptor;
      if (adaptor == null) return;

      var controller = adaptor.GetComponentInParent<Controller>();
      if (controller == null) return;

      var allScreens = Object.FindObjectsByType<YamaPlayerScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
      foreach (var screen in allScreens)
      {
        if (screen.controller != controller) continue;
        if (screen.Type != ScreenType.Renderer) continue;

        var renderer = screen.Reference as Renderer;
        if (renderer == null) continue;

        var ltcgiScreen = screen.GetComponent<LTCGI_Screen>();

        _targetScreens.Add(new ScreenInfo
        {
          Screen = screen,
          Renderer = renderer,
          LtcgiScreen = ltcgiScreen
        });
      }

      _targetScreens = _targetScreens.OrderBy(s => s.Screen.gameObject.name).ToList();
    }

    private void DrawScreenListSection()
    {
      using (new EditorGUILayout.HorizontalScope())
      {
        EditorGUILayout.LabelField(
          $"{EditorLocalization.Get("module.ltcgiAdaptor.screenList")} ({_targetScreens.Count})",
          EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(EditorLocalization.Get("module.ltcgiAdaptor.refresh"), GUILayout.Width(80)))
        {
          RefreshScreenList();
        }
      }

      EditorGUILayout.Space(SpaceSmall);

      if (_targetScreens.Count == 0)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("module.ltcgiAdaptor.noScreens"), MessageType.Info);
      }
      else
      {
        DrawScreenListBox();
      }
    }

    private void DrawScreenListBox()
    {
      EditorGUILayout.BeginVertical();

      for (int i = 0; i < _targetScreens.Count; i++)
      {
        DrawScreenRow(i);
      }

      EditorGUILayout.EndVertical();
    }

    private void DrawScreenRow(int index)
    {
      var info = _targetScreens[index];

      var rowBgColor = index % 2 == 0
        ? (EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f))
        : (EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.8f, 0.8f, 0.8f));

      var rowRect = EditorGUILayout.BeginVertical();
      EditorGUI.DrawRect(rowRect, rowBgColor);

      var statusBarRect = new Rect(rowRect.x, rowRect.y, 3, rowRect.height);
      EditorGUI.DrawRect(statusBarRect, info.IsEnabled ? ActiveColor : InactiveColor);

      var rowHeight = 26f;
      using (new EditorGUILayout.HorizontalScope(GUILayout.Height(rowHeight)))
      {
        GUILayout.Space(10);

        var iconStyle = new GUIStyle(GUIStyle.none) { alignment = TextAnchor.MiddleCenter };
        var iconContent = EditorGUIUtility.IconContent("d_MeshRenderer Icon");
        if (GUILayout.Button(iconContent, iconStyle, GUILayout.Width(20), GUILayout.Height(rowHeight)))
        {
          EditorGUIUtility.PingObject(info.Screen.gameObject);
        }
        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

        var labelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
        if (GUILayout.Button(info.Screen.gameObject.name, labelStyle, GUILayout.MinWidth(100), GUILayout.Height(rowHeight)))
        {
          EditorGUIUtility.PingObject(info.Screen.gameObject);
        }
        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

        GUILayout.FlexibleSpace();

        if (info.IsEnabled)
        {
          if (GUILayout.Button(EditorLocalization.Get("module.ltcgiAdaptor.settings"), EditorStyles.miniButton, GUILayout.Width(50)))
          {
            Selection.activeGameObject = info.Screen.gameObject;
            EditorGUIUtility.PingObject(info.Screen.gameObject);
          }

          if (GUILayout.Button(EditorLocalization.Get("module.ltcgiAdaptor.disable"), EditorStyles.miniButton, GUILayout.Width(50)))
          {
            DisableLtcgiScreen(info);
          }
        }
        else
        {
          if (GUILayout.Button(EditorLocalization.Get("module.ltcgiAdaptor.enable"), EditorStyles.miniButton, GUILayout.Width(50)))
          {
            EnableLtcgiScreen(info);
          }
        }

        GUILayout.Space(8);
      }

      EditorGUILayout.EndVertical();
    }

    private const string LTCGI_CONTROLLER_PREFAB_GUID = "4b1aac09caa0ea54ba902102643bb545";
    private const string LTCGI_DEFAULT_TEXTURE_GUID = "68718da77206620438ca14e29cefa6fb";

    private void EnableLtcgiScreen(ScreenInfo info)
    {
      if (LTCGI_Controller.Singleton == null)
      {
        var prefabPath = AssetDatabase.GUIDToAssetPath(LTCGI_CONTROLLER_PREFAB_GUID);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
          var controllerObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
          Undo.RegisterCreatedObjectUndo(controllerObj, "Create LTCGI Controller");
        }
      }

      if (LTCGI_Controller.Singleton != null)
      {
        var defaultTexturePath = AssetDatabase.GUIDToAssetPath(LTCGI_DEFAULT_TEXTURE_GUID);
        var defaultTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(defaultTexturePath);
        if (defaultTexture != null)
        {
          LTCGI_Controller.Singleton.VideoTexture = defaultTexture;
        }
      }

      Undo.RecordObject(info.Screen.gameObject, "Enable LTCGI Screen");

      if (info.LtcgiScreen == null)
      {
        info.LtcgiScreen = Undo.AddComponent<LTCGI_Screen>(info.Screen.gameObject);
        info.LtcgiScreen.ColorMode = ColorMode.Texture;
      }
      else
      {
        info.LtcgiScreen.enabled = true;
      }

      if (LTCGI_Controller.Singleton != null)
      {
        LTCGI_Controller.Singleton.UpdateMaterials();
      }

      EditorUtility.SetDirty(info.Screen.gameObject);
      RefreshScreenList();
    }

    private void DisableLtcgiScreen(ScreenInfo info)
    {
      if (info.LtcgiScreen == null) return;

      Undo.RecordObject(info.LtcgiScreen, "Disable LTCGI Screen");
      info.LtcgiScreen.enabled = false;

      if (LTCGI_Controller.Singleton != null)
      {
        LTCGI_Controller.Singleton.UpdateMaterials();
      }

      EditorUtility.SetDirty(info.LtcgiScreen);
      RefreshScreenList();
    }
#else
    private void OnEnable()
    {
      Title = EditorLocalization.Get("module.ltcgiAdaptor.title");
      ShowHeader = false;
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();
      Title = EditorLocalization.Get("module.ltcgiAdaptor.title");
      EditorGUILayout.HelpBox(EditorLocalization.Get("module.ltcgiAdaptor.notInstalled"), MessageType.Warning);
    }
#endif
  }
}
