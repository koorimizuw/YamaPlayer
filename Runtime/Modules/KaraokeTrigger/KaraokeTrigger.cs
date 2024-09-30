
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream.Modules
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(BoxCollider))]
    public class KaraokeTrigger : UdonSharpBehaviour
    {
        [SerializeField] Controller _controller;
        BoxCollider _boxCollider;

        void Start() => _boxCollider = GetComponent<BoxCollider>();

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (_controller == null ||
                !Networking.IsOwner(_controller.gameObject) ||
                _controller.KaraokeMode == KaraokeMode.None ||
                Array.IndexOf(_controller.KaraokeMembers, player.displayName) >= 0) return;
            _controller.KaraokeMembers = _controller.KaraokeMembers.Add(player.displayName);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (_controller == null ||
                !Networking.IsOwner(_controller.gameObject) ||
                _controller.KaraokeMode == KaraokeMode.None ||
                Array.IndexOf(_controller.KaraokeMembers, player.displayName) < 0) return;
            _controller.KaraokeMembers = _controller.KaraokeMembers.Remove(player.displayName);
        }
    }
}