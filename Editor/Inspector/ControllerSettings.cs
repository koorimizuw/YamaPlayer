using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

using Object = UnityEngine.Object;

namespace Yamadev.YamaStream.Editor
{
    public class ControllerSettings
    {
        private readonly Controller _controller;
        private readonly SerializedObject _controllerSerializedObject;

        private readonly SerializedProperty _localMode;
        private readonly SerializedProperty _volume;
        private readonly SerializedProperty _mirrorFlip;
        private readonly SerializedProperty _emission;
        private readonly SerializedProperty _mute;
        private readonly SerializedProperty _loop;
        private readonly SerializedProperty _shuffle;
        private readonly SerializedProperty _defaultPlayerEngine;
        private readonly SerializedProperty _forwardInterval;
        private readonly SerializedProperty _screenTypes;
        private readonly SerializedProperty _screens;
        private readonly SerializedProperty _textureProperties;

        private readonly ReorderableList _screenList;

        public bool IsValid => _controller != null && _controllerSerializedObject != null;

        public ControllerSettings(Controller controller)
        {
            _controller = controller;
            if (_controller != null)
            {
                _controllerSerializedObject = new SerializedObject(_controller);

                _localMode = _controllerSerializedObject.FindProperty("_isLocal");
                _volume = _controllerSerializedObject.FindProperty("_volume");
                _mirrorFlip = _controllerSerializedObject.FindProperty("_mirrorFlip");
                _emission = _controllerSerializedObject.FindProperty("_emission");
                _mute = _controllerSerializedObject.FindProperty("_mute");
                _loop = _controllerSerializedObject.FindProperty("_loop");
                _shuffle = _controllerSerializedObject.FindProperty("_shuffle");
                _defaultPlayerEngine = _controllerSerializedObject.FindProperty("_playerType");
                _forwardInterval = _controllerSerializedObject.FindProperty("_forwardInterval");
                _screenTypes = _controllerSerializedObject.FindProperty("_screenTypes");
                _screens = _controllerSerializedObject.FindProperty("_screens");
                _textureProperties = _controllerSerializedObject.FindProperty("_textureProperties");

                _screenList = new ReorderableList(_controllerSerializedObject, _screens)
                {
                    drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, Localization.GetLayout("screenTargets"), EditorStyles.boldLabel),
                    onAddCallback = OnAddScreen,
                    onRemoveCallback = OnRemoveScreen,
                    drawElementCallback = DrawScreenElement,
                    elementHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2,
                };
            }
        }

        public void DrawVideoPlayerSettings()
        {
            EditorGUILayout.PropertyField(_defaultPlayerEngine, Localization.GetLayout("videoPlayerType"));
            EditorGUILayout.LabelField("　", Localization.Get("selectDefaultVideoPlayerType"));
        }

        public void DrawSyncSettings()
        {
            EditorGUILayout.PropertyField(_localMode, Localization.GetLayout("localMode"));
        }

        public void DrawAudioSettings()
        {
            EditorGUILayout.PropertyField(_mute, Localization.GetLayout("mute"));
            EditorGUILayout.PropertyField(_volume, Localization.GetLayout("volume"));
        }

        public void DrawVideoSettings()
        {
            EditorGUILayout.PropertyField(_mirrorFlip, Localization.GetLayout("mirrorInverse"));
            EditorGUILayout.PropertyField(_emission, Localization.GetLayout("brightness"));
        }

        public void DrawPlaybackSettings()
        {
            EditorGUILayout.PropertyField(_loop, Localization.GetLayout("loop"));
        }

        public void DrawShuffleSettings()
        {
            EditorGUILayout.PropertyField(_shuffle, Localization.GetLayout("shuffle"));
        }

        public void DrawPlaylistForwardIntervalSettings()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(_forwardInterval, Localization.GetLayout("forwardInterval"));
                EditorGUILayout.LabelField("秒", GUILayout.Width(20));
            }
        }

        public void DrawScreenSettings()
        {
            _screenList?.DoLayoutList();
        }

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

        private void DrawScreenElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty screenType = _screenTypes.GetArrayElementAtIndex(index);
            SerializedProperty screen = _screens.GetArrayElementAtIndex(index);
            SerializedProperty textureProperty = _textureProperties.GetArrayElementAtIndex(index);

            rect.height = EditorGUIUtility.singleLineHeight;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(rect, screen, Localization.GetLayout("screen"));
                if (check.changed)
                {
                    HandleScreenTypeChange(screen, screenType, textureProperty);
                }
            }

            rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, textureProperty, Localization.GetLayout("mainTextureProperty"));
        }

        private void HandleScreenTypeChange(SerializedProperty screen, SerializedProperty screenType, SerializedProperty textureProperty)
        {
            if (screen.objectReferenceValue is Material)
            {
                screenType.intValue = (int)ScreenType.Material;
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

            if (screen.objectReferenceValue != null)
            {
                SetDefaultTextureProperty(screen.objectReferenceValue, textureProperty);
            }
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

        public void ApplyModifiedProperties()
        {
            try
            {
                _controllerSerializedObject?.ApplyModifiedProperties();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ControllerSettings: Failed to apply modified properties - {ex.Message}");
            }
        }
    }
}