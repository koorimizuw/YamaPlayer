
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InputController : UdonSharpBehaviour
    {
        Vector3 _mousePosition = Vector3.zero;
        bool _rightHand = true;

        void Update()
        {
            _mousePosition = Utils.GetMousePosition(Networking.LocalPlayer.IsUserInVR() ? _rightHand ? VRCPlayerApi.TrackingDataType.RightHand : VRCPlayerApi.TrackingDataType.LeftHand : VRCPlayerApi.TrackingDataType.Head);
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            if (args.handType == HandType.RIGHT) _rightHand = true;
            else if (args.handType == HandType.LEFT) _rightHand = false;
        }

        public override void InputGrab(bool value, UdonInputEventArgs args)
        {
            if (args.handType == HandType.RIGHT) _rightHand = true;
            else if (args.handType == HandType.LEFT) _rightHand = false;
        }

        public Vector3 MousePosition => _mousePosition;
    }
}