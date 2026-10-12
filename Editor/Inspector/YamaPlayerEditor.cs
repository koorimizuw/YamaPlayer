using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Video.Components.AVPro;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

using Object = UnityEngine.Object;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Editor
{
  [CustomEditor(typeof(YamaPlayer))]
  public class YamaPlayerEditor : EditorBase
  {
    private YamaPlayer _target;

    private Controller _controller;
    private SerializedObject _controllerSerializedObject;

    private SerializedProperty _localMode;
    private SerializedProperty _volume;
    private SerializedProperty _mirrorFlip;
    private SerializedProperty _brightness;
    private SerializedProperty _mute;
    private SerializedProperty _loop;
    private SerializedProperty _shuffle;
    private SerializedProperty _videoPlayerHandlers;
    // private SerializedProperty _retryAfterSeconds;
    private SerializedProperty _maxErrorRetry;
    private SerializedProperty _useFallbackAfterErrors;
    private SerializedProperty _forwardInterval;
    private SerializedProperty _screenTypes;
    private SerializedProperty _screens;
    private SerializedProperty _textureProperties;
    private SerializedProperty _audioSources;

    private ReorderableList _screenList;
    private ReorderableList _speakerList;

    private VRCAVProVideoPlayer _avPro;
    private SerializedObject _avProSerializedObject;
    private SerializedProperty _useLowLatency;

    private AppearanceSettings _appearanceSettings;
    private SerializedObject _appearanceSerializedObject;
    private SerializedProperty _defaultColorSet;
    private SerializedProperty _colorSets;

    private LocalizationSettings _localizationSettings;
    private SerializedObject _localizationSerializedObject;
    private SerializedProperty _defaultLanguage;
    private SerializedProperty _languages;

    private ModuleManager _moduleManager;

    private UIController _uiController;
    private SerializedObject _uiControllerSerializedObject;
    private SerializedProperty _idleScreenSprite;

    private bool _screenSettingsFoldout = false;
    private bool _speakerSettingsFoldout = false;

    private void OnEnable()
    {
      _target = target as YamaPlayer;

      if (Application.isPlaying) return;

      _controller = _target.GetComponentInChildren<Controller>(true);

      if (_controller != null)
      {
        _controllerSerializedObject = new SerializedObject(_controller);

        _localMode = _controllerSerializedObject.FindProperty("_isLocal");
        _volume = _controllerSerializedObject.FindProperty("_volume");
        _mirrorFlip = _controllerSerializedObject.FindProperty("_mirrorFlip");
        _brightness = _controllerSerializedObject.FindProperty("_brightness");
        _mute = _controllerSerializedObject.FindProperty("_mute");
        _loop = _controllerSerializedObject.FindProperty("_loop");
        _shuffle = _controllerSerializedObject.FindProperty("_shuffle");
        _videoPlayerHandlers = _controllerSerializedObject.FindProperty("_videoPlayerHandlers");
        // _retryAfterSeconds = _controllerSerializedObject.FindProperty("_retryAfterSeconds");
        _maxErrorRetry = _controllerSerializedObject.FindProperty("_maxErrorRetry");
        _useFallbackAfterErrors = _controllerSerializedObject.FindProperty("_useFallbackAfterErrors");
        _forwardInterval = _controllerSerializedObject.FindProperty("_forwardInterval");
        _screenTypes = _controllerSerializedObject.FindProperty("_screenTypes");
        _screens = _controllerSerializedObject.FindProperty("_screens");
        _textureProperties = _controllerSerializedObject.FindProperty("_textureProperties");
        _audioSources = _controllerSerializedObject.FindProperty("_audioSources");

        _screenList = new ReorderableList(_controllerSerializedObject, _screens)
        {
          drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, EditorLocalization.GetLayout("settings.screen.targets"), EditorStyles.boldLabel),
          onAddCallback = OnAddScreen,
          onRemoveCallback = OnRemoveScreen,
          onReorderCallbackWithDetails = OnReorderScreen,
          drawElementCallback = DrawScreenElement,
          elementHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2,
        };

        _speakerList = new ReorderableList(_controllerSerializedObject, _audioSources)
        {
          drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, EditorLocalization.GetLayout("settings.audio.speakers"), EditorStyles.boldLabel),
          onAddCallback = OnAddSpeaker,
          drawElementCallback = (rect, index, isActive, isFocused) =>
          {
            rect.height = EditorGUIUtility.singleLineHeight;
            var element = _audioSources.GetArrayElementAtIndex(index);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
              EditorGUI.PropertyField(rect, element, GUIContent.none);
              if (check.changed)
              {
                HandleSpeakerChange(element);
              }
            }
          },
          elementHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
        };
      }

      _avPro = _target.GetComponentInChildren<VRCAVProVideoPlayer>();
      if (_avPro != null)
      {
        _avProSerializedObject = new SerializedObject(_avPro);
        _useLowLatency = _avProSerializedObject.FindProperty("useLowLatency");
      }

      _appearanceSettings = _target.GetComponentInChildren<AppearanceSettings>(true);
      if (_appearanceSettings != null)
      {
        _appearanceSerializedObject = new SerializedObject(_appearanceSettings);
        _defaultColorSet = _appearanceSerializedObject.FindProperty("defaultColorSet");
        _colorSets = _appearanceSerializedObject.FindProperty("colorSets");
      }

      _localizationSettings = _target.GetComponentInChildren<LocalizationSettings>(true);
      if (_localizationSettings != null)
      {
        _localizationSerializedObject = new SerializedObject(_localizationSettings);
        _defaultLanguage = _localizationSerializedObject.FindProperty("defaultLanguage");
        _languages = _localizationSerializedObject.FindProperty("languages");
      }

      _moduleManager = _target.GetComponentInChildren<ModuleManager>(true);

      _uiController = _target.GetComponentInChildren<UIController>(true);
      if (_uiController != null)
      {
        _uiControllerSerializedObject = new SerializedObject(_uiController);
        _idleScreenSprite = _uiControllerSerializedObject.FindProperty("_idleScreenSprite");
      }
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();
      serializedObject.Update();

      if (Application.isPlaying)
      {
        EditorGUILayout.HelpBox("YamaPlayer is not available in the play mode.", MessageType.Info);
        return;
      }

      DrawAppearanceSettings();
      DrawLocalizationSettings(false);
      DrawUISettings(false);
      EditorGUILayout.Space(SpaceMedium);

      DrawVideoPlayerSettings();
      EditorGUILayout.Space(SpaceMedium);

      DrawPlaylistSettings();
      EditorGUILayout.Space(SpaceMedium);

      DrawModulesSettings();
      EditorGUILayout.Space(SpaceMedium);

      DrawVersionSettings();
      EditorGUILayout.Space(SpaceSmall);

      ApplyModifiedProperties();
    }

    private void DrawAppearanceSettings()
    {
      if (_appearanceSettings == null) return;

      EditorGUILayout.LabelField(EditorLocalization.Get("appearance.title"), EditorStyles.boldLabel);

      using (new EditorGUILayout.HorizontalScope())
      {
        if (_colorSets != null && _colorSets.arraySize > 0)
        {
          var colorSetNames = new string[_colorSets.arraySize];
          int selectedIndex = 0;

          for (int i = 0; i < _colorSets.arraySize; i++)
          {
            var colorSet = _colorSets.GetArrayElementAtIndex(i);
            var nameProperty = colorSet.FindPropertyRelative("colorSetName");
            colorSetNames[i] = nameProperty != null ? nameProperty.stringValue : $"ColorSet {i}";

            if (_defaultColorSet != null && colorSetNames[i] == _defaultColorSet.stringValue)
            {
              selectedIndex = i;
            }
          }

          using (var check = new EditorGUI.ChangeCheckScope())
          {
            int newIndex = EditorGUILayout.Popup(EditorLocalization.Get("appearance.defaultColorSet"), selectedIndex, colorSetNames);
            if (check.changed && _defaultColorSet != null)
            {
              _defaultColorSet.stringValue = colorSetNames[newIndex];
            }
          }
        }
        else
        {
          EditorGUILayout.LabelField(EditorLocalization.Get("appearance.noColorSets"));
        }

        if (GUILayout.Button(EditorLocalization.Get("button.edit"), GUILayout.Width(60)))
        {
          Selection.activeObject = _appearanceSettings;
        }
      }
    }

    private void DrawLocalizationSettings(bool showLabel = true)
    {
      if (_localizationSettings == null) return;

      if (showLabel)
      {
        EditorGUILayout.LabelField(EditorLocalization.Get("localization.title"), EditorStyles.boldLabel);
      }

      using (new EditorGUILayout.HorizontalScope())
      {
        if (_languages != null && _languages.arraySize > 0)
        {
          var optionCodes = new List<string> { "" };
          var optionNames = new List<string> { EditorLocalization.Get("localization.defaultLanguage.auto") };

          for (int i = 0; i < _languages.arraySize; i++)
          {
            var language = _languages.GetArrayElementAtIndex(i);
            var displayNameProperty = language.FindPropertyRelative("displayName");
            var codeProperty = language.FindPropertyRelative("languageCode");
            var code = codeProperty != null ? codeProperty.stringValue : "";
            var displayName = displayNameProperty != null ? displayNameProperty.stringValue : $"Language {i}";
            optionCodes.Add(code);
            optionNames.Add($"{code} - {displayName}");
          }

          int selectedIndex = 0;
          if (!string.IsNullOrEmpty(_defaultLanguage?.stringValue))
          {
            selectedIndex = optionCodes.IndexOf(_defaultLanguage.stringValue);
            if (selectedIndex < 0) selectedIndex = 0;
          }

          using (var check = new EditorGUI.ChangeCheckScope())
          {
            int newIndex = EditorGUILayout.Popup(EditorLocalization.Get("localization.defaultLanguage"), selectedIndex, optionNames.ToArray());
            if (check.changed && _defaultLanguage != null && newIndex >= 0 && newIndex < optionCodes.Count)
            {
              _defaultLanguage.stringValue = optionCodes[newIndex];
            }
          }
        }
        else
        {
          EditorGUILayout.LabelField(EditorLocalization.Get("localization.noLanguages"));
        }

        if (GUILayout.Button(EditorLocalization.Get("button.edit"), GUILayout.Width(60)))
        {
          Selection.activeObject = _localizationSettings;
        }
      }
    }

    private void DrawUISettings(bool showLabel = true)
    {
      if (_uiController == null) return;

      if (showLabel)
      {
        EditorGUILayout.LabelField(EditorLocalization.Get("settings.ui.label"), EditorStyles.boldLabel);
      }

      if (_idleScreenSprite != null)
      {
        EditorGUILayout.PropertyField(_idleScreenSprite, EditorLocalization.GetLayout("settings.ui.idleScreenSprite", "settings.ui.idleScreenSprite.tooltip"));
      }
    }

    private void DrawDefaultPlayerEngineDropdown()
    {
      if (_videoPlayerHandlers == null || _videoPlayerHandlers.arraySize == 0) return;

      var handlers = new List<PlayerHandler>();
      var displayNames = new List<string>();

      for (int i = 0; i < _videoPlayerHandlers.arraySize; i++)
      {
        var handlerProp = _videoPlayerHandlers.GetArrayElementAtIndex(i);
        var handler = handlerProp.objectReferenceValue as PlayerHandler;
        if (handler != null)
        {
          handlers.Add(handler);
          displayNames.Add(handler.Type.GetString());
        }
      }

      if (handlers.Count == 0) return;

      int currentIndex = 0;
      int newIndex = EditorGUILayout.Popup(
        EditorLocalization.GetLayout("settings.videoPlayerType.label", "settings.videoPlayerType.tooltip"),
        currentIndex,
        displayNames.ToArray()
      );

      if (newIndex != currentIndex && newIndex < handlers.Count)
      {
        var selectedHandler = handlers[newIndex];
        handlers.RemoveAt(newIndex);
        handlers.Insert(0, selectedHandler);

        for (int i = 0; i < handlers.Count; i++)
        {
          _videoPlayerHandlers.GetArrayElementAtIndex(i).objectReferenceValue = handlers[i];
        }
      }
    }

    private void DrawVideoPlayerSettings()
    {
      EditorGUILayout.LabelField(EditorLocalization.Get("settings.player.label"), EditorStyles.boldLabel);
      DrawDefaultPlayerEngineDropdown();
      EditorGUILayout.PropertyField(_localMode, EditorLocalization.GetLayout("settings.localMode.label", "settings.localMode.tooltip"));
      EditorGUILayout.PropertyField(_loop, EditorLocalization.GetLayout("settings.playback.loop", "settings.playback.loop.tooltip"));
      EditorGUILayout.PropertyField(_shuffle, EditorLocalization.GetLayout("settings.playlist.shuffle", "settings.playlist.shuffle.tooltip"));
      EditorGUILayout.PropertyField(_mirrorFlip, EditorLocalization.GetLayout("settings.video.mirrorFlip", "settings.video.mirrorFlip.tooltip"));
      EditorGUILayout.PropertyField(_brightness, EditorLocalization.GetLayout("settings.video.brightness", "settings.video.brightness.tooltip"));
      EditorGUILayout.PropertyField(_mute, EditorLocalization.GetLayout("settings.audio.mute", "settings.audio.mute.tooltip"));
      EditorGUILayout.PropertyField(_volume, EditorLocalization.GetLayout("settings.audio.volume", "settings.audio.volume.tooltip"));
      // EditorGUILayout.PropertyField(_retryAfterSeconds, EditorLocalization.GetLayout("settings.playback.retryInterval", "settings.playback.retryInterval.tooltip"));
      EditorGUILayout.PropertyField(_maxErrorRetry, EditorLocalization.GetLayout("settings.playback.maxRetry", "settings.playback.maxRetry.tooltip"));
      EditorGUILayout.PropertyField(_useFallbackAfterErrors, EditorLocalization.GetLayout("settings.playback.fallbackAfterErrors", "settings.playback.fallbackAfterErrors.tooltip"));
      EditorGUILayout.PropertyField(_useLowLatency, EditorLocalization.GetLayout("settings.playback.lowLatency", "settings.playback.lowLatency.tooltip"));
      DrawScreenSettings();
      DrawSpeakerSettings();
    }

    private void DrawScreenSettings()
    {
      if (_screenList == null) return;

      _screenSettingsFoldout = EditorGUILayout.Foldout(_screenSettingsFoldout, EditorLocalization.Get("settings.screen.settingsLabel"), true);

      if (_screenSettingsFoldout)
      {
        _screenList.DoLayoutList();
      }
    }

    private void DrawSpeakerSettings()
    {
      if (_speakerList == null) return;

      _speakerSettingsFoldout = EditorGUILayout.Foldout(_speakerSettingsFoldout, EditorLocalization.Get("settings.audio.speakerSettings"), true);

      if (_speakerSettingsFoldout)
      {
        _speakerList.DoLayoutList();
        EditorGUILayout.HelpBox(EditorLocalization.Get("settings.audio.unityVideoPlayerWarning"), MessageType.Info);
      }
    }

    private void DrawPlaylistSettings()
    {
      EditorGUILayout.LabelField(EditorLocalization.Get("settings.playlist.label"), EditorStyles.boldLabel);
      using (new EditorGUILayout.HorizontalScope())
      {
        EditorGUILayout.PropertyField(_forwardInterval, EditorLocalization.GetLayout("settings.playlist.interval", "settings.playlist.interval.tooltip"));
        EditorGUILayout.LabelField("秒", GUILayout.Width(20));
      }
      if (GUILayout.Button(EditorLocalization.Get("settings.playlist.edit")))
      {
        PlaylistEditorWindow.ShowPlaylistEditorWindow(_target);
      }
    }

    private void DrawModulesSettings()
    {
      if (_moduleManager == null) return;

      EditorGUILayout.LabelField(EditorLocalization.Get("label.modules"), EditorStyles.boldLabel);

      var installedModules = GetInstalledModules();

      if (installedModules.Count == 0)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("module.manager.noInstalledModules"), MessageType.Info);
      }
      else
      {
        DrawInstalledModulesList(installedModules);
      }

      EditorGUILayout.Space(SpaceSmall);

      if (GUILayout.Button(EditorLocalization.Get("module.manager.title")))
      {
        Selection.activeObject = _moduleManager;
      }
    }

    private List<YamaPlayerModuleDefinition> GetInstalledModules()
    {
      var modules = new List<YamaPlayerModuleDefinition>();
      if (_moduleManager == null) return modules;

      foreach (Transform child in _moduleManager.transform)
      {
        var moduleDef = child.GetComponent<YamaPlayerModuleDefinition>();
        if (moduleDef != null)
        {
          modules.Add(moduleDef);
        }
      }
      return modules;
    }

    private void DrawInstalledModulesList(List<YamaPlayerModuleDefinition> modules)
    {
      for (int i = 0; i < modules.Count; i++)
      {
        DrawModuleRow(modules[i], i);
      }
    }

    private void DrawModuleRow(YamaPlayerModuleDefinition module, int index)
    {
      var rowBgColor = index % 2 == 0
        ? (EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f))
        : (EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.8f, 0.8f, 0.8f));

      var rowRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(22));
      EditorGUI.DrawRect(rowRect, rowBgColor);

      bool isActive = module.gameObject.activeSelf;
      var statusColor = isActive ? new Color(0.4f, 0.8f, 0.4f) : new Color(0.55f, 0.55f, 0.55f);
      var statusBarRect = new Rect(rowRect.x, rowRect.y, 3, rowRect.height);
      EditorGUI.DrawRect(statusBarRect, statusColor);

      GUILayout.Space(10);

      string moduleName = GetModuleName(module);
      EditorGUILayout.LabelField(moduleName, GUILayout.ExpandWidth(true));

      if (!module.noNeedSetUp)
      {
        if (GUILayout.Button(EditorLocalization.Get("module.manager.button.settings"), GUILayout.Width(50)))
        {
          if (ActiveEditorTracker.sharedTracker.isLocked)
          {
            EditorUtility.DisplayDialog(
              EditorLocalization.Get("msg.inspectorLocked.title"),
              EditorLocalization.Get("msg.inspectorLocked"),
              EditorLocalization.Get("button.ok"));
          }
          else
          {
            Selection.activeGameObject = module.gameObject;
          }
        }
      }

      string toggleLabel = isActive
        ? EditorLocalization.Get("module.manager.button.disable")
        : EditorLocalization.Get("module.manager.button.enable");
      if (GUILayout.Button(toggleLabel, GUILayout.Width(56)))
      {
        Undo.RecordObject(module.gameObject, isActive ? "Disable Module" : "Enable Module");
        module.gameObject.SetActive(!isActive);
        EditorUtility.SetDirty(module.gameObject);
      }

      GUILayout.Space(4);

      EditorGUILayout.EndHorizontal();
    }

    private string GetModuleName(YamaPlayerModuleDefinition module)
    {
      if (!string.IsNullOrEmpty(module.moduleNameTranslationKey))
      {
        var translated = EditorLocalization.Get(module.moduleNameTranslationKey);
        if (!string.IsNullOrEmpty(translated))
          return translated;
      }
      return module.moduleName;
    }

    private void DrawVersionSettings()
    {
#if USE_VPM_RESOLVER
      EditorGUILayout.LabelField(EditorLocalization.Get("update.label"), EditorStyles.boldLabel);
      PackageManager.CheckBetaVersion = EditorGUILayout.Toggle(EditorLocalization.GetLayout("update.checkBeta", "update.checkBeta.tooltip"), PackageManager.CheckBetaVersion);
      EditorGUILayout.LabelField(EditorLocalization.Get("update.current"), PackageManager.Version);
      EditorGUILayout.LabelField(EditorLocalization.Get("update.newest"), string.IsNullOrEmpty(PackageManager.NewestVersion) ? "-" : PackageManager.NewestVersion);

      EditorGUILayout.Space(SpaceSmall);

      using (new EditorGUILayout.HorizontalScope())
      {
        if (GUILayout.Button(EditorLocalization.Get("update.check")))
        {
          CheckAndPromptUpdate().Forget();
        }

        using (new EditorGUI.DisabledGroupScope(!PackageManager.HasNewVersion))
        {
          if (GUILayout.Button(EditorLocalization.Get("update.doUpdate")))
          {
            PackageManager.UpdatePackage().Forget();
          }
        }
      }
#else
      EditorGUILayout.HelpBox(EditorLocalization.Get("update.onlyVCC"), MessageType.Info);
#endif
    }

#if USE_VPM_RESOLVER
    private async UniTaskVoid CheckAndPromptUpdate()
    {
      bool hasNew = await PackageManager.CheckUpdate();
      if (hasNew)
      {
        if (EditorUtility.DisplayDialog(
          EditorLocalization.Get("update.newVersionFound"),
          string.Format(EditorLocalization.Get("update.newVersionConfirm"), PackageManager.NewestVersion),
          EditorLocalization.Get("button.doUpdate"),
          EditorLocalization.Get("button.cancel")))
        {
          await PackageManager.UpdatePackage();
        }
      }
      else
      {
        EditorUtility.DisplayDialog(
          EditorLocalization.Get("update.noNewVersion"),
          EditorLocalization.Get("update.youUseNewest"),
          "OK");
      }
    }
#endif

    #region Speaker Settings
    private void OnAddSpeaker(ReorderableList list)
    {
      _audioSources.arraySize += 1;
      _audioSources.GetArrayElementAtIndex(_audioSources.arraySize - 1).objectReferenceValue = null;
    }

    private void HandleSpeakerChange(SerializedProperty element)
    {
      _controllerSerializedObject.ApplyModifiedProperties();

      var audioSource = element.objectReferenceValue as AudioSource;
      if (audioSource == null) return;

      if (!audioSource.TryGetComponent<YamaPlayerSpeaker>(out var speaker))
      {
        speaker = audioSource.gameObject.AddComponent<YamaPlayerSpeaker>();
      }
      else if (speaker.controller != null && speaker.controller != _controller)
      {
        if (!EditorUtility.DisplayDialog(
          EditorLocalization.Get("dialog.conflict.title"),
          EditorLocalization.Get("dialog.conflict.speaker"),
          EditorLocalization.Get("button.continue"),
          EditorLocalization.Get("button.cancel")))
        {
          element.objectReferenceValue = null;
          return;
        }
        RemoveAudioSourceFromController(speaker.controller, audioSource);
      }
      speaker.controller = _controller;

      if (_avPro != null && audioSource.TryGetComponent<VRCAVProVideoSpeaker>(out var avProSpeaker))
      {
        var speakerSO = new SerializedObject(avProSpeaker);
        var videoPlayerProp = speakerSO.FindProperty("videoPlayer");
        if (videoPlayerProp != null)
        {
          videoPlayerProp.objectReferenceValue = _avPro;
          speakerSO.ApplyModifiedProperties();
        }
      }

      EditorUtility.SetDirty(audioSource.gameObject);
    }

    private void RemoveAudioSourceFromController(Controller controller, AudioSource audioSource)
    {
      var so = new SerializedObject(controller);
      var audioSourcesProp = so.FindProperty("_audioSources");
      for (int i = audioSourcesProp.arraySize - 1; i >= 0; i--)
      {
        if (audioSourcesProp.GetArrayElementAtIndex(i).objectReferenceValue == audioSource)
        {
          audioSourcesProp.DeleteArrayElementAtIndex(i);
        }
      }
      so.ApplyModifiedProperties();
    }
    #endregion

    #region Screen Settings
    private void OnAddScreen(ReorderableList list)
    {
      _screenTypes.arraySize += 1;
      _screens.arraySize += 1;
      _textureProperties.arraySize += 1;
      _screenTypes.GetArrayElementAtIndex(_screenTypes.arraySize - 1).intValue = (int)ScreenType.Renderer;
      _screens.GetArrayElementAtIndex(_screens.arraySize - 1).objectReferenceValue = null;
      _textureProperties.GetArrayElementAtIndex(_textureProperties.arraySize - 1).stringValue = "_MainTex";
    }

    private void OnRemoveScreen(ReorderableList list)
    {
      _screenTypes.DeleteArrayElementAtIndex(list.index);
      _screens.DeleteArrayElementAtIndex(list.index);
      _textureProperties.DeleteArrayElementAtIndex(list.index);
    }

    private void OnReorderScreen(ReorderableList list, int oldIndex, int newIndex)
    {
      _screenTypes.MoveArrayElement(oldIndex, newIndex);
      _textureProperties.MoveArrayElement(oldIndex, newIndex);
    }

    private void DrawScreenElement(Rect rect, int index, bool isActive, bool isFocused)
    {
      SerializedProperty screenType = _screenTypes.GetArrayElementAtIndex(index);
      SerializedProperty screen = _screens.GetArrayElementAtIndex(index);
      SerializedProperty textureProperty = _textureProperties.GetArrayElementAtIndex(index);

      rect.height = EditorGUIUtility.singleLineHeight;

      using (var check = new EditorGUI.ChangeCheckScope())
      {
        EditorGUI.PropertyField(rect, screen, EditorLocalization.GetLayout("label.screen", "label.screen.tooltip"));
        if (check.changed)
        {
          HandleScreenTypeChange(screen, screenType, textureProperty);
        }
      }

      rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
      EditorGUI.PropertyField(rect, textureProperty, EditorLocalization.GetLayout("settings.screen.mainTextureProperty", "settings.screen.mainTextureProperty.tooltip"));
    }

    private void HandleScreenTypeChange(SerializedProperty screen, SerializedProperty screenType, SerializedProperty textureProperty)
    {
      if (screen.objectReferenceValue is Material)
      {
        screenType.intValue = (int)ScreenType.Material;
      }
      else if (screen.objectReferenceValue is CustomRenderTexture crt)
      {
        screenType.intValue = (int)ScreenType.Material;
        screen.objectReferenceValue = crt.material;
      }
      else if (screen.objectReferenceValue is GameObject gameObject)
      {
        if (gameObject.TryGetComponent(out Renderer renderer))
        {
          screenType.intValue = (int)ScreenType.Renderer;
          screen.objectReferenceValue = renderer;
        }
        else if (gameObject.TryGetComponent(out RawImage rawImage))
        {
          screenType.intValue = (int)ScreenType.RawImage;
          screen.objectReferenceValue = rawImage;
        }
        else
        {
          screen.objectReferenceValue = null;
        }
      }
      else
      {
        screen.objectReferenceValue = null;
      }

      if (screen.objectReferenceValue != null)
      {
        SetDefaultTextureProperty(screen.objectReferenceValue, textureProperty);

        if (screen.objectReferenceValue is Component component)
        {
          if (!component.TryGetComponent<YamaPlayerScreen>(out var yamaScreen))
          {
            yamaScreen = component.gameObject.AddComponent<YamaPlayerScreen>();
          }
          else if (yamaScreen.controller != null && yamaScreen.controller != _controller)
          {
            if (!EditorUtility.DisplayDialog(
              EditorLocalization.Get("dialog.conflict.title"),
              EditorLocalization.Get("dialog.conflict.screen"),
              EditorLocalization.Get("button.continue"),
              EditorLocalization.Get("button.cancel")))
            {
              screen.objectReferenceValue = null;
              return;
            }
            RemoveScreenFromController(yamaScreen.controller, component);
          }
          yamaScreen.controller = _controller;
          yamaScreen.property = textureProperty.stringValue;
          EditorUtility.SetDirty(component.gameObject);
        }
      }
    }

    private void RemoveScreenFromController(Controller controller, Component screenComponent)
    {
      var so = new SerializedObject(controller);
      var screensProp = so.FindProperty("_screens");
      var screenTypesProp = so.FindProperty("_screenTypes");
      var texturePropertiesProp = so.FindProperty("_textureProperties");
      for (int i = screensProp.arraySize - 1; i >= 0; i--)
      {
        if (screensProp.GetArrayElementAtIndex(i).objectReferenceValue == screenComponent)
        {
          screensProp.DeleteArrayElementAtIndex(i);
          screenTypesProp.DeleteArrayElementAtIndex(i);
          texturePropertiesProp.DeleteArrayElementAtIndex(i);
        }
      }
      so.ApplyModifiedProperties();
    }

    private void SetDefaultTextureProperty(Object target, SerializedProperty textureProperty)
    {
      Shader shader = null;
      if (target is Renderer renderer) shader = renderer.sharedMaterial?.shader;
      else if (target is RawImage rawImage) shader = rawImage.material?.shader;
      else if (target is Material material) shader = material.shader;

      if (shader != null)
      {
        int count = ShaderUtil.GetPropertyCount(shader);
        if (count > 0)
        {
          var textureProperties = Enumerable.Range(0, count)
              .Where(x => ShaderUtil.GetPropertyType(shader, x) == ShaderUtil.ShaderPropertyType.TexEnv)
              .ToArray();

          if (textureProperties.Length > 0)
          {
            textureProperty.stringValue = ShaderUtil.GetPropertyName(shader, textureProperties[0]);
          }
        }
      }
    }
    #endregion

    private void ApplyModifiedProperties()
    {
      serializedObject.ApplyModifiedProperties();
      try
      {
        _controllerSerializedObject?.ApplyModifiedProperties();
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"YamaPlayerEditor: Failed to apply controller properties - {ex.Message}");
      }
      _avProSerializedObject?.ApplyModifiedProperties();
      _appearanceSerializedObject?.ApplyModifiedProperties();
      _localizationSerializedObject?.ApplyModifiedProperties();
      _uiControllerSerializedObject?.ApplyModifiedProperties();
    }

  }
}
