using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;
using static VRC.SDKBase.VRCPlayerApi;

namespace Yamadev.YamaStream
{
    [AutoAssign]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InputController : UdonSharpBehaviour
    {
        private bool _rightHand = true;
        private bool _isVr = false;
        private bool _initialized = false;
        private VRCPlayerApi _localPlayer;

        private void Start() => Initialize();

        private void Initialize()
        {
            if (_initialized || !Utilities.IsValid(Networking.LocalPlayer)) return;
            _localPlayer = Networking.LocalPlayer;
            _isVr = _localPlayer.IsUserInVR();
            _initialized = true;
        }

        public TrackingDataType TrackingDataType
        {
            get
            {
                if (!_isVr) return TrackingDataType.Head;
                return _rightHand ? TrackingDataType.RightHand : TrackingDataType.LeftHand;
            }
        }

        public Vector3 GetMousePosition()
        {
            Initialize();
            if (!_initialized) return Vector3.zero;

            TrackingData trackingData = _localPlayer.GetTrackingData(TrackingDataType);
            Quaternion rotation = trackingData.rotation;
            if (_isVr) rotation *= Quaternion.Euler(0, 40f, 0);
            return GetRayPoint(trackingData.position, rotation * Vector3.forward);
        }

        private static Vector3 GetRayPoint(Vector3 origin, Vector3 direction)
        {
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, Mathf.Infinity);
            float distance = Mathf.Infinity;
            Vector3 uiPoint = Vector3.zero;
            float uiDistance = Mathf.Infinity;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null) continue;
                if (!hit.collider.isTrigger && hit.distance < distance) distance = hit.distance;
                if (hit.collider.GetComponent<RectTransform>() != null && hit.distance < uiDistance)
                {
                    uiDistance = hit.distance;
                    uiPoint = hit.point;
                }

            }
            return distance < uiDistance ? Vector3.zero : uiPoint;
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            _rightHand = args.handType == HandType.RIGHT;
        }

        public override void InputGrab(bool value, UdonInputEventArgs args)
        {
            _rightHand = args.handType == HandType.RIGHT;
        }
    }
}