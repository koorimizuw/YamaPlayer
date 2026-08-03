using System;
using System.Linq;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UdonSharpEditor;

namespace Yamadev.YamaStream.Editor
{
  public abstract class EditorBase : UnityEditor.Editor
  {
    private const string LogoGuid = "45177375d4933bc469e82e59e57ce065";
    private const float LogoHeight = 60f;
    private const float LogoMarginTop = 12f;

    protected const float SpaceLarge = 16f;
    protected const float SpaceMedium = 8f;
    protected const float SpaceSmall = 4f;

    private static GUIStyle _titleStyle;
    private static GUIStyle _languageCodeStyle;
    private static GUIStyle _displayNameStyle;

    protected string Title { get; set; }
    protected bool ShowHeader { get; set; } = true;

    protected static GUIStyle LanguageCodeStyle => _languageCodeStyle;
    protected static GUIStyle DisplayNameStyle => _displayNameStyle;

    public override void OnInspectorGUI()
    {
      InitStyles();

      if (ShowHeader)
      {
        DrawLogoAndVersion();
        EditorGUILayout.Space(SpaceMedium);
        DrawLanguageSelector();
        EditorGUILayout.Space(SpaceMedium);
        DrawTitle();
        EditorGUILayout.Space(SpaceLarge);
      }

      if (target is UdonSharpBehaviour)
      {
        UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target);
      }
    }

    protected void DrawLogoAndVersion()
    {
      var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(LogoGuid));
      if (logo == null) return;

      var logoRect = new Rect
      {
        height = LogoHeight,
        y = LogoMarginTop
      };
      logoRect.width = logoRect.height * logo.width / logo.height;
      logoRect.x = (EditorGUIUtility.currentViewWidth - logoRect.width) / 2f;

      GUI.DrawTexture(logoRect, logo);

      var versionContent = new GUIContent($"v{PackageManager.PackageInfo.version}");
      var versionSize = Styles.Bold.CalcSize(versionContent);
      var versionRect = new Rect(logoRect.xMax, logoRect.yMax - versionSize.y, versionSize.x, versionSize.y);
      GUI.Label(versionRect, versionContent, Styles.Bold);

      EditorGUILayout.Space(LogoMarginTop + logoRect.height);
    }

    protected void DrawLanguageSelector()
    {
      var availableLanguages = EditorLocalization.AvailableLanguages;
      if (availableLanguages == null || availableLanguages.Length == 0) return;

      var languageNames = availableLanguages.Select(EditorLocalization.GetLanguageName).ToArray();
      var currentIndex = Array.IndexOf(availableLanguages, EditorLocalization.CurrentLanguage);
      if (currentIndex < 0) currentIndex = 0;

      using (new EditorGUILayout.HorizontalScope())
      {
        GUILayout.FlexibleSpace();
        var selectedIndex = EditorGUILayout.Popup(currentIndex, languageNames, GUILayout.Width(200));
        if (selectedIndex >= 0 && selectedIndex < availableLanguages.Length)
        {
          EditorLocalization.CurrentLanguage = availableLanguages[selectedIndex];
        }
        GUILayout.FlexibleSpace();
      }
    }

    protected void DrawTitle()
    {
      if (string.IsNullOrEmpty(Title)) return;

      using (new EditorGUILayout.HorizontalScope())
      {
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(Title, _titleStyle);
        GUILayout.FlexibleSpace();
      }
    }

    protected static void InitStyles()
    {
      _titleStyle ??= new GUIStyle(EditorStyles.boldLabel)
      {
        fontSize = 14,
        alignment = TextAnchor.MiddleCenter
      };

      _languageCodeStyle ??= new GUIStyle(EditorStyles.boldLabel)
      {
        fontSize = 12,
        fontStyle = FontStyle.Bold
      };

      _displayNameStyle ??= new GUIStyle(EditorStyles.label)
      {
        fontSize = 11,
        fontStyle = FontStyle.Italic
      };
    }
  }
}