
using System;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp;

namespace Yamadev.YamaStream.Script
{
    public class PlayListContainer : MonoBehaviour
    {
        [SerializeField]
        Transform targetContent;

        public Transform TargetContent => targetContent;
    }
}