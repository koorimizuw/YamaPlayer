
using UdonSharp;
using UnityEngine;

namespace Yamadev.YamaStream
{
    [RequireComponent(typeof(Renderer))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class YamaPlayerScreen : UdonSharpBehaviour
    {
        [SerializeField] Controller _controller;
        void Start()
        {
            if (_controller == null) return;
            Renderer screen = GetComponent<Renderer>();
            _controller.RenderScreens = _controller.RenderScreens.Add(screen);
        }
    }
}