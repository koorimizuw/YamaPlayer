using UnityEngine;
using VRC.SDKBase;
using static VRC.SDKBase.VRCPlayerApi;

namespace Yamadev.YamaStream
{
  public static class TrackingUtils
  {
    private const float MAX_RAY_DISTANCE = 100f;

    public static Vector3 GetMousePosition(VRCPlayerApi player, TrackingDataType trackingDataType)
    {
      if (!Utilities.IsValid(player)) return Vector3.zero;

      TrackingData trackingData = player.GetTrackingData(trackingDataType);
      Vector3 origin = trackingData.position;
      Quaternion rotation = trackingData.rotation;

      if (player.IsUserInVR() && trackingDataType != TrackingDataType.Head)
      {
#if !UNITY_ANDROID
        Vector3 handPosOffset = new Vector3(0.0185f, 0f, 0.0506f);
        float handRotOffsetY = 40f;
#else
        Vector3 handPosOffset = new Vector3(0.0079f, 0f, 0.02125f);
        float handRotOffsetY = 45f;
#endif
        float scale = player.GetAvatarEyeHeightAsMeters();
        origin += rotation * handPosOffset * scale;
        rotation *= Quaternion.Euler(0f, handRotOffsetY, 0f);
      }

      return GetRayPoint(origin, rotation * Vector3.forward, MAX_RAY_DISTANCE);
    }

    private static Vector3 GetRayPoint(Vector3 origin, Vector3 direction, float maxDistance)
    {
      RaycastHit[] hitBuffer = new RaycastHit[32];
      int hitCount = Physics.RaycastNonAlloc(origin, direction, hitBuffer, maxDistance);

      float closestPhysicsDistance = float.MaxValue;
      Vector3 uiPoint = Vector3.zero;
      float uiDistance = float.MaxValue;

      for (int i = 0; i < hitCount; i++)
      {
        RaycastHit hit = hitBuffer[i];
        if (!Utilities.IsValid(hit.collider)) continue;

        if (!hit.collider.isTrigger && hit.distance < closestPhysicsDistance)
        {
          closestPhysicsDistance = hit.distance;
        }

        if (Utilities.IsValid(hit.collider.GetComponent<RectTransform>()) && Utilities.IsValid(hit.collider.GetComponent(typeof(VRC_UiShape))) && hit.distance < uiDistance)
        {
          uiDistance = hit.distance;
          uiPoint = hit.point;
        }
      }

      return closestPhysicsDistance < uiDistance ? Vector3.zero : uiPoint;
    }
  }
}