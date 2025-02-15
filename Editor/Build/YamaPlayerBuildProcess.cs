using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

namespace Yamadev.YamaStream.Editor
{
    public interface IYamaPlayerBuildProcess : IOrderedCallback
    {
        public void Process();
    }

    internal class YamaPlayerBuildProcess : IProcessSceneWithReport
    {
        public int callbackOrder => -1000;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var processors = VersionManager.EditorAssembly.GetTypes()
                .Where(i => !i.IsInterface && !i.IsAbstract && i.GetInterfaces().Contains(typeof(IYamaPlayerBuildProcess)))
                .Select(i => Activator.CreateInstance(i) as IYamaPlayerBuildProcess)
                .OrderBy(i => i.callbackOrder);
            foreach (var processor in processors) processor.Process();
        }
    }

}