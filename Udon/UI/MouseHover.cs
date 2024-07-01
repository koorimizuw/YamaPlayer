
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(Animator))]
    public class MouseHover : UdonSharpBehaviour
    {
        [SerializeField] InputController _inputController;
        [SerializeField] RectTransform _detectArea;
        [SerializeField] string _onPointerEnterTrigger;
        [SerializeField] string _onPointerExitTrigger;
        Animator _anim;
        Rect _detect;
        bool _inArea;
        bool _isVR;

        void Start()
        {
            _anim = GetComponent<Animator>();
            _isVR = Networking.LocalPlayer.IsUserInVR();
        }

        void Update()
        {
            if (_isVR) return;
            Vector3[] corners = new Vector3[4];

            _detectArea.GetWorldCorners(corners);
            _detect = corners[2].x - corners[0].x > 0 ? new Rect(corners[0], corners[2] - corners[0]) : new Rect(corners[3], corners[1] - corners[3]);
            if (_detect.Contains(_inputController.MousePosition))
            {
                if (!_inArea) _anim.SetTrigger(_onPointerEnterTrigger);
                _inArea = true;
                return;
            }
            if (_inArea) _anim.SetTrigger(_onPointerExitTrigger);
            _inArea = false;
        }
    }
}