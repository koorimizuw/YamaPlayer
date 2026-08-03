using UdonSharp;
using UnityEngine;
using VRC.Udon.Common;
using static VRC.SDKBase.VRCPlayerApi;

namespace Yamadev.YamaStream.UI
{
  [RequireComponent(typeof(RectTransform))]
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class SliderHelper : YamaPlayerBehaviour
  {
    [SerializeField] private RectTransform _tooltip;
    private RectTransform _rect;
    private float _percent = 0f;
    private bool _rightHand = true;

    private void Start()
    {
      _rect = GetComponent<RectTransform>();
    }

    public float Percent => _percent;

    public override void PostLateUpdate()
    {
      var trackingDataType = IsInVR ? (_rightHand ? TrackingDataType.RightHand : TrackingDataType.LeftHand) : TrackingDataType.Head;
      Vector3 mousePosition = TrackingUtils.GetMousePosition(LocalPlayer, trackingDataType);

      if (mousePosition == Vector3.zero)
      {
        _tooltip.gameObject.SetActive(false);
        return;
      }

      Vector3 localPosition = _rect.InverseTransformPoint(mousePosition);

      if (!_rect.rect.Contains(localPosition))
      {
        _tooltip.gameObject.SetActive(false);
        return;
      }

      float localX = localPosition.x + (_rect.rect.width * _rect.pivot.x);
      _percent = Mathf.Clamp01(localX / _rect.rect.width);

      _tooltip.gameObject.SetActive(true);
      Vector3 pos = _tooltip.localPosition;
      pos.x = localPosition.x;
      _tooltip.localPosition = pos;
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