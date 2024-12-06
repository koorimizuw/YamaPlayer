
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

        public Vector3 GetMousePosition()
        {
            if (!Utilities.IsValid(Networking.LocalPlayer)) return Vector3.zero;
            bool isVr = Networking.LocalPlayer.IsUserInVR();
            TrackingDataType trackingDataType = isVr ? _rightHand ? TrackingDataType.RightHand : TrackingDataType.LeftHand : TrackingDataType.Head;
            TrackingData trackingData = Networking.LocalPlayer.GetTrackingData(trackingDataType);
            Quaternion rotation = trackingData.rotation;
            if (isVr) rotation *= Quaternion.Euler(0, 40f, 0);
            return GetRayPoint(trackingData.position, rotation * Vector3.forward);
        }

        public static Vector3 GetRayPoint(Vector3 origin, Vector3 direction)
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
            if (args.handType == HandType.RIGHT) _rightHand = true;
            else if (args.handType == HandType.LEFT) _rightHand = false;
        }

        public override void InputGrab(bool value, UdonInputEventArgs args)
        {
            if (args.handType == HandType.RIGHT) _rightHand = true;
            else if (args.handType == HandType.LEFT) _rightHand = false;
        }

        public Vector3 MousePosition => GetMousePosition();
    }
}