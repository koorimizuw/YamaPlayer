using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

namespace Yamadev.YamaStream.Editor
{
    public interface IYamaPlayerBuildProcess : IOrderedCallback
    {
        void Process();
    }

    internal class YamaPlayerBuildProcess : IProcessSceneWithReport
    {
        public int callbackOrder => -100000;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var processors = typeof(YamaPlayerBuildProcess).Assembly.GetTypes()
                .Where(IsValidProcessorType)
                .Select(CreateProcessorInstance)
                .Where(processor => processor != null)
                .OrderBy(processor => processor.callbackOrder);
            ExecuteProcessors(processors);
        }

        private static bool IsValidProcessorType(Type type)
        {
            return !type.IsInterface
                   && !type.IsAbstract
                   && type.GetInterfaces().Contains(typeof(IYamaPlayerBuildProcess));
        }

        private static IYamaPlayerBuildProcess CreateProcessorInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type) as IYamaPlayerBuildProcess;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create instance of {type.Name}: {ex.Message}");
                return null;
            }
        }

        private static void ExecuteProcessors(IEnumerable<IYamaPlayerBuildProcess> processors)
        {
            foreach (var processor in processors)
            {
                try
                {
                    processor.Process();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error executing processor {processor.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}