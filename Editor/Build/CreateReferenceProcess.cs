using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Yamadev.YamaStream.Script;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Editor
{
    public class CreateReferenceProcess : IYamaPlayerBuildProcess
    {
        public int callbackOrder => -1;

        private const string TranslationFileGuid = "02e2b6ce10f26f94fb504aba7ccd2bfe";
        private const string UpdateLogFileGuid = "011c1aa791634cf45b66a6811ad47c8a";

        public void Process()
        {
            try
            {
                ProcessYamaPlayers();
                ProcessUIControllers();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in CreateReferenceProcess: {ex.Message}");
                throw;
            }
        }

        private static void ProcessYamaPlayers()
        {
            var yamaPlayers = Utils.FindComponentsInHierarthy<YamaPlayer>();

            foreach (var player in yamaPlayers)
            {
                if (player == null) continue;

                try
                {
                    ProcessYamaPlayer(player);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to process YamaPlayer {player.name}: {ex.Message}");
                }
            }
        }

        private static void ProcessYamaPlayer(YamaPlayer player)
        {
            var internalController = player.GetComponentInChildren<Controller>();
            if (internalController != null)
            {
                internalController.SetProgramVariable("_version", VersionManager.Version);
            }
            else
            {
                Debug.LogWarning($"Controller not found in YamaPlayer {player.name}");
            }

            var internalTransform = GetInternalTransform(player);
            if (internalTransform != null)
            {
                internalTransform.SetParent(null, true);
                GameObjectUtility.EnsureUniqueNameForSibling(internalTransform.gameObject);
            }
        }

        private static Transform GetInternalTransform(YamaPlayer player)
        {
            return player.Internal != null ? player.Internal : player.transform.Find("Internal");
        }

        private static void ProcessUIControllers()
        {
            var uiControllers = Utils.FindComponentsInHierarthy<UIController>();

            foreach (var uiController in uiControllers)
            {
                if (uiController == null) continue;

                try
                {
                    ProcessUIController(uiController);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to process UIController {uiController.name}: {ex.Message}");
                }
            }
        }

        private static void ProcessUIController(UIController uiController)
        {
            AssignTextAssets(uiController);
            ApplyUIColors(uiController);
            ApplyFontToTexts(uiController);
        }

        private static void AssignTextAssets(UIController uiController)
        {
            var updateLogFile = LoadAssetByGuid<TextAsset>(UpdateLogFileGuid);
            if (updateLogFile != null)
            {
                uiController.SetProgramVariable("_updateLogFile", updateLogFile);
            }
            else
            {
                Debug.LogWarning($"UpdateLog file not found with GUID: {UpdateLogFileGuid}");
            }

            var translationFile = LoadAssetByGuid<TextAsset>(TranslationFileGuid);
            if (translationFile != null)
            {
                uiController.SetProgramVariable("_translationTextFile", translationFile);
            }
            else
            {
                Debug.LogWarning($"Translation file not found with GUID: {TranslationFileGuid}");
            }
        }

        private static T LoadAssetByGuid<T>(string guid) where T : UnityEngine.Object
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(assetPath) ? null : AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        private static void ApplyUIColors(UIController uiController)
        {
            var uiColors = uiController.GetComponentsInChildren<UIColor>(true);

            foreach (var component in uiColors)
            {
                if (component == null) continue;
                component.UIController = uiController;
            }
        }

        private static void ApplyFontToTexts(UIController uiController)
        {
            var font = uiController.GetProgramVariable("_font") as Font;
            if (font == null) return;

            var texts = uiController.GetComponentsInChildren<Text>(true);

            foreach (var text in texts)
            {
                if (text != null)
                {
                    text.font = font;
                }
            }
        }
    }
}