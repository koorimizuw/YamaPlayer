
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;
using static VRC.SDKBase.VRCPlayerApi;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InputController : UdonSharpBehaviour
    {
        Vector3 _mousePosition = Vector3.zero;
        bool _rightHand = true;

        public override void PostLateUpdate()
        {
            if (!Utilities.IsValid(Networking.LocalPlayer)) return;
            TrackingDataType trackingData = Networking.LocalPlayer.IsUserInVR() ? _rightHand ? TrackingDataType.RightHand : TrackingDataType.LeftHand : TrackingDataType.Head;
            _mousePosition = GetMousePosition(trackingData);
        }

        public static Vector3 GetMousePosition(TrackingDataType type = TrackingDataType.Head)
        {
            if (!Utilities.IsValid(Networking.LocalPlayer)) return Vector3.zero;
            var tracking = Networking.LocalPlayer.GetTrackingData(type);
            Quaternion rot = tracking.rotation;
            if (type == TrackingDataType.LeftHand || type == TrackingDataType.RightHand) rot *= Quaternion.Euler(0, 40f, 0);
            RaycastHit[] hits = Physics.RaycastAll(tracking.position, rot * Vector3.forward, Mathf.Infinity);
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == null) continue;
                if (hit.collider != null && !hit.collider.isTrigger) return Vector3.zero;
                if (hit.collider.gameObject.GetComponent<RectTransform>() == null) continue;
                return hit.point;
            }
            return Vector3.zero;
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