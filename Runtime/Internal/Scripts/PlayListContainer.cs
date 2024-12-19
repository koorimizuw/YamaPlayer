
using UnityEngine;


namespace Yamadev.YamaStream.Script
{
    public class PlayListContainer : MonoBehaviour
    {
        [SerializeField] Transform targetContent;

        public Transform TargetContent => targetContent;
    }
}