using UnityEngine;
using UdonSharp;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public abstract class YamaPlayerBehaviour : UdonSharpBehaviour
    {
        const string _debugPrefix = "[<color=#EF6291>YamaStream</color>]";

        protected bool _isLocalPlayerValid => Utilities.IsValid(Networking.LocalPlayer);

        protected VRCPlayerApi _localPlayer
        {
            get
            {
                if (_isLocalPlayerValid) return Networking.LocalPlayer;
                return null;
            }
        }

        protected bool _isObjectOwner => _localPlayer != null && Networking.IsOwner(gameObject);

        protected bool _isInstanceOwner => _localPlayer != null && _localPlayer.isInstanceOwner;

        protected bool _isMaster => _localPlayer != null && _localPlayer.isMaster;

        protected bool _isInVR => _localPlayer != null && _localPlayer.IsUserInVR();

        public void TakeOwnership()
        {
            if (!_isLocalPlayerValid) return;
            if (!_isObjectOwner) Networking.SetOwner(_localPlayer, gameObject);
        }

        public void SyncVariables()
        {
            if (!_isLocalPlayerValid) return;
            TakeOwnership();
            RequestSerialization();
        }

        public void PrintLog(object message) => Debug.Log($"{_debugPrefix} {message}");

        public void PrintWarning(object message) => Debug.LogWarning($"{_debugPrefix} {message}");

        public void PrintError(object message) => Debug.LogError($"{_debugPrefix} {message}");
    }
}