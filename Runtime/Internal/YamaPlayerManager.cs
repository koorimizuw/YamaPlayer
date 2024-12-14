
using UdonSharp;
using UnityEngine;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class YamaPlayerManager : YamaPlayerBehaviour
    {
        public string Version;
        public TextAsset Translation;
        public TextAsset UpdateLogs;
        public LatencyManager LatencyManager;
    }
}