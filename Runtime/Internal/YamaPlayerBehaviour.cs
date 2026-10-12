using UnityEngine;
using UdonSharp;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public abstract class YamaPlayerBehaviour : UdonSharpBehaviour
    {
        protected const string DebugPrefix = "[<color=#EF6291>YamaStream</color>]";

        protected bool IsLocalPlayerValid => Utilities.IsValid(Networking.LocalPlayer);
        protected VRCPlayerApi LocalPlayer => IsLocalPlayerValid ? Networking.LocalPlayer : null;
        protected bool IsObjectOwner => IsLocalPlayerValid && Networking.IsOwner(gameObject);
        protected bool IsInstanceOwner => IsLocalPlayerValid && LocalPlayer.isInstanceOwner;
        protected bool IsMaster => IsLocalPlayerValid && LocalPlayer.isMaster;
        protected bool IsInVR => IsLocalPlayerValid && LocalPlayer.IsUserInVR();

        public virtual void TakeOwnership()
        {
            if (!IsLocalPlayerValid) return;
            if (!IsObjectOwner) Networking.SetOwner(LocalPlayer, gameObject);
        }

        public virtual void SyncVariables()
        {
            if (!IsLocalPlayerValid) return;
            TakeOwnership();
            RequestSerialization();
        }

        protected virtual void PrintLog(object message) => Debug.Log($"{DebugPrefix} {message}");

        protected virtual void PrintWarning(object message) => Debug.LogWarning($"{DebugPrefix} {message}");

        protected virtual void PrintError(object message) => Debug.LogError($"{DebugPrefix} {message}");
    }
}