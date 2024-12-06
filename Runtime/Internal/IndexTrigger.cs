
using UdonSharp;
using UnityEngine;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class IndexTrigger : UdonSharpBehaviour
    {
        [SerializeField] UdonSharpBehaviour _udon;
        [SerializeField] string _varibaleName;
        [SerializeField] string _varibaleValue;
        [SerializeField] object _varibaleObject;
        [SerializeField] string _eventName;

        public void OnButtonClick()
        {
            if (!string.IsNullOrEmpty(_varibaleValue)) _udon.SetProgramVariable(_varibaleName, _varibaleValue);
            else _udon.SetProgramVariable(_varibaleName, _varibaleObject);
            _udon.SendCustomEvent(_eventName);
        }
    }
}