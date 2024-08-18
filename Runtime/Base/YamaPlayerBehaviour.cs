using UdonSharp;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public abstract class YamaPlayerBehaviour : UdonSharpBehaviour
    {
        protected VRCPlayerApi _localPlayer
        {
            get
            {
                if (Utilities.IsValid(Networking.LocalPlayer))
                    return Networking.LocalPlayer;
                return null;
            }
        }

        protected bool _isObjectOwner => _localPlayer != null && Networking.IsOwner(gameObject);

        protected bool _isInstanceOwner => _localPlayer != null && _localPlayer.isInstanceOwner;

        protected bool _isMaster => _localPlayer != null && _localPlayer.isMaster;

        protected bool isInVR => _localPlayer != null && _localPlayer.IsUserInVR();

        public virtual void TakeOwnership()
        {
            if (_localPlayer == null) return;
            if (!_isObjectOwner) Networking.SetOwner(_localPlayer, gameObject);
        }
    }
}