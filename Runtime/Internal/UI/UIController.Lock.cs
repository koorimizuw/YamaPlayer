using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common;
using static VRC.SDKBase.VRCPlayerApi;

namespace Yamadev.YamaStream.UI
{
  public partial class UIController
  {
    private const int DEFAULT_LAYER = 0; // こっちはビームが出る
    private const int UI_LAYER = 5; // こっちはビームが出ない、ロック時はこっち

    [Header("UI Lock - Components")]
    [SerializeField] private RectTransform _lockDetectionRect;
    [SerializeField] private Text _unlockProgressText;
    [SerializeField] private float _unlockHoldDuration = 0.6f;

    private bool _locked = false;
    private bool _lastInput = false;
    private float _lastInputTime = 0f;
    private bool _pointerInside = false;
    private bool _rightHand = true;

    public override void PostLateUpdate()
    {
      if (!Utilities.IsValid(_lockDetectionRect) || !_locked) return;

      var trackingDataType = IsInVR ? (_rightHand ? TrackingDataType.RightHand : TrackingDataType.LeftHand) : TrackingDataType.Head;
      Vector3 mousePosition = TrackingUtils.GetMousePosition(LocalPlayer, trackingDataType);

      if (mousePosition == Vector3.zero)
      {
        if (_pointerInside)
        {
          _systemUIAnimator.SetTrigger("HideLockMessage");
          _pointerInside = false;
        }
        _unlockProgressText.text = "";
        return;
      }

      Vector3 localPosition = _lockDetectionRect.InverseTransformPoint(mousePosition);

      bool pointerInside = _lockDetectionRect.rect.Contains(localPosition);
      if (pointerInside != _pointerInside)
      {
        if (pointerInside) _systemUIAnimator.SetTrigger("ShowLockMessage");
        else _systemUIAnimator.SetTrigger("HideLockMessage");
        _pointerInside = pointerInside;
      }

      if (!pointerInside)
      {
        _unlockProgressText.text = "";
        return;
      }

      if (_lastInputTime != 0f)
      {
        _unlockProgressText.text = $"{Mathf.Clamp01((Time.time - _lastInputTime) / _unlockHoldDuration) * 100f:0}%";

        if (Time.time - _lastInputTime >= _unlockHoldDuration)
        {
          UnlockUI();
          _unlockProgressText.text = "";
          _pointerInside = false;
          _systemUIAnimator.SetTrigger("HideLockMessage");
        }
      }

      if (!_lastInput)
      {
        _unlockProgressText.text = "";
      }
    }

    public void LockUI()
    {
      if (!Utilities.IsValid(_lockDetectionRect) || !Utilities.IsValid(_userUIAnimator)) return;
      if (!Utilities.IsValid(_systemUIAnimator) || !Utilities.IsValid(_unlockProgressText)) return;
      _lockDetectionRect.gameObject.layer = UI_LAYER;
      _userUIAnimator.SetTrigger("ToggleUI");
      _locked = true;
    }

    public void UnlockUI()
    {
      if (!Utilities.IsValid(_lockDetectionRect) || !Utilities.IsValid(_userUIAnimator)) return;
      _lockDetectionRect.gameObject.layer = DEFAULT_LAYER;
      _userUIAnimator.SetTrigger("ToggleUI");
      _locked = false;
    }

    public override void InputUse(bool value, UdonInputEventArgs args)
    {
      _lastInput = value;
      _rightHand = args.handType == HandType.RIGHT;

      if (value)
      {
        _lastInputTime = Time.time;
      }
      else
      {
        _lastInputTime = 0f;
      }
    }

    public override void InputGrab(bool value, UdonInputEventArgs args)
    {
      _rightHand = args.handType == HandType.RIGHT;
    }
  }
}