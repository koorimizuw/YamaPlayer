using UdonSharp;
using UnityEngine;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class IndexTrigger : UdonSharpBehaviour
    {
        [SerializeField] private UdonSharpBehaviour _udon;
        [SerializeField] private string _varibaleName;
        [SerializeField] private string _varibaleValue;
        [SerializeField] private object _varibaleObject;
        [SerializeField] private string _eventName;

        public void OnButtonClick()
        {
            if (!string.IsNullOrEmpty(_varibaleValue))
            {
                _udon.SetProgramVariable(_varibaleName, _varibaleValue);
            }
            else
            {
                _udon.SetProgramVariable(_varibaleName, _varibaleObject);
            }

            _udon.SendCustomEvent(_eventName);
        }
    }
}